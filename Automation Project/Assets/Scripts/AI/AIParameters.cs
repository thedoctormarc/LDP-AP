using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Simulation.Games;

public class AIParameters : Parameters  
{
    public enum Player_Type { killer, collector, socializer }

    [SerializeField]
    Player_Type pType;

    [SerializeField]
    [Range(0.3f, 2f)]
    float aimSpeed = 1.5f;

    [SerializeField]
    [Range(1.1f, 2f)]
    float aimSpeedMultiAudio = 1.5f;

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
    [Range(10f, 25f)]
    float searchPickupHealth = 20f;

    [SerializeField]
    [Range(10f, 20f)]
    float audioPerceptionRadius = 15f;

    [SerializeField]
    [Range(5f, 15f)]
    float shotgunTargetFightDist = 12f;

    [SerializeField]
    [Range(10f, 20f)]
    float rifleCoverMinTriggerDist = 15f;

    [SerializeField]
    [Range(5f, 10f)]
    float rifleCoverMaxDist = 8f;


    public Player_Type _pType() => pType;
    public float _aimSpeed() => aimSpeed;
    public float _maxViewAngle() => maxViewAngle;
    public float _visualRefreshTime() => visualRefreshTime;
    public float _auditiveRefreshTime() => auditiveRefreshTime;
    public float _audioPerceptionRadius() => audioPerceptionRadius;
    public float _shotgunTargetFightDist() => shotgunTargetFightDist;
    public float _aimSpeedMultiAudio() => aimSpeedMultiAudio;
    public float _rifleCoverMinTriggerDist() => rifleCoverMinTriggerDist;
    public float _rifleCoverMaxDist() => rifleCoverMaxDist;

    private void Start()
    {
        ResetHealth();
        GameSimManager.Instance.FetchConfig(OnConfigFetched);
    }

    void OnConfigFetched(GameSimConfigResponse config)
    {
        respawnTime = config.GetFloat("respawnTime");
        maxViewAngle = config.GetFloat("maxViewAngle");
    }

    public bool NeedHealth() => currentHealth <= searchPickupHealth;
}
