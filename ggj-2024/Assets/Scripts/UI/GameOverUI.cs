using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameOverUI : MonoBehaviour
{    
    private Button retryButton;
    private TextMeshProUGUI scoreUI;

    private Game game;
    public void Initialize(Game game) {
        this.game = game;
        retryButton = transform.Find("Retry").GetComponent<Button>();        
        retryButton.onClick.AddListener(() => {
            game.CurrentState = EGameState.FAIL;
        });
        scoreUI = transform.Find("Score").GetComponent<TextMeshProUGUI>();        
    }

    public void SetScore(int score, int maxScore) {
        scoreUI.text = "Your score: " + score.ToString() + "/" + maxScore.ToString();
    }
}
