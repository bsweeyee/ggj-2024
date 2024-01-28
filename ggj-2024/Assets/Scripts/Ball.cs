using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    [SerializeField] private float initialRotation;
    [SerializeField] private float targetDeathRotation;    
    [SerializeField] private AnimationCurve hitStrengthDecay;
    [SerializeField] private LayerMask hitMasks;
    [SerializeField] private ContactFilter2D filter;

    [Header("Scale")]    
    [SerializeField] private AnimationCurve scaleReductionCurve;

    [Header("Direction")]
    [SerializeField] private float aimDirectionChangePerSecond = 80;
    [SerializeField] private float launchDirectionCorrectionPerSecond = 1;
    
    [Header("Power")]
    [SerializeField] private float rateOfPowerChangePerSecond = 1f;
    [SerializeField] private float maxPower = 2;
    [SerializeField] private float rollRate = 10; 

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
    private float cacheRotation;
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

    public Vector3 TravelDirection {
        get {
            return travelDirection;
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

            // we find the 0-1 value of initial and final position to determine how much to scale based on curve
            var iv = Mathf.InverseLerp(initialPosition.y, finalPosition.y, transform.position.y);
            transform.localScale =  Vector3.one * scaleReductionCurve.Evaluate(iv); 

            var isRightSide = Vector3.Dot(travelDirection, Vector3.right);
            if (isRightSide > 0) {
                var roll = transform.eulerAngles.z - (rollRate * dt * power);                
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, roll);
            } else {
                var roll = transform.eulerAngles.z + (rollRate * dt * power);                
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, roll);
            }            

            break;
            case EBallState.DEATH:
            currentDeathTime += dt;
            if (currentDeathTime > deathTimeInSeconds) {                
                Game.Instance.CurrentState = EGameState.GAME_END;                
            } else {
                var isRight = Vector3.Dot(travelDirection, Vector3.right);

                float euler = Mathf.Lerp(cacheRotation, -Mathf.Sign(isRight) * targetDeathRotation * power, currentDeathTime/deathTimeInSeconds);
                transform.eulerAngles = new Vector3(0, 0, euler);
                var travelPower = Mathf.Clamp(power * 2, 0, 20);                                
                transform.position += travelDirection * hitStrengthDecay.Evaluate(currentDeathTime / deathTimeInSeconds) * travelPower * dt;
            }
            break;
        }     
    }

    public void OnFixedUpdate(float dt) {        
        switch (currentState) {
            case EBallState.LAUNCH:                        
            var hits = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * transform.localScale.magnitude, hitMasks);                        
            if (hits.Length > 0) {
                foreach(var hit in hits) {                                        
                    var trigger = hit.GetComponent<ITrigger>();
                    if (trigger != null) {                        
                        trigger = hit.GetComponentInParent<ITrigger>();
                    }
                    trigger.OnHit(this);
                }
            }

            transform.position += travelDirection * power * dt;
            break;
        }
    }

    void OnEnterState(EBallState newState, EBallState oldState) {
        switch(newState) {
            case EBallState.AIM:
            transform.position = initialPosition;
            transform.eulerAngles = new Vector3(0, 0, initialRotation);

            arrow.transform.eulerAngles = Vector3.zero;

            travelDirection = Vector3.up;
            leftTravelNormalized = 0;
            rightTravelNormalized = 0;
            power = 0;
            
            arrowMask.transform.localScale = new Vector3(1, 0, 1);                        
            arrow.gameObject.SetActive(true);

            travelDirection = Vector3.up;
            break;

            case EBallState.LAUNCH:
            var launch = FindObjectsOfType<MonoBehaviour>().OfType<ILaunch>().ToArray();
            foreach(var l in launch) {
                l.OnLaunch(this);
            }
            arrow.gameObject.SetActive(false);
            break;
            case EBallState.DEATH:
            animator.SetBool("IsDeath", true);
            cacheRotation = transform.eulerAngles.z;
            break;
        }
    }

    void OnExitState(EBallState newState, EBallState oldState) {
        switch (oldState) {
            case EBallState.AIM:
            break;
            case EBallState.LAUNCH:            
            break;
            case EBallState.DEATH:
            currentDeathTime = 0;
            transform.localScale = Vector3.one;
            animator.SetBool("IsDeath", false);
            break;
        }
    }
}
