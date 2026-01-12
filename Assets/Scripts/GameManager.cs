using UnityEngine;
using UnityEngine.UI; // UI画像操作に必要
using TMPro;
using UnityEngine.SceneManagement; // シーン読み込みに必要
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("設定")]
    [SerializeField] private GameObject bottomBunPrefab;
    [SerializeField] private float[] laneXPositions;
    [SerializeField] private float bottomYPosition = -3.5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("リザルト画面設定")]
    [SerializeField] private GameObject resultPanel; // リザルト画面全体
    [SerializeField] private TextMeshProUGUI resultText; // 個数表示
    [SerializeField] private Transform bestBurgerContainer; // バーガーを積む場所
    [SerializeField] private Button returnButton; // 戻るボタン

    private int score = 0;
    private int burgerCount = 0; // 作った個数
    private float timeLimit = 30.0f;

    private bool isLastOrder = false; // ラストオーダー中か？
    // 他のスクリプトから「ラストオーダー中？」と聞けるようにする窓口
    public bool IsLastOrder => isLastOrder;
    private bool isGameOver = false;  // 完全に終わったか？

    // 現在画面内にある下バンズの数（これが0になったらゲーム終了）
    private int activeBottomBuns = 0;

    // 一番背の高かったバーガーの画像リスト
    private List<Sprite> bestBurgerSprites = new List<Sprite>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // リザルト画面を隠す
        if (resultPanel != null) resultPanel.SetActive(false);

        // ボタンに機能を登録
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(OnReturnTitle);
        }

        // 下バンズ配置
        for (int i = 0; i < 4; i++)
        {
            SpawnBottomBun(i);
        }
        UpdateUI();
    }

    private void Update()
    {
        if (isGameOver) return;

        // タイマー処理
        if (timeLimit > 0)
        {
            timeLimit -= Time.deltaTime;
            if (timeLimit <= 0)
            {
                timeLimit = 0;
                StartLastOrder(); // 時間切れ＝ラストオーダー開始
            }
        }

        // UI表示更新
        timerText.text = Mathf.Ceil(timeLimit).ToString();

        // ラストオーダー表示（文字を赤くするなど）
        if (isLastOrder)
        {
            timerText.color = Color.red;
            timerText.text = "LAST!";
        }
    }

    // ラストオーダー開始
    private void StartLastOrder()
    {
        isLastOrder = true;
        Debug.Log("ラストオーダー！補充ストップ！");

        // もしこの時点で下バンズが1つもなければ即終了
        if (activeBottomBuns <= 0)
        {
            FinishGame();
        }
    }

    public void AddScore(int points, List<GameObject> parts)
    {
        if (isGameOver) return;

        score += points;
        burgerCount++; // 個数を増やす

        // 一番高いバーガーかチェックして記録
        SaveBestBurger(parts);

        UpdateUI();
    }

    // 一番高いバーガーなら画像を保存しておく
    private void SaveBestBurger(List<GameObject> parts)
    {
        // 今回の高さ（パーツ数）が、記録より多かったら更新
        if (parts.Count > bestBurgerSprites.Count)
        {
            bestBurgerSprites.Clear();

            // 下から順に画像を保存（BottomBun -> ... -> TopBun）
            // partsリストはBurgerJudgeで上から順に追加されているので、逆順にする
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                SpriteRenderer sr = parts[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    bestBurgerSprites.Add(sr.sprite);
                }
            }
        }
    }

    private void UpdateUI()
    {
        scoreText.text = "SCORE: " + score.ToString();
    }

    public void SpawnBottomBun(int laneIndex)
    {
        Vector3 spawnPos = new Vector3(laneXPositions[laneIndex], bottomYPosition, 0);
        GameObject bun = Instantiate(bottomBunPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = bun.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        activeBottomBuns++; // バンズ数をカウント
    }

    public void RefillLane(int laneIndex)
    {
        activeBottomBuns--; // 1つ完成したので減らす

        // ラストオーダー中は補充しない！
        if (isLastOrder)
        {
            // 全てのバンズが無くなったらゲーム終了
            if (activeBottomBuns <= 0)
            {
                FinishGame();
            }
            return;
        }

        StartCoroutine(RefillCoroutine(laneIndex));
    }

    private System.Collections.IEnumerator RefillCoroutine(int laneIndex)
    {
        yield return new WaitForSeconds(1.0f);
        SpawnBottomBun(laneIndex);
    }

    // 本当のゲーム終了
    private void FinishGame()
    {
        isGameOver = true;
        Debug.Log("全オーダー終了！リザルトへ");
        ShowResult();
    }

    private void ShowResult()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultText.text = burgerCount + "こ できたよ！";

            // 一番大きかったバーガーをUIで再現
            // リストを逆順（上から下の順）にループして表示していく
            for (int i = bestBurgerSprites.Count - 1; i >= 0; i--)
            {
                Sprite sprite = bestBurgerSprites[i];

                // 新しい画像オブジェクトを作る
                GameObject imgObj = new GameObject("BurgerPart");
                // 第2引数を false にすることで、余計な位置ズレを防ぐ
                imgObj.transform.SetParent(bestBurgerContainer, false);

                // Imageコンポーネントをつけて画像を表示
                Image img = imgObj.AddComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true; // 縦横比を維持
            }
        }
    }

    // タイトルへ戻る（今はシーンをリロード）
    public void OnReturnTitle()
    {
        // 「TitleScene」という名前のシーンへ移動する
        SceneManager.LoadScene("TitleScene");
    }
}