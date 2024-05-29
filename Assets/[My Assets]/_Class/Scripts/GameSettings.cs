using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Class/GameSettings")]
public class GameSettings : ScriptableObject
{
    private static GameSettings instance;

    public static GameSettings Instance
    {
        get
        {
            if (!instance) // Only true for the FIRST that asks for the instance
            {
                if (Application.isEditor)
                {
                    instance = Resources.Load<GameSettings>("GameSettingsDeveloper");
                }
                else
                {
                    instance = Resources.Load<GameSettings>("GameSettingsRelease");
                }
            }

            return instance;
        }
    }
    
    public bool isGamePaused;
    public float gameSpeed;
    public float characterSpeed;
}
