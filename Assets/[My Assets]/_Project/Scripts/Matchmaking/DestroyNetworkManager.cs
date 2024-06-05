using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyNetworkManager : MonoBehaviour
{
    private void Start()
    {
        GameObject networkManager = GameObject.Find("NetworkManager");
        Destroy(networkManager);
    }
}
