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

    [SerializeField]
    AnimationCurve damageFallOff;


    [SerializeField]
    [Range(10f, 200f)]
    float maxEffectiveDistance = 200f;

    public float GetDamageAtDistance (float distance)
    {
        if (distance > 100f)
            distance = 100f;

        distance /= 100f; // 100 m = 1 anim unit

        return damage * damageFallOff.Evaluate(distance); 
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
