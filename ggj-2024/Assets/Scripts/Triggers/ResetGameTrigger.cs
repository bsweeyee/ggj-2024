using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGameTrigger : MonoBehaviour, ITrigger
{
    void ITrigger.OnHit(Ball ball)
    {
        // TODO: play ball animation bounce away animation and reset        
        ball.CurrentState = EBallState.DEATH;
    }
}
