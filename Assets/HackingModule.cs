using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingModule : MonoBehaviour
{
    public event Action PlayerInRange;
    [SerializeField] private GameObject pressE;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pressE.SetActive(true);
            PlayerInRange?.Invoke();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pressE.SetActive(false);
            PlayerInRange?.Invoke();
        }
    }
}

