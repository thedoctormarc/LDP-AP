using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponParameters : MonoBehaviour
{
    [SerializeField]
    float fireRate = 13.3f;
    public float _fireRate() => fireRate;
    [SerializeField]
    [Range(0f, 1f)]
    float damage = 0.5f;
    public float _damage() => damage;
}
