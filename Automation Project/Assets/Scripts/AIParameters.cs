using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Simulation.Games;

public class AIParameters : Parameters  // TODO: child classes depending on weapon and psychology
{
    public enum Player_Type { killer, collector, socializer }

    [SerializeField]
    Player_Type pType;

    [SerializeField]
    [Range(0f, 1f)]
    float aimSpread = 0.8f;

    [SerializeField]
    [Range(0f, 1f)]
    float aimSpeed = 0.5f;

    [SerializeField]
    [Range(1.1f, 2f)]
    float aimSpeedMultiAudio = 1.5f;

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
    [Range(10f, 25f)]
    float searchPickupHealth = 20f;

    [SerializeField]
    [Range(10f, 20f)]
    float audioPerceptionRadius = 15f;

    [SerializeField]
    [Range(0f, 10f)]
    float timeUntilDeAggro = 8f;

    [SerializeField]
    [Range(3f, 10f)]
    float shotgunTargetFightDist = 5f;

    [SerializeField]
    [Range(10f, 20f)]
    float rifleCoverMinTriggerDist = 15f;

    [SerializeField]
    [Range(5f, 10f)]
    float rifleCoverMaxDist = 8f;

    [SerializeField]
    [Range(0f, 5f)]
    float killerInspectTime = 2f;


    public Player_Type _pType() => pType;
    public float _aimSpread() => aimSpread;
    public float _aimSpeed() => aimSpeed;
    public float _headPositionOffset() => headPositionOffset;
    public float _waistPositionOffset() => waistPositionOffset;
    public float _maxViewAngle() => maxViewAngle;
    public float _visualRefreshTime() => visualRefreshTime;
    public float _auditiveRefreshTime() => auditiveRefreshTime;
    public float _audioPerceptionRadius() => audioPerceptionRadius;
    public float _timeUntilDeAggro() => timeUntilDeAggro;
    public float _shotgunTargetFightDist() => shotgunTargetFightDist;
    public float _aimSpeedMultiAudio() => aimSpeedMultiAudio;
    public float _rifleCoverMinTriggerDist() => rifleCoverMinTriggerDist;
    public float _rifleCoverMaxDist() => rifleCoverMaxDist;
    public float _killerInspectTime() => killerInspectTime;

    private void Start()
    {
        ResetHealth();
        GameSimManager.Instance.FetchConfig(OnConfigFetched);
    }

    void OnConfigFetched(GameSimConfigResponse config)
    {
        aimSpread = config.GetFloat("aimSpread");
        respawnTime = config.GetFloat("respawnTime");
        maxViewAngle = config.GetFloat("maxViewAngle");
    }

    public bool NeedHealth() => currentHealth <= searchPickupHealth;
}
