using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private WinUI winUI;       


    private Game game;
    public void Initialize(Game game) {
        this.game = game;                
        for(var i=0; i<transform.childCount; i++) {
            Debug.Log(transform.GetChild(i).name);
        }
        // gameOverUI = transform.GetComponentInChildren<GameOverUI>();
        // winUI = transform.GetComponentInChildren<WinUI>();

        gameOverUI.Initialize(game);
        winUI.Initialize(game);

        Reset();

        DontDestroyOnLoad(this);       
    }

    public void Win() {
        winUI.gameObject.SetActive(true);
    }

    public void GameOver() {
        gameOverUI.gameObject.SetActive(true);
        gameOverUI.SetScore(game.CurrentScore, game.CurrentTargetScore);
    }

    public void Reset() {        
        gameOverUI.gameObject.SetActive(false);
        winUI.gameObject.SetActive(false);
    }    
}
