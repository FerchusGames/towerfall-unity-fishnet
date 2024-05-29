using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassCharacterController : MonoBehaviour
{
    public IntSO lifeSO;
    private GameSettings gameSetting;

    private void Start()
    {
        gameSetting = GameSettings.Instance;
        //gameSetting = ScriptableObject.Instantiate(GameSettings.Instance); Only to use copies of the value instead of a reference
    }

    private void Update()
    {
        // The operations in C# are made from left to right
        // Always multiply ints and floats first, then vectors or quaternions
        transform.Translate(Input.GetAxisRaw("Horizontal") * gameSetting.characterSpeed * Time.deltaTime, 0, 0);

        if (Input.GetKeyDown(KeyCode.D))
        {
            lifeSO.Value--;
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            lifeSO.Value++;
        }
    }
}
