using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using UnityEngine;

public class JoinMatch : MonoBehaviour
{
    [SerializeField] private GameObject _fishnetCanvas;
    
    void Start()
    {
        if (Matchmaking.FoundServer == null)
            return;
        
        _fishnetCanvas.SetActive(false);
        
        // Option 1: The UI is in the same scene as the Network Manager
        NetworkManager networkManager = FishNet.InstanceFinder.NetworkManager;
        networkManager.TransportManager.Transport.SetClientAddress(Matchmaking.FoundServer.ip);
        int newPort;
        if (Matchmaking.FoundServer.ports.TryGetValue("7770", out newPort)) // The default port exists? 
        {
            networkManager.TransportManager.Transport.SetPort((ushort)newPort);
        }

        networkManager.ClientManager.StartConnection();   
    }
}
