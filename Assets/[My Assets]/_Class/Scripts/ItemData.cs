using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The variables can be changed in runtime and the change is saved, but only on Editor. On executables, they reset when you open and close the game.
// It is a class, so we can add methods

[CreateAssetMenu(fileName = "Item", menuName = "Class/Item")]
public class ItemData : ScriptableObject
{
    public string id;
    public string name;
    public Sprite icon;
    public int cost;
    public int maxStack;
    public GameObject ownerGO;
    
    public void Print()
    {
        Debug.Log($"id: {id}, name: {name}, cost: {cost}");
    }
}
