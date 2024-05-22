using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [field: SerializeField] public Map CurrentMap;

    private void Awake()
    {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
    
            Instance = this;
            DontDestroyOnLoad(gameObject);
    }
}
