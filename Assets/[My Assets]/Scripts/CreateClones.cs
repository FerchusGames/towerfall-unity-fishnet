using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateClones : MonoBehaviour
{
    [SerializeField] private GameObject _mainRenderer;
    
    private GameObject[] _cloneRenderers = new GameObject[4];

    private void Start()
    {
        Vector2 mapSize = GameManager.Instance.CurrentMap.MapSize;

        Vector3 clonePosition = new Vector3(transform.position.x - mapSize.x, transform.position.y, transform.position.z); 
        _cloneRenderers[0] = Instantiate(_mainRenderer, clonePosition, Quaternion.identity);
        
        clonePosition.x = transform.position.x + mapSize.x; 
        _cloneRenderers[1] = Instantiate(_mainRenderer, clonePosition, Quaternion.identity);
        
        
        clonePosition.x = transform.position.x;
        clonePosition.y = transform.position.y - mapSize.y;
        _cloneRenderers[2] = Instantiate(_mainRenderer, clonePosition, Quaternion.identity);
        
        clonePosition.y = transform.position.y + mapSize.y;
        _cloneRenderers[3] = Instantiate(_mainRenderer, clonePosition, Quaternion.identity);

        for (int i = 0; i < _cloneRenderers.Length; i++)
        {
            _cloneRenderers[i].transform.SetParent(_mainRenderer.transform, true);
        }

        foreach (ICloned cloned in GetComponents<ICloned>())
        {
            cloned.SetAnimators(GetAnimators());
        }
    }

    public Animator[] GetAnimators()
    {
        return new Animator[]
        {
            _mainRenderer.GetComponent<Animator>(),
            _cloneRenderers[0].GetComponent<Animator>(),
            _cloneRenderers[1].GetComponent<Animator>(),
            _cloneRenderers[2].GetComponent<Animator>(),
            _cloneRenderers[3].GetComponent<Animator>(),
        };
    }
}
