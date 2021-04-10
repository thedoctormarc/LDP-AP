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
    float aimTime = 0.5f;

    [SerializeField]
    Vector3 weaponTipOffset;

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


    public float _aimSpread() => aimSpread;
    public float _aimTime() => aimTime;
    public Vector3 _weaponTipOffset() => weaponTipOffset;
    public float _headPositionOffset() => headPositionOffset;
    public float _waistPositionOffset() => waistPositionOffset;
    public float _maxViewAngle() => maxViewAngle;
    public float _visualRefreshTime() => visualRefreshTime;
}
