using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private int currentScore;
    public TextMesh scoreText;
    // Use this for initialization
    void Start()
    {
        currentScore = 0;
    }

    private void HandleScore ()
    {
        scoreText.text = "Score: " + currentScore;
    }
}

