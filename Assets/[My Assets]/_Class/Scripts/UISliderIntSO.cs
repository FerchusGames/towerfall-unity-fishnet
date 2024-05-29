using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISliderIntSO : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private IntSO _intSO;
    
    private void Awake()
    {
        _intSO.OnValueChange += OnValueChange;
    }

    private void OnValueChange(int value)
    {
        _slider.value = (float)_intSO.Value / _intSO.maxValue;
    }
}
