using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioSetting", menuName = "Audio/Settings", order = 1)]
public class AudioSetting : ScriptableObject
{
    [Header("UI Sounds")] 
    public AudioClip DefaultOnButtonClick;
    public AudioClip DefaultOnSliderChange;
    public AudioClip DefaultOnDropdownClick;
    public AudioClip DefaultOnTriggerHit;
}
