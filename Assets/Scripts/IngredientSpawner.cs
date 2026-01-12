using UnityEngine;
using UnityEngine.UI; // 画像操作に必要

public class IngredientSpawner : MonoBehaviour
{
    [Header("具材リスト")]
    [SerializeField] private GameObject topBunPrefab;
    [SerializeField] private GameObject[] ingredientPrefabs;

    [Header("UI設定")]
    [SerializeField] private Image nextImageUI; // 次の具材を表示するUI

    [Header("設定")]
    [SerializeField] private float spawnYPosition = 4.5f;
    [SerializeField] private float[] laneXPositions;

    private int sortingOrderCounter = 10;

    // 「次に落とす予定」のプレハブを覚えておく変数
    private GameObject nextPrefab;

    private void Start()
    {
        // ゲーム開始時に「最初の1個」と「次の1個」を決める
        DecideNextItem(); // 最初の分
        // UI更新のために、もう一度呼んで「次の分」を準備する
        PrepareNextSpawn();
    }

    // 次に何を出すか決めて、UIを更新する
    private void PrepareNextSpawn()
    {
        DecideNextItem();

        // UIの画像を更新
        if (nextImageUI != null && nextPrefab != null)
        {
            // プレハブについているSpriteRendererから画像をもらう
            SpriteRenderer sr = nextPrefab.GetComponent<SpriteRenderer>();
            nextImageUI.sprite = sr.sprite;

            // 色も白に戻す（元の画像通りに表示）
            nextImageUI.color = Color.white;
        }
    }

    // ランダムにプレハブを選ぶだけの処理
    private void DecideNextItem()
    {
        int randomValue = Random.Range(0, 10);
        if (randomValue < 3)
        {
            nextPrefab = topBunPrefab;
        }
        else
        {
            int ingredientIndex = Random.Range(0, ingredientPrefabs.Length);
            nextPrefab = ingredientPrefabs[ingredientIndex];
        }
    }

    public void SpawnItem(int laneIndex)
    {
        // ラストオーダー中の「具材捨て」防止チェック
        if (GameManager.Instance.IsLastOrder)
        {
            // そのレーンの上空から下に向かってレーザー（Raycast）を飛ばす
            Vector2 checkPos = new Vector2(laneXPositions[laneIndex], spawnYPosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(checkPos, Vector2.down, 20.0f);

            bool hasTarget = false;
            foreach (var hit in hits)
            {
                // 「下バンズ」「具材」「上バンズ（積み途中）」のどれかがあればOK
                string t = hit.collider.tag;
                if (t == "BottomBun" || t == "Ingredient" || t == "TopBun")
                {
                    hasTarget = true;
                    break;
                }
            }

            // 何もなければ（＝空きレーンなら）、落とさずに処理を終わる
            if (!hasTarget) return;
        }
        if (nextPrefab == null) return;

        // 1. 準備しておいた「nextPrefab」を生成
        Vector3 spawnPos = new Vector3(laneXPositions[laneIndex], spawnYPosition, 0);
        GameObject newItem = Instantiate(nextPrefab, spawnPos, Quaternion.identity);

        // 2. 描画順の設定（手前に表示）
        SpriteRenderer sr = newItem.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = sortingOrderCounter;
            sortingOrderCounter++;
        }

        // 3. 次の具材を準備してUIを更新
        PrepareNextSpawn();
    }
}