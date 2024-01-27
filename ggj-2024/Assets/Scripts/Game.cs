using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EGameState {
    NONE,
    START,
    PLAYING,
    LOAD_IN,
    LOAD_OUT,
    RESET,
    FAIL,
    PASS
}

public class Game : MonoBehaviour
{   
    [SerializeField] private Scene scenes;    
    [SerializeField] private EGameState currentState;

    // private int currentSceneIdx;
    private Ball ball;
    private List<Pinguin> pinguins;    

    private static Game instance;

    public EGameState CurrentState {
        get { return currentState; }
        set {
            var oldState = currentState;
            OnExitState(value, oldState);
            currentState = value; 
            OnEnterState(currentState, oldState);
        }
    }

    public static Game Instance 
    {
        get {
            if (instance == null) {
                instance = FindObjectOfType<Game>();
            }
            if (instance == null) {
                var go = new GameObject("Game");
                instance = go.AddComponent<Game>();
            }
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod]
    void Initialize() {
        DontDestroyOnLoad(Game.Instance.gameObject);        
        CurrentState = EGameState.PLAYING;        
    }

    void OnEnterState(EGameState newState, EGameState oldState) {
        switch (newState) {
            case EGameState.LOAD_IN:
            pinguins = FindObjectsOfType<Pinguin>().ToList();
            ball = FindObjectOfType<Ball>();

            ball.Initialize(this);
            foreach(var pinguin in pinguins) {
                pinguin.Initialize(this);
            }
            // TODO: run load in animation
            break;
            case EGameState.RESET:

            // TODO: play animation when there is time
            CurrentState = EGameState.PLAYING;
            break;
        }
    }

    void OnExitState(EGameState newState, EGameState oldState) {
        
    }

    void Update() {
        switch (currentState) {
            case EGameState.LOAD_IN:
            // TODO: wait until animation is done before going to playing state
            CurrentState = EGameState.PLAYING;
            break;
            case EGameState.PLAYING:
            var dt = Time.deltaTime;
            ball.OnUpdate(dt);

            foreach(var pinguin in pinguins) {
                pinguin.OnUpdate(dt);
            }
            break;
        }
    }

    void FixedUpdate() {
        switch (currentState) {            
            case EGameState.PLAYING:
            var dt = Time.fixedDeltaTime;

            ball.OnFixedUpdate(dt);
            foreach(var pinguin in pinguins) {
                pinguin.OnFixedUpdate(dt);
            }        
            break;
        }
    }
}
