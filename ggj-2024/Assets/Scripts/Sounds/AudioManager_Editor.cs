#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.Rendering;
using System;
using UnityEngine.Playables;
using UnityEngine.Analytics;

[CustomEditor(typeof(AudioManager))]
public class AudioManager_Editor : Editor
{    
    private string m_audioName;
    private AudioClip m_audioClip;
    private AudioManager.EAudioType m_audioType;
    private bool m_loop;

    SerializedObject m_so;
    SerializedProperty m_bgmList;
    SerializedProperty m_sfxList;
    SerializedProperty m_ambientList;

    private void OnEnable()
    {
        m_so = serializedObject;
        m_bgmList = m_so.FindProperty("m_bgmList");
        m_sfxList = m_so.FindProperty("m_sfxList");
        m_ambientList = m_so.FindProperty("m_ambientList");
    }

    GameObject CreateAudioSource(AudioManager.AudioData data, AudioManager audioManager) {
        GameObject source = new GameObject(data.Clip.name);
        var audioSrc = source.AddComponent<AudioSource>();
        audioSrc.clip = data.Clip;
        audioSrc.playOnAwake = false;

        audioSrc.transform.parent = audioManager.transform;

        return source;
    }

    void RemoveAudio(AudioManager.EAudioType type, AudioManager audioManager) {
        SerializedProperty list= null;
        switch(type) {
            case AudioManager.EAudioType.BGM:            
            list = m_bgmList;
            break;
            case AudioManager.EAudioType.SFX:
            list = m_sfxList;
            break;
            case AudioManager.EAudioType.AMBIENT:
            list = m_ambientList;
            break;
        }        

        for (var i = list.arraySize - 1; i >= 0 ; i--)
        {
            var audioSource = list.GetArrayElementAtIndex(i).objectReferenceValue as AudioSource;
            var audioSources = audioManager.AudioDataArray.Where(x => x.Clip.name == audioSource.name && x.Type == type).ToArray();
            if (audioSources == null || audioSources.Length <= 0)
            {
                var asrc = list.GetArrayElementAtIndex(i).objectReferenceValue as AudioSource;
                DestroyImmediate(asrc.gameObject);
                list.DeleteArrayElementAtIndex(i);
            }
        }
    }    

    int AddAudio(AudioManager.AudioData data, AudioManager audioManager, ref AudioSource source) {
        SerializedProperty list= null;
        List<AudioSource> audioList = null;
        int idx = 0;
        
        switch (data.Type) {
            case AudioManager.EAudioType.BGM:
            list = m_bgmList;
            audioList = audioManager.BGMList;
            break;
            case AudioManager.EAudioType.SFX:
            list = m_sfxList;
            audioList = audioManager.SFXList;
            break;
            case AudioManager.EAudioType.AMBIENT:
            list = m_ambientList;
            audioList = audioManager.AmbientList;
            break;
        }        
        if (!audioList.Exists(x => x.name == data.Clip.name)) {
            GameObject audioSource = CreateAudioSource(data, audioManager);
            list.InsertArrayElementAtIndex(list.arraySize);
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = audioSource;
            idx = list.arraySize - 1;
            source = audioSource.GetComponent<AudioSource>();
        } else {
            idx = audioList.FindIndex(x => x.name == data.Clip.name);
            source = audioList.Find(x => x.name == data.Clip.name);
        } 
        return idx;
    }

    void UpdateAudio(AudioManager.AudioData data, AudioManager audioManager, int idx) {
        SerializedProperty list= null;
        switch (data.Type) {
            case AudioManager.EAudioType.BGM:
            list = m_bgmList;
            break;
            case AudioManager.EAudioType.SFX:
            list = m_sfxList;
            break;
            case AudioManager.EAudioType.AMBIENT:
            list = m_ambientList;
            break;
        } 
        var sceneCount = SceneManager.sceneCountInBuildSettings;
        var scenes = new string[sceneCount+1];
        scenes[0] = "Play on scene start?";
        for(int i =1; i<sceneCount + 1; i++) {
            var s = SceneUtility.GetScenePathByBuildIndex(i-1);            
            scenes[i] = s;            
        }        

        var audioSource = list.GetArrayElementAtIndex(idx).objectReferenceValue as AudioSource;        
        EditorGUILayout.LabelField(audioSource.loop ? "loop" : "no loop", EditorStyles.label);        

        
        var mainMenu = audioManager.InitialMainMenuAudio.FindIndex(x => x == audioSource);
        var gameScene = audioManager.InitialGameSceneAudio.FindIndex(x => x == audioSource);
        var credit = audioManager.InitialCreditAudio.FindIndex(x => x == audioSource);

        var oldAudioList = (mainMenu >= 0) ? audioManager.InitialMainMenuAudio : (gameScene >= 0) ? audioManager.InitialGameSceneAudio : (credit >= 0) ? audioManager.InitialCreditAudio : null;        
        var oldSerializedAudioList = (mainMenu >= 0) ? m_so.FindProperty("m_initialMainMenuAudio") : (gameScene >= 0) ? m_so.FindProperty("m_initialGameSceneAudio") : (credit >= 0) ? m_so.FindProperty("m_initialCreditAudio") : null;

        int oldIndex = (mainMenu >= 0) ? 1 : (gameScene >= 0) ? 2 : (credit >= 0) ? 3 : 0;
        var newIndex = EditorGUILayout.Popup(oldIndex, scenes);            
        
        var currentAudioList = (newIndex == 1) ? audioManager.InitialMainMenuAudio : (oldIndex == 2) ? audioManager.InitialGameSceneAudio : (oldIndex == 3) ? audioManager.InitialCreditAudio : null;    
        var currentSerializedAudioList = (newIndex == 1) ? m_so.FindProperty("m_initialMainMenuAudio") : (newIndex == 2) ? m_so.FindProperty("m_initialGameSceneAudio") : (newIndex == 3) ? m_so.FindProperty("m_initialCreditAudio") : null;                 


        if (newIndex >= 1 && newIndex != oldIndex) {
            if (currentAudioList != null) {
                var hitIdx = currentAudioList.FindIndex(x => x == audioSource);
                if (hitIdx < 0) {
                    currentSerializedAudioList.InsertArrayElementAtIndex(currentSerializedAudioList.arraySize);
                    currentSerializedAudioList.GetArrayElementAtIndex(currentSerializedAudioList.arraySize - 1).objectReferenceValue = audioSource;                                    
                }
            }
            else {
                currentSerializedAudioList.InsertArrayElementAtIndex(currentSerializedAudioList.arraySize);
                currentSerializedAudioList.GetArrayElementAtIndex(currentSerializedAudioList.arraySize - 1).objectReferenceValue = audioSource;
            }                 

            if (oldAudioList != null) {
                var oldHitIdx = oldAudioList.FindIndex(x => x == audioSource);
                if (oldHitIdx >= 0) {
                    oldSerializedAudioList.DeleteArrayElementAtIndex(oldHitIdx);
                }                   
            }
        }

        if (newIndex <= 0) {
            if (oldAudioList != null) {
                var oldHitIdx = oldAudioList.FindIndex(x => x == audioSource);
                if (oldHitIdx >= 0) {
                    oldSerializedAudioList.DeleteArrayElementAtIndex(oldHitIdx);
                } 
            }
        }       

        list.GetArrayElementAtIndex(idx).objectReferenceValue = audioSource;                
    }

    public override void OnInspectorGUI()
    {
        bool persistent = EditorUtility.IsPersistent(Selection.activeObject);
        if (persistent) {
            EditorGUILayout.LabelField("Please edit audio in Prefab mode or in a Scene", EditorStyles.boldLabel); 
            return;
        }                        
        base.OnInspectorGUI();
        var audioManager = m_so.targetObject as AudioManager;

        m_so.Update();
        EditorGUILayout.LabelField("Audio Sources:", EditorStyles.boldLabel);                             
        
        if (audioManager.AudioDataArray == null) return;
        foreach(var data in audioManager.AudioDataArray) {                        
            if (data.Clip != null) {
            using (new GUILayout.HorizontalScope()) {
                    var originalLabelWidth = EditorGUIUtility.labelWidth;
                    var originalFieldWidth = EditorGUIUtility.fieldWidth;

                    AudioSource source = null;

                    var idx = AddAudio(data, audioManager, ref source);
                    
                    EditorGUIUtility.fieldWidth = 50;
                    EditorGUILayout.ObjectField(source, typeof(AudioSource), false);                                        
                    EditorGUIUtility.labelWidth = 50;                
                    EditorGUILayout.LabelField(data.Type.ToString());

                    UpdateAudio(data, audioManager, idx);

                    EditorGUIUtility.labelWidth = originalLabelWidth;
                    EditorGUIUtility.fieldWidth = originalFieldWidth;
                }
            }
        }

        RemoveAudio(AudioManager.EAudioType.BGM, audioManager);
        RemoveAudio(AudioManager.EAudioType.SFX, audioManager);
        RemoveAudio(AudioManager.EAudioType.AMBIENT, audioManager); 

        m_so.ApplyModifiedProperties();            
    }
}

#endif
