using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Map")]
public class Map : ScriptableObject
{
    [field: SerializeField] public Vector2 MapSize;
}
