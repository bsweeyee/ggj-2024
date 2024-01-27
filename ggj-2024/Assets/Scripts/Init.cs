using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    public static void InitGame() {
        Game.Instance.Initialize();
    }
}
