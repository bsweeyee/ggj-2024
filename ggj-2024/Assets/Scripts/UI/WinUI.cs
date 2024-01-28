using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinUI : MonoBehaviour
{
    private Button nextLevel;
    public void Initialize(Game game) {
        nextLevel = transform.Find("NextLevel").GetComponent<Button>();        
        nextLevel.onClick.AddListener(() => {
            game.CurrentState = EGameState.PASS;
        });        
    }
}
