using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIParameters : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 1f)]
    float aimSpread = 0.5f;

    [SerializeField]
    [Range(0f, 1f)]
    float aimSpeed = 0.5f;

    [SerializeField]
    float headPositionOffset = 1.65f;

    [SerializeField]
    float waistPositionOffset = 0.9f;

    [SerializeField]
    [Range(90f, 180f)]
    float maxViewAngle = 120f;

    [SerializeField]
    [Range(0f, 0.5f)]
    float visualRefreshTime = 0.25f;

    [SerializeField]
    [Range(50f, 100f)]
    float maxHealth = 50f;
    public float currentHealth;

    [SerializeField]
    [Range(0f, 10f)]
    float respawnTime= 5f;
    

    public float _aimSpread() => aimSpread;
    public float _aimSpeed() => aimSpeed;
    public float _headPositionOffset() => headPositionOffset;
    public float _waistPositionOffset() => waistPositionOffset;
    public float _maxViewAngle() => maxViewAngle;
    public float _visualRefreshTime() => visualRefreshTime;
    public float _maxHealth() => maxHealth;
    public float _respawnTime() => respawnTime;

    private void Start()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
}
