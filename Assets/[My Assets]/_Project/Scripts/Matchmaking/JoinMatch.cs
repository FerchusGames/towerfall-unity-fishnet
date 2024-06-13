using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using UnityEngine;

public class JoinMatch : MonoBehaviour
{
    [SerializeField] private GameObject _fishnetCanvas;
    private WaitForSeconds _ws1 = new WaitForSeconds(1f);
    private NetworkManager _networkManager;

    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private SetAudioClip _setAudioClip;
    
    void Start()
    {
        if (Matchmaking.FoundServer == null)
        {
            StartMatch();
            return;
        }
        
        _fishnetCanvas.SetActive(false);
        
        // Option 1: The UI is in the same scene as the Network Manager
        _networkManager = FishNet.InstanceFinder.NetworkManager;
        _networkManager.TransportManager.Transport.SetClientAddress(Matchmaking.FoundServer.ip);
        int newPort;
        if (Matchmaking.FoundServer.ports.TryGetValue("7770", out newPort)) // The default port exists? 
        {
            _networkManager.TransportManager.Transport.SetPort((ushort)newPort);
        }

        StartCoroutine(TryConnection());
    }

    private IEnumerator TryConnection()
    {
        while (!_networkManager.ClientManager.Connection.IsActive)
        {
            _networkManager.ClientManager.StartConnection();   
            yield return null;
        }
        StartMatch();
    }

    private void StartMatch()
    {
        _loadingScreen.SetActive(false);
        _setAudioClip.enabled = true;
    }
}
