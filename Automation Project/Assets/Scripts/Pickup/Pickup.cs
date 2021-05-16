using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum Type { HEALTH, POINTS}
    public Type pickupType;
    [SerializeField]
    [Range(5f, 15f)]
    float cooldown = 10f;
    float currentCooldwon = 0f;
    protected MeshRenderer mRenderer;

    // Start is called before the first frame update
    void Start()
    {
        mRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (IsActive() == false)
        {
            if ((currentCooldwon += Time.deltaTime) >= cooldown)
            {
                currentCooldwon = 0f;
                Appear();
            }
        }

    }

    public void OnPlayer (GameObject go)
    {
        if (IsActive())
        {
            Disappear(go);
        }
    }

    void Appear ()
    {
        mRenderer.enabled = true;
    }

    public virtual void Disappear (GameObject player)
    {
        mRenderer.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject go = other.transform.parent.gameObject;
        if (go.CompareTag("Player"))
        {
            OnPlayer(go);
        }

    }

    public bool Active() => mRenderer.enabled;

    bool IsActive() => mRenderer.enabled;
}
