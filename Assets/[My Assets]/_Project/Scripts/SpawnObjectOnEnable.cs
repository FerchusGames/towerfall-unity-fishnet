using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObjectOnEnable : MonoBehaviour
{
    [SerializeField] GameObject _spawnObject;
    [SerializeField] float _objectLifetime;

    private void OnEnable()
    {
        GameObject gameObject = Instantiate(_spawnObject, transform.position, transform.rotation);
        gameObject.transform.localScale = transform.root.localScale;

        if (_objectLifetime > 0)
        {
            Destroy(gameObject, _objectLifetime);
        }
    }
}
