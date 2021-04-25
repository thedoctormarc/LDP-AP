﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Simulation.Games;

public class AIParameters : AI // TODO: child classes depending on weapon and psychology
{
    [SerializeField]
    [Range(0, 2)]
    int team;
    public int _team() => team;
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
    [Range(50f, 100f)]
    float maxHealth = 50f;

    [SerializeField]
    [Range(10f, 25f)]
    float searchPickupHealth = 20f;

    [HideInInspector]
    public float currentHealth;

    [SerializeField]
    [Range(0f, 10f)]
    float respawnTime = 5f;

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
    [Range(1f, 2f)]
    float walkSpeed = 1.5f;

    [SerializeField]
    [Range(2.5f, 4f)]
    float runSpeed = 3.5f;

    [SerializeField]
    [Range(10f, 20f)]
    float rifleCoverMinTriggerDist = 15f;

    [SerializeField]
    [Range(5f, 10f)]
    float rifleCoverMaxDist = 8f;

    [SerializeField]
    [Range(0f, 5f)]
    float killerInspectTime = 2f;

    [HideInInspector]
    public int currentPoints = 0;

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
    public float _timeUntilDeAggro() => timeUntilDeAggro;
    public float _shotgunTargetFightDist() => shotgunTargetFightDist;
    public float _walkSpeed() => walkSpeed;
    public float _runSpeed() => runSpeed;
    public float _aimSpeedMultiAudio() => aimSpeedMultiAudio;
    public float _rifleCoverMinTriggerDist() => rifleCoverMinTriggerDist;
    public float _rifleCoverMaxDist() => rifleCoverMaxDist;
    public float _killerInspectTime() => killerInspectTime;

    private void Start()
    {
       // GameSimManager.Instance.FetchConfig(OnConfigFetched);
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

    public bool NeedHealth() => currentHealth <= searchPickupHealth;
}
