using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BurgerJudge : MonoBehaviour
{
    // 爆発エフェクトのプレハブ
    [Header("設定")]
    [SerializeField] private GameObject explosionPrefab;

    private bool isJudged = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isJudged) return;

        // 具材か下のバンズに当たった時だけ判定
        if (!collision.gameObject.CompareTag("Ingredient") &&
            !collision.gameObject.CompareTag("BottomBun")) return;

        CheckBurger();
    }

    private void CheckBurger()
    {
        Vector2 startPos = new Vector2(transform.position.x, transform.position.y - 0.1f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, Vector2.down, 10.0f);

        bool hasBottomBun = false;
        List<GameObject> burgerParts = new List<GameObject>();

        // 自分（上のバンズ）をリストに追加
        burgerParts.Add(this.gameObject);

        foreach (RaycastHit2D hit in hits)
        {
            GameObject target = hit.collider.gameObject;

            if (target == this.gameObject) continue;

            if (target.CompareTag("Ingredient"))
            {
                burgerParts.Add(target);
            }
            else if (target.CompareTag("BottomBun"))
            {
                hasBottomBun = true;
                burgerParts.Add(target);
                break;
            }
            // 別の上のバンズに当たったら中断
            else if (target.CompareTag("TopBun"))
            {
                break;
            }
        }

        if (hasBottomBun)
        {
            CompleteBurger(burgerParts);
        }
    }

    private void CompleteBurger(List<GameObject> parts)
    {
        isJudged = true;
        GameManager.Instance.AddScore(100 * parts.Count, parts);

        // ★★★ ここに追加！ ★★★
        // クローン作成用に、GameManagerへパーツリストを渡す
        // （GameManager側で少し修正が必要です。後述します）
        GameManager.Instance.CheckRecordAndCloneList(parts);

        // アニメーションして消去
        StartCoroutine(AnimateAndDestroy(parts));
    }

    private IEnumerator AnimateAndDestroy(List<GameObject> parts)
    {
        // 1. 物理演算を止めて「食べる」演出の準備
        foreach (GameObject part in parts)
        {
            if (part == null) continue;

            Collider2D col = part.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Rigidbody2D rb = part.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            SpriteRenderer sr = part.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1f, 1f, 0.5f, 1f); // 黄色っぽく発光
        }

        // 2. 派手な爆発エフェクト
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // 3. 一瞬止まる（余韻）
        yield return new WaitForSeconds(0.5f);

        // 4. スーッと消える
        float fadeDuration = 0.5f;
        float currentTime = 0f;

        Color startColor = new Color(1f, 1f, 0.5f, 1f);
        Color endColor = new Color(1f, 1f, 0.5f, 0f);

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / fadeDuration;

            foreach (GameObject part in parts)
            {
                if (part == null) continue;
                SpriteRenderer sr = part.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.Lerp(startColor, endColor, t);
                }
            }
            yield return null;
        }

        // 5. 削除と補充
        // レーン判定のためにX座標を使う（パーツが全部同じX座標なのでどれでもいい）
        float xPos = transform.position.x;
        int laneIndex = GetLaneIndexFromX(xPos);

        foreach (GameObject part in parts)
        {
            Destroy(part);
        }
        GameManager.Instance.RefillLane(laneIndex);
    }

    private int GetLaneIndexFromX(float xPos)
    {
        if (xPos < -1.35f) return 0;
        if (xPos < 0f) return 1;
        if (xPos < 1.35f) return 2;
        return 3;
    }
}