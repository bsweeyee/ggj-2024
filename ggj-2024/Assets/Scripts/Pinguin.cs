using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrigger {
    void OnHit(Ball ball);
}

public interface ILaunch {
    void OnLaunch(Ball ball);
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
    [SerializeField] private Vector3 initialLocalPosiiton;
    [SerializeField] private float initialRotation;
    [SerializeField] private float targetDeathRotation;
    [SerializeField] private float initialHitStrength = 5;
    [SerializeField] private AnimationCurve hitStrengthDecay;
    
    [Header("Death")]
    [SerializeField] private float deathTimeInSeconds = 2;
    [SerializeField] private float popinTimeInSeconds = 0.5f;
    public bool deathRotation = true;

    protected Animator animator;

    [Header("Runtime")]
    [SerializeField] private EPinguinState currentState;

    private float currentDeathAnimationTime;
    private float currentPopInAnimationTime;
    private Vector3 hitDirection;

    private Game game;
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
        this.game = game;
        animator = GetComponent<Animator>();
        initialLocalPosiiton = transform.localPosition; 
        Vector3 euler = new Vector3 (0, 0, initialRotation);
        transform.eulerAngles  = euler;
        CurrentState = EPinguinState.IDLE;                
    }

    public void Reset() {
        animator.SetBool("IsDeath", false);
        animator.SetBool("IsPopIn", true);

        currentPopInAnimationTime = 0;

        transform.localPosition = initialLocalPosiiton;
        Vector3 euler = new Vector3 (0, 0, initialRotation);
        
        transform.eulerAngles  = euler;        
        CurrentState = EPinguinState.IDLE;
    }

    public void OnUpdate(float dt) {
        switch (currentState) {
            case EPinguinState.IDLE:
            if (currentPopInAnimationTime > popinTimeInSeconds) {
                animator.SetBool("IsPopIn", false);                
            }
            else {
                currentPopInAnimationTime += dt;
            }            
            break;
            case EPinguinState.DEATH:
            if (currentDeathAnimationTime < deathTimeInSeconds && deathRotation) {
                    if (deathRotation)
                    {
                        float euler = Mathf.Lerp(initialRotation, targetDeathRotation, currentDeathAnimationTime / deathTimeInSeconds);
                        transform.eulerAngles = new Vector3(0, 0, euler);
                        transform.localPosition += hitDirection * hitStrengthDecay.Evaluate(currentDeathAnimationTime / deathTimeInSeconds) * initialHitStrength * dt;
                    }
                    currentDeathAnimationTime += dt;
                }            
            break;
        }
    }

    public void OnFixedUpdate(float dt) {
    }

    void ITrigger.OnHit(Ball ball)
    {
        if (CurrentState == EPinguinState.IDLE) {
            CurrentState = EPinguinState.DEATH;
            hitDirection = (transform.position - ball.transform.position).normalized;
            var dot = Vector3.Dot(Vector3.Cross(ball.TravelDirection, hitDirection), Vector3.forward);            
            if (dot >= 0) {
                targetDeathRotation *= 1;
            } else {
                targetDeathRotation *= -1;
            }                
        }        
    }

    void OnEnterState(EPinguinState newState, EPinguinState oldState) {
        switch(newState) {
            case EPinguinState.DEATH:
            game.CurrentScore += 1;
            animator.SetBool("IsDeath", true);
            break;
        }
    }

    void OnExitState(EPinguinState newState, EPinguinState oldState) {
        switch(newState) {
            case EPinguinState.DEATH:
            currentDeathAnimationTime = 0;
            break;
        }    
    }
}
