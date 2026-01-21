using UnityEngine;
using GoogleMobileAds.Api; // 広告機能を使うためのおまじない
using System;

public class AdsManager : MonoBehaviour
{
    // どこからでも呼び出せるようにする（シングルトン）
    public static AdsManager Instance;

    [Header("広告ID設定")]
    // ↓今は「テスト用ID」を入れています。リリースの時に「本番のID」に書き換えます。
    [SerializeField] private string androidRewardUnitId = "ca-app-pub-3940256099942544/5224354917";

    private RewardedAd _rewardedAd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン移動しても消えないようにする
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 1. Google Mobile Ads SDKを初期化する
        MobileAds.Initialize(initStatus => {
            Debug.Log("AdMob初期化完了！");
            // 初期化が終わったら、さっそく次の広告を読み込んでおく
            LoadRewardAd();
        });
    }

    // ▼ 広告を読み込む機能
    public void LoadRewardAd()
    {
        // 既に読み込み済みなら何もしない
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("リワード広告を読み込み中...");

        // 広告の読み込みリクエストを作成
        var adRequest = new AdRequest();

        // 広告をロード
        RewardedAd.Load(androidRewardUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // エラーが起きた場合
                if (error != null || ad == null)
                {
                    Debug.LogError("広告の読み込み失敗: " + error);
                    return;
                }

                Debug.Log("リワード広告の読み込み成功！");
                _rewardedAd = ad;

                // 広告を見終わった後のイベント登録（ここで閉じられた時の処理などを登録できる）
                RegisterEventHandlers(ad);
            });
    }

    // ▼ 広告を表示する機能
    // 引数で「見終わった後に実行したい処理（action）」を受け取れるようにしています
    public void ShowRewardAd(Action onRewardEarned)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                // ここは「報酬を獲得した瞬間」に呼ばれる
                Debug.Log("報酬獲得！");

                // 渡された「復活処理」を実行する
                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.Log("まだ広告の準備ができていません。");
            // 広告がない場合は、読み込み直しておく
            LoadRewardAd();
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // 広告を閉じた時に、次の広告を読み込んでおく
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("広告が閉じられました。次をロードします。");
            LoadRewardAd();
        };

        // エラーで表示できなかった場合も次をロード
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("広告表示エラー: " + error);
            LoadRewardAd();
        };
    }
}