using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;


public class GameOverScreen : MonoBehaviour

{
    public GameOverScreen gameOverText;
    void Start()
    {
        gameOverText.gameObject.SetActive(true); 
    }
    public void GameOver() 
    {
        gameOverText.gameObject.SetActive(true); 
    }
}
