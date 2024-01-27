using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrigger {
    void OnHit(Ball ball);
}

public class Pinguin : MonoBehaviour, ITrigger
{
    public void Initialize(Game game) {

    }

    public void OnUpdate(float dt) {

    }

    public void OnFixedUpdate(float dt) {
        
    }

    void ITrigger.OnHit(Ball ball)
    {

    }
}
