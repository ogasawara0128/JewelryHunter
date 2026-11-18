using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;  // シーンの切り替えに必要

public class TitleManager : MonoBehaviour
{
    public string sceneName;            // 読み込むシーン名

    void OnSubmit(InputValue value)
    {
        Load();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {

    }

    // シーンを読み込む
    public void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}
