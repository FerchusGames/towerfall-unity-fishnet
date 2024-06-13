using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName="AudioLibrary" ,menuName = "Scriptables/AudioLibrary", order = 0)]

public class AudioLibrary : ScriptableObject
{
    [SerializeField] AudioData[] audios;


    public AudioClip GetAudio(SOUND_TYPE _soundRequest)
    {
        for (int i = 0; i < audios.Length; i++)
        {
            if (audios[i].soundType == _soundRequest)
            {
                int randomNumber = UnityEngine.Random.Range(0, audios[i].clip.Length);
                return audios[i].clip[randomNumber];
            }
        }
        Debug.LogWarning($"No audio clip found for {_soundRequest}");
        return null;
    }

    public float GetLevel(SOUND_TYPE _soundRequest)
    {
        for (int i = 0; i < audios.Length; i++)
        {
            if (audios[i].soundType == _soundRequest)
            {
                return audios[i].audioLevel;
            }
        }

        Debug.LogWarning($"No audio level found for {_soundRequest}");
        return 1;
    }
    
}
[Serializable]
public class AudioData
{
    [Range(0, 2)] public float audioLevel = 1.0f;
    public SOUND_TYPE soundType;
    public AudioClip[] clip;
}
public enum SOUND_TYPE
{
    JUMP_SELF,
    JUMP_OPPONENT,
    DAMAGE_SELF,
    DAMAGE_OPPONENT,
    SHOOT_SELF,
    SHOOT_OPPONENT,
    AIM_SELF,
    AIM_OPPONENT,
    LAND_SELF,
    LAND_OPPONENT,
}
