using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBallState {
    NONE,
    AIM,
    CHARGE,
    LAUNCH,
    DEATH,
    HIT
}

public class Ball : MonoBehaviour
{
    [Header("Initial")]
    [SerializeField] private Vector3 initialPosition;
    [SerializeField] private Vector3 finalPosition;
    [SerializeField] private LayerMask hitMasks;

    [Header("Scale")]    
    [SerializeField] private AnimationCurve scaleReductionCurve;

    [Header("Direction")]
    [SerializeField] private float aimDirectionChangePerSecond = 80;
    [SerializeField] private float launchDirectionCorrectionPerSecond = 1;
    
    [Header("Power")]
    [SerializeField] private float rateOfPowerChangePerSecond = 1f;
    [SerializeField] private float maxPower = 2;

    [Header("Death")]
     [SerializeField] private float deathTimeInSeconds = 0.5f;

    [Header("Runtime state")]
    [SerializeField] private float power;

    [SerializeField] private EBallState currentState;
    [SerializeField] private GameObject arrow;
    [SerializeField] private Transform arrowMask;    

    private float currentDeathTime;

    private Vector3 travelDirection;
    private Vector3 cacheTravelDirection;
    private float leftTravelNormalized;
    private float rightTravelNormalized;

    private SpriteRenderer ballSprite;
    private Animator animator;

    private CircleCollider2D circleCollider;

    public EBallState CurrentState {
        get { return currentState; }
        set {
            var oldState = currentState;
            OnExitState(value, oldState);
            currentState = value; 
            OnEnterState(currentState, oldState);
        }
    }

    public void Initialize(Game game) {
        arrow = transform.Find("Arrow").gameObject;
        arrowMask = transform.Find("Arrow/MaskObject");
                
        ballSprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        circleCollider = transform.Find("Collider").GetComponent<CircleCollider2D>();
        
        animator = transform.GetComponent<Animator>();
        
        CurrentState = EBallState.AIM;
    }

    public void Reset() {
        CurrentState = EBallState.AIM;
    }

    public void OnUpdate(float dt) {        
        switch (currentState) {
            case EBallState.AIM:
            if (Input.GetKey(KeyCode.LeftArrow)) {                
                var newEuler = arrow.transform.eulerAngles + new Vector3(0, 0, aimDirectionChangePerSecond * dt);
                arrow.transform.eulerAngles = newEuler;
                travelDirection = arrow.transform.up;                                
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                var newEuler = arrow.transform.eulerAngles + new Vector3(0, 0, -aimDirectionChangePerSecond * dt);
                arrow.transform.eulerAngles = newEuler;
                travelDirection = arrow.transform.up;
            }            
            if (Input.GetKeyDown(KeyCode.Space)) {
                CurrentState = EBallState.CHARGE;
            }
            break;
            case EBallState.CHARGE:
            if (Input.GetKey(KeyCode.Space)) {
                var newPower = power + (rateOfPowerChangePerSecond * dt);
                power = Mathf.Lerp(0, maxPower, newPower / maxPower);                

                var normalizedScale = (newPower / maxPower);
                var newScale = new Vector3(1, normalizedScale, 1);
                arrowMask.transform.localScale = newScale;
            }
            if (Input.GetKeyUp(KeyCode.Space)) {
                CurrentState = EBallState.LAUNCH;
            }
            break;
            case EBallState.LAUNCH:
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                cacheTravelDirection = travelDirection;        
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                cacheTravelDirection = travelDirection;
            }

            if (Input.GetKey(KeyCode.LeftArrow)) {
                leftTravelNormalized = Mathf.Clamp(leftTravelNormalized + launchDirectionCorrectionPerSecond * dt, 0, 1);
                travelDirection = Vector3.Slerp(cacheTravelDirection, -Vector3.right, leftTravelNormalized);
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                rightTravelNormalized = Mathf.Clamp(rightTravelNormalized + launchDirectionCorrectionPerSecond * dt, 0, 1);
                travelDirection = Vector3.Slerp(cacheTravelDirection, Vector3.right, rightTravelNormalized);
            }
            
            if (Input.GetKeyUp(KeyCode.LeftArrow)) {
                leftTravelNormalized = 0;
            }
            if (Input.GetKeyUp(KeyCode.RightArrow)) {
                rightTravelNormalized = 0;
            }

            var iv = Mathf.InverseLerp(initialPosition.y, finalPosition.y, transform.position.y);
            ballSprite.transform.localScale =  Vector3.one * scaleReductionCurve.Evaluate(iv);

            var hits = Physics.OverlapSphere(transform.position, circleCollider.radius, hitMasks);
            if (hits.Length > 0) {
                foreach(var hit in hits) {
                    hit.GetComponent<ITrigger>().OnHit(this);
                }
            }
            break;
            case EBallState.DEATH:
            currentDeathTime += dt;
            if (currentDeathTime > deathTimeInSeconds) {
                Game.Instance.CurrentState = EGameState.PLAYING;
            }
            break;
        }     
    }

    public void OnFixedUpdate(float dt) {        
        switch (currentState) {
            case EBallState.LAUNCH:
            transform.position += travelDirection * power * dt;
            break;
        }
    }

    void OnEnterState(EBallState newState, EBallState oldState) {
        switch(newState) {
            case EBallState.AIM:
            transform.position = initialPosition;
            
            arrowMask.transform.localScale = new Vector3(1, 0, 1);                        
            arrow.gameObject.SetActive(true);

            travelDirection = Vector3.up;
            break;

            case EBallState.LAUNCH:
            arrow.gameObject.SetActive(false);
            break;
            case EBallState.DEATH:
            animator.SetBool("IsDeath", true);
            break;
        }
    }

    void OnExitState(EBallState newState, EBallState oldState) {
        switch (oldState) {
            case EBallState.AIM:
            break;
            case EBallState.LAUNCH:
            ballSprite.transform.localScale = Vector3.one;
            break;
            case EBallState.DEATH:
            animator.SetBool("IsDeath", false);
            break;
        }
    }
}
