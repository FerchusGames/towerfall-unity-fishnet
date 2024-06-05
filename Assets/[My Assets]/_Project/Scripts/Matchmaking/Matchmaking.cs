using System;
using System.Collections.Generic;
using FishNet.Managing;
using PlayFlow;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Matchmaking : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private Button _searchButton;
    
    public static Server FoundServer { get; private set; }
    
    private const string clientToken = "68862c21c5b0502c617a085a03a1568d";
    
    public void FindMatch()
    {
        // In this variable we store the information necessary for the matchmaking
        PlayFlowManager.PlayerData playerData = new PlayFlowManager.PlayerData();
 
        playerData.player_id = SystemInfo.deviceUniqueIdentifier;
        
        #if UNITY_EDITOR
        // If we are in the editor, we add a random number to make tests with multiple editors. 
        playerData.player_id += Random.Range(100, 99999);
        #endif

        playerData.region = new List<string>()
        {
            "us-west",
            "us-west",
        };
        
        playerData.game_type = "default"; // Server tag
        
        // Optional
        playerData.elo = 100; // Skill
        playerData.custom_parameters = new List<PlayFlowManager.CustomParameter>();
        PlayFlowManager.CustomParameter matchParameter = new PlayFlowManager.CustomParameter();
        matchParameter.key = "difficulty";
        matchParameter.value = "medium";
        playerData.custom_parameters.Add(matchParameter);

        StartCoroutine(PlayFlowManager.FindMatch(clientToken, playerData, OnMatchFound));
        _searchButton.interactable = false; // We turn off the button to prevent the player from enqueing multiple times
    }

    private void Update()
    {
        if (PlayFlowManager.status == "In Queue")
            _statusText.text = PlayFlowManager.status;
    }

    private void OnMatchFound(Server server)
    {
        FoundServer = server;

        SceneManager.LoadScene("Map 1");
    }
}
