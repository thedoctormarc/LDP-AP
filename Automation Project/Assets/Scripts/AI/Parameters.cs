﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour
{

    [SerializeField]
    [Range(0.3f, 1.5f)]
    float runningSpread = 0.8f;

    [Range(0, 2)]
    public int team;

    [SerializeField]
    [Range(50f, 100f)]
    protected float maxHealth = 50f;

    [HideInInspector]
    protected float currentHealth;

    [SerializeField]
    [Range(0f, 10f)]
    protected float respawnTime = 5f;

    [SerializeField]
    [Range(2.5f, 4f)]
    protected float walkSpeed = 1.5f;

    [SerializeField]
    [Range(2.5f, 4f)]
    protected float runSpeed = 3.5f;

    [HideInInspector]
    public int currentPoints = 0;

    [SerializeField]
    float headPositionOffset = 1.65f;

    [SerializeField]
    float waistPositionOffset = 0.9f;

    public int _team() => team;
    public float _maxHealth() => maxHealth;
    public float _respawnTime() => respawnTime;
    public float _walkSpeed() => walkSpeed;
    public float _runSpeed() => runSpeed;
    public float _currentHealth() => currentHealth;
    public float _headPositionOffset() => headPositionOffset;
    public float _waistPositionOffset() => waistPositionOffset;

    public float _runningSpread() => runningSpread;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void OnDeath()
    {
        ResetHealth();
    }

    public void UpdateHealth(float health)
    {
        currentHealth = (health > 0f) ? health : 0f;

        HumanController h = gameObject.GetComponent<HumanController>();

        if (h)
        {
            h.UpdateHealthBar();
        }
    }
}
