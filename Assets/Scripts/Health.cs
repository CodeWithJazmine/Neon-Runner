using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int health;
    public int maxHealth = 3;
    public Slider healthSlider;

    void Start()
    {
        health = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        healthSlider.value = health;

        if (health <= 0)
        {
            GameManager.instance.YouLose();
        }
    }
}
