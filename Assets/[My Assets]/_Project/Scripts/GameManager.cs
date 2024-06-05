using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [field: SerializeField] public Map CurrentMap;

    [field: SerializeField] public TextMeshProUGUI YourScoreText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI OpponentScoreText { get; private set; }
    [field: SerializeField] public Image ResultsBackground { get; private set; }
    [field: SerializeField] public TextMeshProUGUI ResultsScoreText { get; private set; }
    

    private void Awake()
    {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
    
            Instance = this;
    }

    public void GoToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
}
