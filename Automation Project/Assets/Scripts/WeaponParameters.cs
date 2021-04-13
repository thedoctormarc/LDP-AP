using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponParameters : MonoBehaviour
{
    [SerializeField]
    float fireRate = 13.3f;
    public float _fireRate() => fireRate;
    [SerializeField]
    [Range(10f, 100f)]
    float damage = 25f;
    public float _damage() => damage;
}
