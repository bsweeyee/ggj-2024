using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrigger {
    void OnHit(Ball ball);
}

public enum EPinguinState {
    NONE,
    IDLE,
    SHOCK,
    DEATH
}

public class Pinguin : MonoBehaviour, ITrigger
{
    [Header("Initial")]
    [SerializeField] private Vector3 initialPosiiton;
    
    [Header("Death")]
    [SerializeField] private float deathTimeInSeconds;

    private Animator animator;

    [Header("Runtime")]
    [SerializeField] private EPinguinState currentState;

    private float currentDeathTime;

    public EPinguinState CurrentState {
        get { return currentState; }
        set {
            var oldState = currentState;
            OnExitState(value, oldState);
            currentState = value; 
            OnEnterState(currentState, oldState);
        }
    }    

    public void Initialize(Game game) {
    }

    public void Reset() {
        animator.SetBool("IsDeath", false);
        animator.SetBool("PopIn", true);
    }

    public void OnUpdate(float dt) {
        switch (currentState) {
            case EPinguinState.DEATH:
            // if (currentDeathTime > deathTimeInSeconds) {
            // }
            break;
        }
    }

    public void OnFixedUpdate(float dt) {
    }

    void ITrigger.OnHit(Ball ball)
    {        
        CurrentState = EPinguinState.DEATH;                
    }

    void OnEnterState(EPinguinState newState, EPinguinState oldState) {
        switch(newState) {
            case EPinguinState.DEATH:
            animator.SetBool("IsDeath", true);
            break;
        }
    }

    void OnExitState(EPinguinState newState, EPinguinState oldState) {
        switch(newState) {
            case EPinguinState.DEATH:
            currentDeathTime = 0;
            break;
        }    
    }
}
