using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public string sceneName;

    void OnSubmit(InputValue value)
    {
        Load();
    }

    void Start()
    {
        scoreText.text = GameManager.totalScore.ToString();
    }

    void Update()
    {
        
    }

    public void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}
