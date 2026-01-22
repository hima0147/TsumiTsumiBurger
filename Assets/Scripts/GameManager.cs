using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("設定")]
    [SerializeField] private GameObject bottomBunPrefab;
    [SerializeField] private float[] laneXPositions;
    [SerializeField] private float bottomYPosition = -3.5f;

    [Header("UI設定")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("ゲーム設定")]
    [SerializeField] private float gameTime = 60.0f;

    // チャンピオンを一時的に隠しておく場所（画面外）
    private Vector3 hidingSpot = new Vector3(500f, 500f, 0f);
    // 最後にチャンピオンをお披露目する場所（画面中央の下の方）
    private Vector3 showRoomPos = new Vector3(0f, -2.5f, 0f);

    private int score = 0;
    private float currentTimer;
    private bool isPlaying = false;
    private bool isLastOrder = false;

    // 記録保持用
    private GameObject currentChampion; // 現在の1位のクローン（親オブジェクト）
    private int maxStackRecord = -1;    // 現在の最高段数

    public bool IsPlaying => isPlaying;
    public bool IsLastOrder => isLastOrder;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // Canvasがカメラを見失わないように再設定する
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceCamera && c.worldCamera == null)
            {
                c.worldCamera = Camera.main;
            }
        }
        StartGame();
    }

    private void Update()
    {
        if (isPlaying)
        {
            if (!isLastOrder)
            {
                currentTimer -= Time.deltaTime;
                UpdateTimerText();

                if (currentTimer <= 0)
                {
                    OnTimeUp();
                }
            }
            else
            {
                // ラストオーダー中：画面内のプレイ用バンズがなくなるのを待つ
                GameObject[] remainingBuns = GameObject.FindGameObjectsWithTag("BottomBun");
                if (remainingBuns.Length == 0)
                {
                    EndGame();
                }
            }
        }
    }

    // BurgerJudgeから呼ばれる関数
    // リストを受け取ってクローンを作る関数
    public void CheckRecordAndCloneList(List<GameObject> parts)
    {
        int stackCount = parts.Count;

        if (stackCount >= maxStackRecord)
        {
            maxStackRecord = stackCount;

            // 前のチャンピオンがいれば消す
            if (currentChampion != null)
            {
                Destroy(currentChampion);
            }

            // 基準点（下のバンズ）を探す
            GameObject basePart = null;
            foreach (var p in parts)
            {
                if (p.CompareTag("BottomBun"))
                {
                    basePart = p;
                    break;
                }
            }
            if (basePart == null && parts.Count > 0) basePart = parts[0];

            Vector3 anchorPos = basePart.transform.position;

            // 親オブジェクト作成
            currentChampion = new GameObject("ChampionBurger");
            currentChampion.transform.position = hidingSpot;

            foreach (GameObject originalPart in parts)
            {
                if (originalPart == null) continue;

                GameObject clonePart = Instantiate(originalPart, currentChampion.transform);

                // 位置合わせ
                Vector3 relativePos = originalPart.transform.position - anchorPos;
                clonePart.transform.localPosition = relativePos;

                // ★修正箇所：SpriteRendererの処理を1回にまとめました
                SpriteRenderer sr = clonePart.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 5;     // 表示順を「5」に（背景と文字の間）
                    sr.color = Color.white;  // 色を白に戻す
                }

                // 不要なコンポーネントの削除
                clonePart.tag = "Untagged";

                var rb = clonePart.GetComponent<Rigidbody2D>();
                if (rb != null) Destroy(rb);

                var col = clonePart.GetComponent<Collider2D>();
                if (col != null) Destroy(col);

                var judge = clonePart.GetComponent<BurgerJudge>();
                if (judge != null) Destroy(judge);
            }
        }
    }

    public void StartGame()
    {
        score = 0;
        currentTimer = gameTime;
        isPlaying = true;
        isLastOrder = false;

        // 記録リセット
        maxStackRecord = -1;
        if (currentChampion != null) Destroy(currentChampion);

        UpdateScoreText();
        UpdateTimerText();
        if (resultPanel != null) resultPanel.SetActive(false);
        if (timerText != null) timerText.color = Color.white;

        if (laneXPositions != null && bottomBunPrefab != null)
        {
            for (int i = 0; i < laneXPositions.Length; i++)
            {
                RefillLane(i);
            }
        }
    }

    public void RefillLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= laneXPositions.Length) return;
        if (isLastOrder) return;

        Vector3 spawnPos = new Vector3(laneXPositions[laneIndex], bottomYPosition, 0);
        Instantiate(bottomBunPrefab, spawnPos, Quaternion.identity);
    }

    private void OnTimeUp()
    {
        isLastOrder = true;
        if (timerText != null)
        {
            timerText.text = "FINISH!!";
            timerText.color = Color.red;
        }
    }

    private void EndGame()
    {
        isPlaying = false;
        isLastOrder = true;

        // ★ここでチャンピオンをお披露目！
        ShowChampionBurger();

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);

            string comment = "";
            if (score >= 2000) comment = "ハンバーガーの神様！";
            else if (score >= 1000) comment = "達人級の腕前！";
            else comment = "次はもっと積める！";

            if (resultText != null)
            {
                resultText.text = $"スコア: {score}\n{comment}";
            }
        }
    }

    private void ShowChampionBurger()
    {
        // 1. 画面に残っているゴミ（作りかけのバンズなど）を掃除する
        // "BottomBun"タグがついているものを全部消す
        GameObject[] leftovers = GameObject.FindGameObjectsWithTag("BottomBun");
        foreach (var obj in leftovers)
        {
            Destroy(obj);
        }

        // 2. 隠しておいたチャンピオンを、画面中央（ショールーム）に移動させる
        if (currentChampion != null)
        {
            currentChampion.transform.position = showRoomPos;

            // カメラのズーム調整（高すぎるなら引く）
            if (Camera.main != null)
            {
                // 基本サイズ5 + 段数 * 0.2
                // カメラの位置は動かさない！（背景が消えるから）
                Camera.main.orthographicSize = 5.0f + (maxStackRecord * 0.2f);
            }
        }
    }

    public void AddScore(int amount) { score += amount; UpdateScoreText(); }
    public void AddScore(int amount, List<GameObject> ingredients) { AddScore(amount); }
    private void UpdateScoreText() { if (scoreText != null) scoreText.text = "Score: " + score; }
    private void UpdateTimerText()
    {
        if (isLastOrder) return;
        if (timerText != null)
        {
            float displayTime = Mathf.Max(0, currentTimer);
            timerText.text = "Time: " + displayTime.ToString("F1");
            if (displayTime <= 5.0f) timerText.color = Color.red;
        }
    }
    public void OnReturnTitle() { SceneManager.LoadScene("TitleScene"); }
}