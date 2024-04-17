using System;
using System.Collections.Generic;
using FishNet.Demo.AdditiveScenes;
using UnityEngine;
using UnityEngine.Serialization;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal _otherPortal;

    [SerializeField] private bool _xAxis = true;
    [SerializeField] private bool _compareWithBigger;
    [SerializeField] private float _portalPointOffset;

    private List<PlayerMovement> _charactersInside = new List<PlayerMovement>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") == false)
            return;
        
        _charactersInside.Add(other.GetComponent<PlayerMovement>());
    }

    private void LateUpdate()
    {
        int charactersInsideCount = _charactersInside.Count;
        
        for (int i = 0; i < charactersInsideCount; i++)
        {
            PlayerMovement playerMovement = _charactersInside[i];

            if (!playerMovement)
            {
                _charactersInside.RemoveAt(i);
                i--; // We go back 1 on the for loop
                charactersInsideCount--;
                continue;
            }

            //Bounds bounds = playerMovement.RenderMain.bounds;
            float depth = 0f; // How much the sprite has entered
            float portalPosition = 0f;

            if (_xAxis)
            {
                portalPosition = transform.position.x + _portalPointOffset;
            }
            
            if (_compareWithBigger)
            {
                //depth = bounds.max.x - portalPosition;
            }
            else // Compare with smaller
            {
                //depth = bounds.min.x - portalPosition;
            }

            Vector3 positionCopy = _otherPortal.transform.position;
            if (_xAxis)
            {
                positionCopy.x += _otherPortal._portalPointOffset;
                //positionCopy.x += bounds.extends.x;
                positionCopy.x += depth;
                //positionCopy.y = playerMovement.RenderMain.transform.position.y;
            }

            //playerMovement.RenderCopy.transform.position = positionCopy;
        }
        
        // Lists are slower with foreach
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") == false)
            return;

        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        playerMovement.RenderCopy.transform.position = 
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 startPoint = transform.position;
        if (_xAxis)
        {
            startPoint.x += _portalPointOffset;
            
            Gizmos.DrawLine(startPoint + Vector3.up, startPoint + Vector3.down);
        }
    }
}
