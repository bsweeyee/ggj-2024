using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    public static void InitGame() {
        if (FindObjectOfType<Game>() == null) {
            Debug.Log("here");
            var game = Resources.Load<Game>("GameManager");
            Instantiate(game);                    
        }
    }
}
