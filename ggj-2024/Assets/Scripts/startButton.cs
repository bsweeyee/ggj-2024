using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [SerializeField] private string nextLevel;
    public void StartLevel()
    {
        SceneManager.LoadScene(nextLevel, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}


