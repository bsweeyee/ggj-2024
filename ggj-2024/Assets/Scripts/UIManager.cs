using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject youWinAssholeText;
    public float resetDelay;

    public void win()
    {
        youWinAssholeText.SetActive(true);
        Time.timeScale = 5f;
        Invoke("ResetGame", resetDelay );
    }
}
