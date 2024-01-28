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
    GAME_END,
    FAIL,
    PASS,
    CREDIT
}

public class Game : MonoBehaviour
{
    [SerializeField] private string mainScene;
    [SerializeField] private string creditScene;   
    [SerializeField] private string[] scenes;
    [SerializeField] private int[] targetScore;    
    [SerializeField] private EGameState currentState;
    [SerializeField] private int currentScore;

    private int currentSceneIdx;

    private UIManager uiManager;
    
    private Ball ball;
    private List<Pinguin> pinguins;    

    private static Game instance;

    public int SceneCount {
        get {  
            return scenes.Length;
        }
    }

    public EGameState CurrentState {
        get { return currentState; }
        set {
            var oldState = currentState;
            OnExitState(value, oldState);
            currentState = value; 
            OnEnterState(currentState, oldState);
        }
    }

    public int CurrentTargetScore {
        get { return targetScore[currentSceneIdx]; }
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

    public int CurrentScore {
        get {
            return currentScore;
        }
        set {
            currentScore = value;
        }
    }

    void Awake() {
        Initialize();
    }

    public void Initialize() {
        instance = this;
        DontDestroyOnLoad(Game.Instance.gameObject);                                

        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => {
            CurrentState = EGameState.LOAD_IN;                
        };       
        CurrentState = EGameState.LOAD_IN;
    } 

    public void ResetGame() {
        CurrentState = EGameState.GAME_END; 
    }

    void OnEnterState(EGameState newState, EGameState oldState) {
        switch (newState) {
            case EGameState.LOAD_IN:
            if (SceneManager.GetActiveScene().name == mainScene) {
                CurrentState = EGameState.START;
                return;
            }
            if (SceneManager.GetActiveScene().name == creditScene) {
                CurrentState = EGameState.CREDIT;
                return;
            }

            uiManager = FindObjectOfType<UIManager>();
            uiManager.Initialize(this);
            
            ball = FindObjectOfType<Ball>();            
            ball.Initialize(this);
            
            pinguins = FindObjectsOfType<Pinguin>().ToList();
            foreach(var pinguin in pinguins) {
                pinguin.Initialize(this);
            }            
            // TODO: run load in animation
            Debug.Log("load in here");
            CurrentState = EGameState.PLAYING;
            break;
            case EGameState.PLAYING:            
            currentScore = 0;            
            ball.Reset();
            foreach(var pinguin in pinguins) {                
                pinguin.Reset();
            }
            break;
            case EGameState.GAME_END:
            if (currentScore < targetScore[currentSceneIdx]) {
                uiManager.GameOver();
            } else {
                uiManager.Win();
            }                                     
            break;
            case EGameState.PASS:
            // TODO: popup the next level ui
            currentSceneIdx += 1;
            if (currentSceneIdx < scenes.Length - 1) {
                SceneManager.LoadScene(scenes[currentSceneIdx]);
            }
            else {
                SceneManager.LoadScene(creditScene);
            }                        
            break;
            case EGameState.FAIL:
            // TODO: popup the next level ui
            uiManager.Reset();
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
