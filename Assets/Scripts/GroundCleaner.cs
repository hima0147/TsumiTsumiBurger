using UnityEngine;

public class GroundCleaner : MonoBehaviour
{
    // 何かがぶつかった時に呼ばれる
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ぶつかった相手のタグを確認
        string tag = collision.gameObject.tag;

        // 「具材」または「上バンズ」だったら消す
        if (tag == "Ingredient" || tag == "TopBun")
        {
            Destroy(collision.gameObject);
            // ここに将来「失敗サウンド」などを入れると良いです
            Debug.Log("地面に落ちた具材を消去しました");
        }
        // ※BottomBunは消さないので、万が一落ちても大丈夫
    }
}