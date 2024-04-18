using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTeleport : MonoBehaviour
{
    private Vector2 _mapBorder;
    
    private void Start()
    {
        _mapBorder = GameManager.Instance.CurrentMap.MapSize / 2;
    }

    private void FixedUpdate()
    {
        Vector3 teleportLocation = transform.position;
        
        if (transform.position.x < -_mapBorder.x)
        {
            teleportLocation.x = _mapBorder.x;
            transform.position = teleportLocation;
        }

        if (transform.position.x > _mapBorder.x)
        {
            teleportLocation.x = -_mapBorder.x;
            transform.position = teleportLocation;
        }
        
        if (transform.position.y < -_mapBorder.y)
        {
            teleportLocation.y = _mapBorder.y;
            transform.position = teleportLocation;
        }
        
        if (transform.position.y > _mapBorder.y)
        {
            teleportLocation.y = -_mapBorder.y;
            transform.position = teleportLocation;
        }
    }
}
