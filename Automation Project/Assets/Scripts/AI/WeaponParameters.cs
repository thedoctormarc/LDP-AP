using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponParameters : MonoBehaviour
{
    [SerializeField]
    float fireTime = 0.15f;
    public float _fireTime() => fireTime;
    [SerializeField]
    [Range(10f, 100f)]
    float damage = 25f;
    public float _damage() => damage;
    [SerializeField]
    AnimationCurve damageFallOff;
    [SerializeField]
    [Range(10f, 200f)]
    float maxEffectiveDistance = 200f;
    [SerializeField]
    [Range(5, 30)]
    int capacity = 10;
    public int _capacity() => capacity;
    [SerializeField]
    float reloadTime = 2f;
    public float _reloadTime() => reloadTime;

    [SerializeField]
    [Range(0f, 1f)]
    float spread = 0.5f;
    public float _spread() => spread;

    public float GetDamageAtDistance (float distance)
    {
        if (distance > 100f)
            distance = 100f;

        distance /= 100f; // 100 m = 1 anim unit
        float percentage = damageFallOff.Evaluate(distance);

        return damage * percentage; 
    }

    public bool InRange (GameObject from, GameObject to)
    {
        return (from.transform.position - to.transform.position).magnitude <= maxEffectiveDistance;
    }


    virtual public float GetAimMulti()
    {
        return 1f;
    }

}
