﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    //    DontDestroyOnLoad(this);
    }

    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}