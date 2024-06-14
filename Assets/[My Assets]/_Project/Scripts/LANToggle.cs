using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LANToggle : MonoBehaviour
{
    [SerializeField] private GameObject _buttons;
    [SerializeField] private Toggle _toggle;
    
    private void Start()
    {
        if (PlayerPrefs.GetInt("LAN") == 1)
        {
            _toggle.isOn = true;
            _buttons.SetActive(true);
        }
        else
        {
            _toggle.isOn = false;
            _buttons.SetActive(false);
        }
    }

    public void SetLANState(bool enabled)
    {
        _toggle.isOn = enabled;
        _buttons.SetActive(enabled);
        if (enabled)
            PlayerPrefs.SetInt("LAN", 1);
        else
        {
            PlayerPrefs.SetInt("LAN", 0);
        }
    }
}
