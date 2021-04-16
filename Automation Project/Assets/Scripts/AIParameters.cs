using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Simulation.Games;

public class AIParameters : AI
{
    [SerializeField]
    [Range(0, 1)]
    int team;
    public int _team() => team;
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
    [Range(0f, 0.5f)]
    float auditiveRefreshTime = 0.25f;

    [SerializeField]
    [Range(50f, 100f)]
    float maxHealth = 50f;

    [HideInInspector]
    public float currentHealth;

    [SerializeField]
    [Range(0f, 10f)]
    float respawnTime = 5f;

    [SerializeField]
    [Range(5f, 15f)]
    float audioPerceptionRadius = 10f;

    public float _aimSpread() => aimSpread;
    public float _aimSpeed() => aimSpeed;
    public float _headPositionOffset() => headPositionOffset;
    public float _waistPositionOffset() => waistPositionOffset;
    public float _maxViewAngle() => maxViewAngle;
    public float _visualRefreshTime() => visualRefreshTime;
    public float _auditiveRefreshTime() => auditiveRefreshTime;
    public float _maxHealth() => maxHealth;
    public float _respawnTime() => respawnTime;
    public float _audioPerceptionRadius() => audioPerceptionRadius;


    private void Start()
    {
        GameSimManager.Instance.FetchConfig(OnConfigFetched);
        ResetHealth();
    }

    void OnConfigFetched(GameSimConfigResponse config)
    {
        aimSpread = config.GetFloat("T" + team.ToString() + " aimSpread");
        maxViewAngle = config.GetFloat("T" + team.ToString() + " maxViewAngle");
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public override void OnDeath()
    {
        ResetHealth();
    }
}
