using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [SerializeField]
    private ItemData[] itemDatas;

    public Dictionary<string, ItemData> items;

    private void Awake()
    {
        if (!Instance && Instance != this)
            Destroy(this);
        Instance = this;

        items = new Dictionary<string, ItemData>(itemDatas.Length);
        foreach (ItemData item in itemDatas)
        {
            items.Add(item.id, item);
        }
    }

    public ItemData GetItem(string id)
    {
        if (items.TryGetValue(id, out ItemData item))
        {
            return item;
        }

        return null;
    }
}
