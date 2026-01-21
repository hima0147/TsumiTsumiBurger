using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private IngredientSpawner spawner;

    // 画面幅を4分割した境界線（0.25, 0.5, 0.75）
    // 左から 0, 1, 2, 3 のレーン番号を返す

    private void Update()
    {
        // ゲーム中じゃなければ、タッチ入力を無視して何もしない
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        // マウス左クリック（スマホならタップ）が離された瞬間
        if (Input.GetMouseButtonUp(0))
        {
            HandleInput(Input.mousePosition);
        }
    }

    private void HandleInput(Vector3 screenPosition)
    {
        // 画面の幅に対する割合 (0.0 〜 1.0) を取得
        float normalizedX = screenPosition.x / Screen.width;

        int laneIndex = 0;

        if (normalizedX < 0.25f) laneIndex = 0;      // 左端
        else if (normalizedX < 0.5f) laneIndex = 1;  // 左中央
        else if (normalizedX < 0.75f) laneIndex = 2; // 右中央
        else laneIndex = 3;                          // 右端

        // Spawnerに「このレーンに落として！」と依頼
        spawner.SpawnItem(laneIndex);
    }
}