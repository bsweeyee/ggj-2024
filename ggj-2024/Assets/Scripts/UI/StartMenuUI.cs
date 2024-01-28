using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private int nextLevel;
    public void StartLevel()
    {
        Debug.Log(nextLevel);
        SceneManager.LoadScene(nextLevel, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}


