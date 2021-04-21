using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperParameters : WeaponParameters
{

    [SerializeField]
    [Range(0.3f, 0.7f)]
    float aimSpeedDecrease = 0.5f;
    public float _aimSpeedDecrease() => aimSpeedDecrease;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override float GetAimMulti()
    {
        return aimSpeedDecrease;
    }
}
