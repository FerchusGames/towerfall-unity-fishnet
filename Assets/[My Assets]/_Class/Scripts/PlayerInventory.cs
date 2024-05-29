using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public ItemData equippedItem;

    private void Start()
    {
        ItemData item = InventoryManager.Instance.GetItem("axe");
        
        item.Print();
        item.cost++;
    }
}
