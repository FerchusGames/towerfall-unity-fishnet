using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using FishNet.Discovery;
using FishNet.Managing;
using UnityEngine;

public class JoinMatch : MonoBehaviour
{
    [SerializeField] private GameObject _fishnetCanvas;
    [SerializeField] private NetworkDiscoveryHud _networkDiscoveryHud;
    private WaitForSeconds _ws1 = new WaitForSeconds(1f);
    private NetworkManager _networkManager;

    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private SetAudioClip _setAudioClip;

    private void OnEnable()
    {
        if (Matchmaking.UserType == USER_TYPE.LAN_CLIENT)
         FindObjectOfType<NetworkDiscovery>().ServerFoundCallback += OnLanServerFound;
    }
    
    private void OnDisable()
    {
        if (Matchmaking.UserType == USER_TYPE.LAN_CLIENT)
            FindObjectOfType<NetworkDiscovery>().ServerFoundCallback += OnLanServerFound;
    }

    void Start()
    {
        if (Matchmaking.UserType == USER_TYPE.PLAYFLOW_SERVER)
        {
            StartMatch();
            return;
        }
        
        _fishnetCanvas.SetActive(false);
        _networkDiscoveryHud.enabled = false;

        switch (Matchmaking.UserType)
        {
            case USER_TYPE.PLAYFLOW_CLIENT:
                _networkManager = FishNet.InstanceFinder.NetworkManager;
                _networkManager.TransportManager.Transport.SetClientAddress(Matchmaking.FoundServer.ip);
                int newPort;
                if (Matchmaking.FoundServer.ports.TryGetValue("7770", out newPort)) // The default port exists? 
                {
                    _networkManager.TransportManager.Transport.SetPort((ushort)newPort);
                }
                break;
            
           case USER_TYPE.LAN_HOST:
                FishNet.InstanceFinder.ServerManager.StartConnection();
                FindObjectOfType<NetworkDiscovery>().AdvertiseServer();
                StartMatch();
               break;
           
           case USER_TYPE.LAN_CLIENT:
                FindObjectOfType<NetworkDiscovery>().SearchForServers();
               break;
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

    private void OnLanServerFound(IPEndPoint ipEndPoint)
    {
        FindObjectOfType<NetworkDiscovery>().StopSearchingOrAdvertising();
        FishNet.InstanceFinder.ClientManager.StartConnection(ipEndPoint.Address.ToString());
        
        StartMatch();
    }

    private void StartMatch()
    {
        _loadingScreen.SetActive(false);
        _setAudioClip.enabled = true;
    }
}
