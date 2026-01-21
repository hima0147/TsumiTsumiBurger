using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
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
    [SerializeField] private GameObject continuePanel;

    [Header("ゲーム設定")]
    [SerializeField] private float gameTime = 30.0f;

    private int score = 0;
    private float currentTimer;
    private bool isPlaying = false;
    private bool isLastOrder = false;

    // ★追加：コンティニューを使ったかどうか
    private bool hasContinued = false;

    // 補充待ちリスト
    private bool[] pendingRefills;

    public bool IsPlaying => isPlaying;
    public bool IsLastOrder => isLastOrder;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (isPlaying)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerText();

            if (currentTimer <= 0)
            {
                OnTimeUp();
            }
        }
    }

    public void StartGame()
    {
        score = 0;
        currentTimer = gameTime;
        isPlaying = true;
        isLastOrder = false;
        hasContinued = false; // ★リセットする

        if (laneXPositions != null)
        {
            pendingRefills = new bool[laneXPositions.Length];
        }

        UpdateScoreText();
        UpdateTimerText();

        if (resultPanel != null) resultPanel.SetActive(false);
        if (continuePanel != null) continuePanel.SetActive(false);

        // 初期配置
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

        // ラストオーダー中ならメモする
        if (isLastOrder)
        {
            if (pendingRefills != null) pendingRefills[laneIndex] = true;
            return;
        }

        Vector3 spawnPos = new Vector3(laneXPositions[laneIndex], bottomYPosition, 0);
        Instantiate(bottomBunPrefab, spawnPos, Quaternion.identity);
    }

    private void OnTimeUp()
    {
        isPlaying = false;
        isLastOrder = true;

        // ★変更：コンティニュー画面を出すのは「まだコンティニューしてない時」だけ
        if (!hasContinued && continuePanel != null)
        {
            continuePanel.SetActive(true);
        }
        else
        {
            // 2回目以降、またはパネルがない場合は即終了
            EndGame();
        }
    }

    public void OnClickWatchAd()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardAd(() =>
            {
                ResumeGameWithBonus();
            });
        }
        else
        {
            Debug.LogWarning("AdsManagerテスト：そのまま復活させます。");
            ResumeGameWithBonus();
        }
    }

    private void ResumeGameWithBonus()
    {
        currentTimer = 10.0f;
        isPlaying = true;
        isLastOrder = false;
        hasContinued = true; // ★「もう使ったよ」と記録する

        if (continuePanel != null) continuePanel.SetActive(false);

        // 待機中のバンズを復活
        if (pendingRefills != null)
        {
            for (int i = 0; i < pendingRefills.Length; i++)
            {
                if (pendingRefills[i])
                {
                    RefillLane(i);
                    pendingRefills[i] = false;
                }
            }
        }
    }

    public void OnClickNoThanks()
    {
        if (continuePanel != null) continuePanel.SetActive(false);
        EndGame();
    }

    private void EndGame()
    {
        isPlaying = false;
        isLastOrder = true;

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);

            string comment = "";
            if (score >= 2000) comment = "すごい！ ハンバーガーの神様！";
            else if (score >= 1000) comment = "ナイス！ 達人級の腕前！";
            else if (score >= 500) comment = "おいしそう！ いい感じ！";
            else comment = "次はもっと積めるはず！";

            if (resultText != null)
            {
                resultText.text = $"スコア: {score}\n{comment}";
            }
        }
    }

    public void AddScore(int amount)
    {
        if (!isPlaying && !isLastOrder) return;
        score += amount;
        UpdateScoreText();
    }

    public void AddScore(int amount, List<GameObject> ingredients)
    {
        AddScore(amount);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            float displayTime = Mathf.Max(0, currentTimer);
            timerText.text = "Time: " + displayTime.ToString("F1");

            if (displayTime <= 5.0f) timerText.color = Color.red;
            else timerText.color = Color.white;
        }
    }

    public void OnReturnTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}