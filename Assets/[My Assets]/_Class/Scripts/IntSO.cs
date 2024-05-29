using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "IntSO", menuName = "Class/IntSO")]
public class IntSO : ScriptableObject
{
    public int InitialValue { get; private set; }
    public int maxValue;
    
    private int value;

    public int Value
    {
        get
        {
            return value;
        }
        set
        {
            if (value == this.value)
                return;
            this.value = Mathf.Clamp(this.value, 0, maxValue);
            OnValueChange?.Invoke(value);
        }
    }
    
    public Action<int> OnValueChange;
    
    private void OnEnable()
    {
        value = InitialValue;
    }
}
