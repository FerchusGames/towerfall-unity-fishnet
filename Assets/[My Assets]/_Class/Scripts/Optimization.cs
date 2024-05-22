using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = System.Object;
using Random = UnityEngine.Random;

public class Optimization : MonoBehaviour
{
    // Invokes are more efficient than coroutines
    // If we use coroutines, we have to be careful with the new Waitxxx
    // We should use a cache to references when possible
    // It is also better to have a reference to the transform
    // If we want to check if one reference is not null, it is better to use it as if it was a boolean
    // We shouldn't use with references to components the miReference?.variable.
    
    private WaitForSeconds _ws3;
    [SerializeField] private MeshRenderer _renderer;
    private Transform _transform;
    Object lockVida;
    
    private void Start()
    {
        _transform = transform;
        _ws3 = new WaitForSeconds(3f);
        
        StartCoroutine(Move(5f));
        Invoke(nameof(MoveByInvoke), 3f);
        // Invoke are lighter and more efficient

        int[] array = NumberArray(10000);
        foreach (int number in array)
        {
            print(number);
        }

        foreach (int number in NumberArrayEnumerable(10000))
        {
            print(number);
            
            if (number == 50)
                break;
        }

        CalculateNumber();
        print("This is called before CalculateNumber");
    }

    async void CalculateNumber() // Can't use Unity stuff, doesn't run in the Unity Main Thread
    {
        Texture texture = new Texture2D(256, 256); // Dynamic memory, we need to look for a way to detect that the async has been cancelled
        lock (lockVida)
        {
            //Add
        }
        int number = 0;
        for (int i = 0; i < 10000; i++)
        {
            number += Random.Range(1, 10000);
        }

        await Task.Delay(1000); // 1000 miliseconds = 1 second
        int average = await CalculateAverage(number, 10000); // Yield return Wait CalculateAverage
        
        print(number);
    }

    async Task<int> CalculateAverage(int number, int amount)
    {
        return number / amount;
    }
    
    IEnumerable NumberArrayEnumerable(int amount)
    {
        for (int i = 0; i < amount; i++) 
        {
            int value = Random.Range(1, 100); // We only use 1 int in RAM
            yield return value;
        }
    }
    
    private int[] NumberArray(int amount)
    {
        int[] array = new int[amount]; // RAM
        for (int i = 0; i < amount; i++) // CPU
        {
            array[i] = Random.Range(1, 100);
        }

        return array;
    }
    
    private void MoveByInvoke()
    {
        transform.Translate(Vector3.forward * 2f);
        if (_renderer) // if (_renderer != null) // Also applies if it is == null
            _renderer.enabled = !_renderer.enabled;

        if (!_renderer)
        {
            _renderer = GetComponent<MeshRenderer>();
        }
    }
    
    IEnumerator Move(float delay)
    {
        yield return null;
        transform.Translate(Vector3.forward * 2f);
        yield return _ws3; 
        transform.Rotate(Vector3.up * 45f);
        yield return _ws3;
        transform.Translate(Vector3.forward * 2f);
        yield return null;
        
        WaitForSeconds wsDelay = new WaitForSeconds(delay);
        while (transform.position.z < 10f)
        {
            transform.Translate(Vector3.forward * 2f);
            yield return wsDelay;
        }
    }
}
