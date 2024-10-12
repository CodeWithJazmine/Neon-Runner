using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCollider : MonoBehaviour
{
    public event Action<Collider> OnPlayerEnterFOV;
    public event Action<Collider> OnPlayerExitFOV;

    private void OnTriggerEnter(Collider other)
    {
        OnPlayerEnterFOV?.Invoke(other);

    }
    private void OnTriggerStay(Collider other)
    {
        OnPlayerEnterFOV?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        OnPlayerExitFOV?.Invoke(other);
    }

    
}
