using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public static AppManager instance;

    public enum gameMode { AI, Human}

    [SerializeField]
    float simulationSeconds = 120f;

    [SerializeField]
    GameObject pickups;

    [SerializeField]
    gameMode gMode;

    public GameObject _pickups () => pickups;
    public gameMode _gMode() => gMode;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    /*    if (Time.realtimeSinceStartup >= simulationSeconds)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }*/
    }
}
