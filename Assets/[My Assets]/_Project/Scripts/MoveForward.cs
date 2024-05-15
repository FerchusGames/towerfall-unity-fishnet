using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    [SerializeField] private float speed = default;
    void Update()
    {
        transform.Translate(transform.right * speed * Time.deltaTime * transform.localScale.x);
    }
}
