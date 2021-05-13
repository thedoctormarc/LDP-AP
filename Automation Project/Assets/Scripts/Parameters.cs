using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour
{
    [SerializeField]
    [Range(0, 2)]
    protected int team;

    [SerializeField]
    [Range(50f, 100f)]
    protected float maxHealth = 50f;

    [HideInInspector]
    protected float currentHealth;

    [SerializeField]
    [Range(0f, 10f)]
    protected float respawnTime = 5f;

    [SerializeField]
    [Range(1f, 2f)]
    protected float walkSpeed = 1.5f;

    [SerializeField]
    [Range(2.5f, 4f)]
    protected float runSpeed = 3.5f;

    [HideInInspector]
    public int currentPoints = 0;

    public int _team() => team;
    public float _maxHealth() => maxHealth;
    public float _respawnTime() => respawnTime;
    public float _walkSpeed() => walkSpeed;
    public float _runSpeed() => runSpeed;

    public float _currentHealth() => currentHealth;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void OnDeath()
    {
        ResetHealth();
    }

    public void UpdateHealth(float health) => currentHealth = (health > 0f) ? health : 0f;
}
