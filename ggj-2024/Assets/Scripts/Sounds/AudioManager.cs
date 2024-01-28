using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public enum EAudioType {
        BGM,
        SFX,
        AMBIENT,
        ALL
    }

    [Serializable]
    public struct AudioData {
        public AudioClip Clip;
        public EAudioType Type;        
    }

    [SerializeField][Range(0, 1)] private float maxMasterVolume = 1;
    [SerializeField][Range(0, 1)] private float maxBGMVolume = 1;
    [SerializeField][Range(0, 1)] private float maxSFXVolume = 1;
    [SerializeField][Range(0, 1)] private float maxAmbientVolume = 1;

    [SerializeField] private float defaultFadeInTime;
    [SerializeField] private float defaultFadeOutTime;
    [SerializeField] private AnimationCurve defaultFadeIn;
    [SerializeField] private AnimationCurve defaultFadeOut;

    [SerializeField] public AudioManager.AudioData[] AudioDataArray;
    [SerializeField] public List<AudioSetting> SceneAudioSettings;
    
    [SerializeField][HideInInspector] private List<AudioSource> m_sfxList;
    [SerializeField][HideInInspector] private List<AudioSource> m_bgmList;
    [SerializeField][HideInInspector] private List<AudioSource> m_ambientList;

    [SerializeField] private List<AudioSource> m_initialMainMenuAudio;
    [SerializeField] private List<AudioSource> m_initialGameSceneAudio;
    [SerializeField] private List<AudioSource> m_initialCreditAudio;

    private static AudioManager m_instance;
    public static AudioManager Instance
    {
        get
        {
            m_instance = GameObject.FindObjectOfType<AudioManager>();
            if (m_instance == null)
            {
                var gObj = Instantiate(Resources.Load<AudioManager>("AudioManager"));
                DontDestroyOnLoad(gObj.gameObject);
                m_instance = gObj.GetComponent<AudioManager>();
            }
            return m_instance;                        
        }
    } 

    public List<AudioSource> SFXList {
        get {
            return m_sfxList;
        }
    }

    public List<AudioSource> BGMList {
        get {
            return m_bgmList;
        }
    }

    public List<AudioSource> AmbientList {
        get {
            return m_ambientList;
        }
    }

    public List<AudioSource> InitialMainMenuAudio {
        get {
            return m_initialMainMenuAudio;
        }
    }

    public List<AudioSource> InitialGameSceneAudio {
        get {
            return m_initialGameSceneAudio;
        }
    }

    public List<AudioSource> InitialCreditAudio {
        get {
            return m_initialCreditAudio;
        }
    }

    private List<AudioSource> currentlyPlayingBGM;
    private List<AudioSource> currentlyPlayingAmbient;
    private List<AudioSource> currentlyPlayingSFX;

    private Dictionary<string, float> targetAudioVolume;
    private Dictionary<string, float> resumeTime;
    private Dictionary<string, float> currentAudioFadeTime; 
    private Dictionary<string, float> audioFadeTime;
    private Dictionary<string, AnimationCurve> audioAnimationCurve;

    private float currentMasterVolume = 1;
    private float currentBGMVolume = 1;
    private float currentSFXVolume = 1;
    private float currentAmbientVolume = 1; 

    private bool isInitialized;   
    public float CurrentMasterVolume {
        get { return currentMasterVolume; }
        set { currentMasterVolume = value; }
    }
    public float CurrentBGMVolume {
        get { return currentBGMVolume; }
    }
    public float CurrentSFXVolume {
        get { return currentSFXVolume; }
    }
    public float CurrentAmbientVolume {
        get { return currentAmbientVolume; }
    }
    public float MaxMasterVolume {
        get { return maxMasterVolume; }
    }

    [RuntimeInitializeOnLoadMethod]
    static void Main() {
        AudioManager.Instance.PlaySceneAudio(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        
        float masterVolume = 1;
        float bgm = 1;
        float sfx = 1;
        float ambient = 1;
        
        if (PlayerPrefs.HasKey("MasterVolume")) masterVolume = PlayerPrefs.GetFloat("MasterVolume");
        if (PlayerPrefs.HasKey("BGM")) bgm = PlayerPrefs.GetFloat("BGM");
        if (PlayerPrefs.HasKey("SFX")) sfx = PlayerPrefs.GetFloat("SFX");
        if (PlayerPrefs.HasKey("Ambient")) ambient = PlayerPrefs.GetFloat("Ambient");
        
        AudioManager.Instance.SetNormalizedVolume(AudioManager.EAudioType.ALL, masterVolume);
        AudioManager.Instance.SetNormalizedVolume(AudioManager.EAudioType.BGM, bgm);
        AudioManager.Instance.SetNormalizedVolume(AudioManager.EAudioType.SFX, sfx);
        AudioManager.Instance.SetNormalizedVolume(AudioManager.EAudioType.AMBIENT, ambient);        
    }

    public void Initialize() {
        if (isInitialized) return;
        targetAudioVolume = new Dictionary<string, float>();
        resumeTime = new Dictionary<string, float>();
        currentAudioFadeTime = new Dictionary<string, float>();
        audioFadeTime = new Dictionary<string, float>();
        audioAnimationCurve = new Dictionary<string, AnimationCurve>();

        currentlyPlayingBGM = new List<AudioSource>();
        currentlyPlayingAmbient = new List<AudioSource>();
        currentlyPlayingSFX = new List<AudioSource>();

        foreach(var audio in m_bgmList) {
            targetAudioVolume.Add(audio.name, -1);
            currentAudioFadeTime.Add(audio.name, -1);
            audioFadeTime.Add(audio.name, -1);
            audioAnimationCurve.Add(audio.name, defaultFadeIn);
            resumeTime.Add(audio.name, 0);
        }
        foreach(var audio in m_sfxList) {
            targetAudioVolume.Add(audio.name, -1);
            currentAudioFadeTime.Add(audio.name, -1);
            audioFadeTime.Add(audio.name, -1);
            audioAnimationCurve.Add(audio.name, defaultFadeIn);
            resumeTime.Add(audio.name, 0);
        }
        foreach(var audio in m_ambientList) {
            targetAudioVolume.Add(audio.name, -1);
            currentAudioFadeTime.Add(audio.name, -1);
            audioFadeTime.Add(audio.name, -1);
            audioAnimationCurve.Add(audio.name, defaultFadeIn);
            resumeTime.Add(audio.name, 0);
        }
        SceneManager.sceneLoaded += Instance.PlaySceneAudio;
        SceneManager.sceneUnloaded += Instance.StopSceneAudio;

        isInitialized = true;
    }

    void Update() {
        var dt = Time.deltaTime;

        for(int i=currentlyPlayingBGM.Count - 1; i >= 0; i--) {
            var c = currentlyPlayingBGM[i];
            if (currentAudioFadeTime[c.name] >= 0) {
                currentAudioFadeTime[c.name] += dt;
            }            
            var fV = Fade(targetAudioVolume[c.name], currentAudioFadeTime[c.name], audioFadeTime[c.name], c, audioAnimationCurve[c.name]);
            c.volume =  (fV < 0) ? c.volume : fV;
            if (currentAudioFadeTime[c.name] > audioFadeTime[c.name]) {
                currentAudioFadeTime[c.name] = -1;
                if (resumeTime[c.name] < 0) {
                    c.Stop();
                    currentlyPlayingBGM.Remove(c);
                    resumeTime[c.name] = 0;
                }
            }
        }
        for(int i=currentlyPlayingSFX.Count - 1; i >= 0; i--) {
            var c = currentlyPlayingSFX[i];
            if (currentAudioFadeTime[c.name] >= 0) {
                currentAudioFadeTime[c.name] += dt;
            }
            var fV = Fade(targetAudioVolume[c.name], currentAudioFadeTime[c.name], audioFadeTime[c.name], c, audioAnimationCurve[c.name]);
            c.volume =  (fV < 0) ? c.volume : fV;
            if (currentAudioFadeTime[c.name] > audioFadeTime[c.name]) {
                currentAudioFadeTime[c.name] = -1;
                if (resumeTime[c.name] < 0) {
                    c.Stop();
                    currentlyPlayingSFX.Remove(c);
                    resumeTime[c.name] = 0;
                }
            }
        }
        for(int i=currentlyPlayingAmbient.Count - 1; i >= 0; i--) {
            var c = currentlyPlayingAmbient[i];
            if (currentAudioFadeTime[c.name] >= 0) {
                currentAudioFadeTime[c.name] += dt;
            }
            var fV = Fade(targetAudioVolume[c.name], currentAudioFadeTime[c.name], audioFadeTime[c.name], c, audioAnimationCurve[c.name]);            
            c.volume =  (fV < 0) ? c.volume : fV;
            if (currentAudioFadeTime[c.name] > audioFadeTime[c.name]) {
                currentAudioFadeTime[c.name] = -1;
                if (resumeTime[c.name] < 0) {
                    c.Stop();
                    currentlyPlayingAmbient.Remove(c);
                    resumeTime[c.name] = 0;
                }
            }
        }
    }     

    void PlaySceneAudio(Scene loadedScene, LoadSceneMode sceneMode) {        
        Initialize();                        
        if (loadedScene.buildIndex == 0) {
            foreach(var imm in m_initialMainMenuAudio) {
                Play(imm.name, defaultFadeInTime);                                      
            }                                     
        }
        else if (loadedScene.buildIndex == 1) {
            foreach(var igs in m_initialGameSceneAudio) {
                Play(igs.name, defaultFadeInTime);
            }                         
        }
        else if (loadedScene.buildIndex == Game.Instance.SceneCount + 1) {
            foreach(var ica in m_initialCreditAudio) {                
                Play(ica.name, defaultFadeInTime);
            }                         
        }
    }

    void StopSceneAudio(Scene unloaded) {
        if (unloaded.buildIndex >= 1 && unloaded.buildIndex < Game.Instance.SceneCount + 2) {
            Stop(EAudioType.SFX);
            Stop(EAudioType.AMBIENT);
        } else {
            Stop(EAudioType.ALL);
        }
    }    

    public void Play(string name, float fadeInTime = -1, float startTime = 0) {
        var ada = AudioDataArray.First(x => x.Clip.name == name);
        if (ada.Clip == null) Debug.Log($"Missing clip name {name}");
        
        var type = ada.Type;
        var list = (type == EAudioType.BGM) ? m_bgmList : (type == EAudioType.SFX) ? m_sfxList : m_ambientList;                        
        var currentList = (type == EAudioType.BGM) ? currentlyPlayingBGM : (type == EAudioType.SFX) ? currentlyPlayingSFX : currentlyPlayingAmbient;
        var currentVolume = (type == EAudioType.BGM) ? currentBGMVolume : (type == EAudioType.SFX) ? currentSFXVolume : currentAmbientVolume;
        var currentMax = (type == EAudioType.BGM) ? maxBGMVolume : (type == EAudioType.SFX) ? maxSFXVolume : maxAmbientVolume;

        AudioSource source = list.Find(x => x.name == name);
        if (source == null) {
            Debug.LogError($"Missing {type.ToString()} audio source: {name}");
        } else {
            if (source.clip == null) Debug.LogError($"Missing {type.ToString()} audio clip: {name}");
            else {
                if (startTime >= 0) source.time = startTime;
                if (fadeInTime < 0) {
                    source.Play();                        
                } else {
                    float fiTime = (fadeInTime > 0) ? fadeInTime : defaultFadeInTime;
                    var nVol = Mathf.Lerp(0, currentMasterVolume, currentMax);
                    currentAudioFadeTime[source.name] = 0; // check what isthe current time wrt to current volume
                    audioFadeTime[source.name] = fiTime;
                    audioAnimationCurve[source.name] = defaultFadeIn;
                    if (resumeTime[source.name] > 0)  { 
                        source.time = resumeTime[source.name];
                        resumeTime[source.name] = 0;
                    }
                    else source.Play();
                }                    
                if (!currentList.Contains(source)) currentList.Add(source);                    
            }
        }
    }

    public void Play(EAudioType type, float fadeInTime = -1) {
        if (type == EAudioType.ALL) {
            if (currentlyPlayingBGM != null) {
                foreach(var item in currentlyPlayingBGM) {
                    Play(item.name, fadeInTime);
                }
            }
            if (currentlyPlayingSFX != null) {
                foreach(var item in currentlyPlayingSFX) {
                    Play(item.name, fadeInTime);
                }
            }
            if (currentlyPlayingAmbient != null) {
                foreach(var item in currentlyPlayingAmbient) {
                    Play(item.name, fadeInTime);
                }
            }
        } else {
            var list = (type == EAudioType.BGM) ? currentlyPlayingBGM : (type == EAudioType.SFX) ? currentlyPlayingSFX : currentlyPlayingAmbient;
            if (list != null) {
                foreach(var item in list) {
                    Play(item.name, fadeInTime);
                }
            }
        }
    }

    public void Stop(string name, float fadeOutTime = -1) {
        var ada = AudioDataArray.First(x => x.Clip.name == name);
        if (ada.Clip == null) Debug.Log($"Missing clip name {name}");
        
        var type = ada.Type;
        var list = (type == EAudioType.BGM) ? m_bgmList : (type == EAudioType.SFX) ? m_sfxList : m_ambientList;
        AudioSource source = list.Find(x => x.name == name);

        if (source == null)
        {
            Debug.LogError("Missing " + type.ToString() + " audio source: " + name);
        }
        else
        {
            if (source.clip == null) Debug.LogError("Missing " + type.ToString() + " audio clip: " + name);
            else {
                if (fadeOutTime >= 0) {                    
                    float foTime = (fadeOutTime > 0) ? fadeOutTime : defaultFadeOutTime;
                    resumeTime[source.name] = -1;  
                    currentAudioFadeTime[source.name] = 0;
                    audioFadeTime[source.name] = foTime;
                    audioAnimationCurve[source.name] = defaultFadeOut;
                }
                else {
                    source.Stop();
                }
            }
        }        
    }

    public void Stop(EAudioType type, float fadeOutTime = -1) {
        if (type == EAudioType.ALL) {
            if (currentlyPlayingBGM != null) {
                foreach(var item in currentlyPlayingBGM) {
                    item.Stop();                    
                }
                currentlyPlayingBGM.Clear();
            }
            if (currentlyPlayingSFX != null) {
                foreach(var item in currentlyPlayingSFX) {
                    item.Stop();
                }
                currentlyPlayingSFX.Clear();
            }
            if (currentlyPlayingAmbient != null) {
                foreach(var item in currentlyPlayingAmbient) {
                    item.Stop();
                }
                currentlyPlayingAmbient.Clear();
            }
        } else {
            var list = (type == EAudioType.BGM) ? currentlyPlayingBGM : (type == EAudioType.SFX) ? currentlyPlayingSFX : currentlyPlayingAmbient;
            if (list != null) {
                foreach(var item in list) {
                    item.Stop();
                }
                list.Clear();
            }
        }
    }

    public void Pause(string name, float fadeOutTime = -1) {
        var ada = AudioDataArray.First(x => x.Clip.name == name);
        if (ada.Clip == null) Debug.Log($"Missing clip name {name}");
        
        var type = ada.Type;
        var list = (type == EAudioType.BGM) ? m_bgmList : (type == EAudioType.SFX) ? m_sfxList : m_ambientList;
        AudioSource source = list.Find(x => x.name == name);

        if (source == null)
        {
            Debug.LogError("Missing " + type.ToString() + " audio source: " + name);
        }
        else
        {
            if (source.clip == null) Debug.LogError("Missing " + type.ToString() + " audio clip: " + name);
            else {                
                if (fadeOutTime >= 0) {
                    float foTime = (fadeOutTime > 0) ? fadeOutTime : defaultFadeOutTime; 
                    resumeTime[source.name] = source.time;
                    currentAudioFadeTime[source.name] = 0;
                    audioFadeTime[source.name] = foTime;
                    audioAnimationCurve[source.name] = defaultFadeOut;
                }
                else {
                    source.Pause();
                }
            }
        }        
    }

    public void Pause(EAudioType type, float fadeOutTime = -1) {
        if (type == EAudioType.ALL) {
            if (currentlyPlayingBGM != null) {
                foreach(var item in currentlyPlayingBGM) {
                    Pause(item.name, fadeOutTime);
                }
            }
            if (currentlyPlayingSFX != null) {
                foreach(var item in currentlyPlayingSFX) {
                    Pause(item.name, fadeOutTime);
                }
            }
            if (currentlyPlayingAmbient != null) {
                foreach(var item in currentlyPlayingAmbient) {
                    Pause(item.name, fadeOutTime);
                }
            }
        } else {
            var list = (type == EAudioType.BGM) ? currentlyPlayingBGM : (type == EAudioType.SFX) ? currentlyPlayingSFX : currentlyPlayingAmbient;
            if (list != null) {
                foreach(var item in list) {
                    Pause(item.name, fadeOutTime);
                }
            }
        }
    }

    public void SetNormalizedVolume(EAudioType type, float normalizeVolume) {
        switch (type) {
            case EAudioType.ALL:
            currentMasterVolume = Mathf.Lerp(0, maxMasterVolume, normalizeVolume);                          
            if (m_bgmList != null) {
                foreach(var s in m_bgmList) {
                    var nBGM = Mathf.Lerp(0, currentMasterVolume, maxBGMVolume);
                    targetAudioVolume[s.name] = Mathf.Lerp(0, nBGM, currentBGMVolume);
                }
            }
            if (m_sfxList != null) {
                foreach(var s in m_sfxList) {
                    var nSFX = Mathf.Lerp(0, currentMasterVolume, maxSFXVolume);
                    targetAudioVolume[s.name] = Mathf.Lerp(0, nSFX, currentSFXVolume);
                }
            }
            if (m_ambientList != null) {
                foreach(var s in m_ambientList) { 
                    var nSFX = Mathf.Lerp(0, currentMasterVolume, maxAmbientVolume);
                    targetAudioVolume[s.name] = Mathf.Lerp(0, nSFX, currentAmbientVolume);
                }
            }
            break;
            case EAudioType.BGM:
            currentBGMVolume = normalizeVolume;
            if (m_bgmList != null) {            
                foreach(var s in m_bgmList) {
                    var nBGM = Mathf.Lerp(0, currentMasterVolume, maxBGMVolume);                
                    targetAudioVolume[s.name] = Mathf.Lerp(0, nBGM, currentBGMVolume);
                }
            }
            break;
            case EAudioType.SFX:
            currentSFXVolume = normalizeVolume;
            if (m_sfxList != null) {
                foreach(var s in m_sfxList) {
                    var nSFX = Mathf.Lerp(0, currentMasterVolume, maxSFXVolume);
                    targetAudioVolume[s.name] = Mathf.Lerp(0, nSFX, currentSFXVolume);
                }
            }
            break;
            case EAudioType.AMBIENT:
            currentAmbientVolume = normalizeVolume;
            if (m_ambientList != null) {
                foreach(var s in m_ambientList) {
                    var nAmbient = Mathf.Lerp(0, currentMasterVolume, maxSFXVolume);
                    targetAudioVolume[s.name] = Mathf.Lerp(0, nAmbient, currentAmbientVolume);
                }
            }
            break;
        }                         
    }

    float Fade(float maxVolume, float currentAudioFadeTime, float fadeTime, AudioSource audio, AnimationCurve audioFadeCurve)
    {
        float finalVolume = -1;                
        // if (!audio) return finalVolume;
        // if (!isStop && !audio.isPlaying) 
        // {
        //     if (currentlyPlayingBGM.Contains(audio) ||
        //         currentlyPlayingSFX.Contains(audio) ||
        //         currentlyPlayingAmbient.Contains(audio)) {                    
        //             audio.time = resumeTime[audio.name];
        //             audio.UnPause();
        //     }
        //     else  {
        //         audio.Play(); 
        //     } 
        // }      
        if (currentAudioFadeTime >= 0) {
            var t = Mathf.InverseLerp(0, fadeTime, currentAudioFadeTime);
            finalVolume = maxVolume * audioFadeCurve.Evaluate(t);                        
        } else if (resumeTime[audio.name] == 0) {
            finalVolume = maxVolume;
        }
                            
        // if (isStop) { audio.Stop(); }
        // if (isPause) { audio.Pause(); }

        return finalVolume;
    }
}
