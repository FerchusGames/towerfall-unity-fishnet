using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTrigger : MonoBehaviour
{
    [SerializeField] private bool _destroySelf = default;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject);
        if (_destroySelf)
        {
            Destroy(gameObject);
        }
    }
}
