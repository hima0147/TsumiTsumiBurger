using UnityEngine;
using UnityEngine.SceneManagement; // シーン移動に必要

public class TitleManager : MonoBehaviour
{
    // ボタンから呼び出す
    public void OnStartGame()
    {
        // "SampleScene" という名前は、メインゲームのシーン名に合わせてください
        SceneManager.LoadScene("SampleScene");
    }
}