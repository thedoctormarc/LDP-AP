﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Simulation.Games;
using Pathfinding;


public class Analytics : MonoBehaviour
{
    [SerializeField]
    int pixelRadius = 1;

    [SerializeField]
    Texture2D mapTexture;

    public static Analytics instance;

    Dictionary<GameObject, List<Vector3>> deaths;
    Dictionary<GameObject, List<Vector3>> positions;
    Dictionary<GameObject, System.Tuple<float, List<float>>> ttk;
    Dictionary<GameObject, int> pickups;

    public float positionIntervalSec = 0f;

    [SerializeField]
    Vector2 offset;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        positions = new Dictionary<GameObject, List<Vector3>>();
        deaths = new Dictionary<GameObject, List<Vector3>>();
        pickups = new Dictionary<GameObject, int>();
        ttk = new Dictionary<GameObject, System.Tuple<float, List<float>>>();
   

        for (int i = 0; i < PlayerManager.instance.transform.childCount; ++i)
        {
            ttk.Add(PlayerManager.instance.transform.GetChild(i).gameObject, new System.Tuple<float, List<float>>(0, new List<float>()));
        }
    }

    // Update is called once per frame
    void Update()
    {

        UpdateTTK();
    }

    public void OnDeath(GameObject dead, GameObject killer)
    {
        AddEntry_Pos(deaths, dead);
        InputTTK(killer);
    }

    public void OnPositionChange(GameObject go)
    {
        AddEntry_Pos(positions, go);
    }

    void AddEntry_Pos(Dictionary<GameObject, List<Vector3>> dict, GameObject go)
    {
        if (dict.ContainsKey(go))
        {
            dict[go].Add(go.transform.position);
        }
        else
        {
            dict.Add(go, new List<Vector3> { go.transform.position });
        }
    }

    public void OnPickup(GameObject pickup)
    {
        if (pickups.ContainsKey(pickup))
        {
            ++pickups[pickup];
        }
        else
        {
            pickups.Add(pickup, 1);
        }
    }

    public void OnAppShutdown()
    {

        if (AppManager.instance._gameSimulation())
        {
            return;
        }

        foreach (KeyValuePair<GameObject, List<Vector3>> element in deaths)
        {
            GenerateDeathmap(element);
        }

        foreach (KeyValuePair<GameObject, List<Vector3>> element in positions)
        {
            GenerateHeatmap(element);
        }

        ComputeTTK();
        GeneratePickupMap();
       
    }


    void InputTTK(GameObject go)
    {
        // Add a new time to the ttk list, set as the current time
        var tuple = ttk[go];
        var times = tuple.Item2;
        times.Add(tuple.Item1);
        
        // Reset current time
        ttk[go] = new System.Tuple<float, List<float>>(0f, times);
    }

    void UpdateTTK ()
    {
        var copy = new Dictionary<GameObject, System.Tuple<float, List<float>>>(ttk);

        foreach (var element in copy)
        {
            // Current time is incremented by app delta time
            var time = element.Value.Item1;
            time += Time.deltaTime;
            var tuple = new System.Tuple<float, List<float>>(time, new List<float>(element.Value.Item2));
            ttk[element.Key] = tuple;
        }
    }

    void ComputeTTK() // time to kill, fore each team!!
    {
        // Compute ttk for each gameObject --> store team and ttk
        List<System.Tuple<int, float>> ttk_computed = new List<System.Tuple<int, float>>();

        foreach (var element in ttk)
        {
            var list = element.Value.Item2;
            float median = 0f;

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    median += item;
                }
                median /= list.Count;
            }
           else
            {
                median = 0f;
            }

            ttk_computed.Add(new System.Tuple<int, float>(element.Key.GetComponent<Parameters>()._team(), median));
        }

        // For each team --> calculate median per team,
        Dictionary<int, float> ttk_team = new Dictionary<int, float>();
        Dictionary<int, int> ttk_times = new Dictionary<int, int>();

        foreach (var element in ttk_computed)
        {
            //  skip players with no ttk(no kills)
            if (element.Item2 == 0f)
            {
                continue;
            }

            if (ttk_team.ContainsKey(element.Item1))
            {
                ttk_team[element.Item1] += element.Item2;
                ttk_times[element.Item1]++;
            }
            else
            {
                ttk_team.Add(element.Item1, element.Item2);
                ttk_times.Add(element.Item1, 1);
            }
        }

        // Actual Median

        var copy = new Dictionary<int, float>(ttk_team);
        foreach (var element in copy)
        {
            // Crashes, can't modify within foreach
            ttk_team[element.Key] /= (float)ttk_times[element.Key];

            // Game Simulation 
            string ttk_name = "T" + element.Key.ToString() + " TTK";
            GameSimManager.Instance.SetCounter(ttk_name, (long)ttk_team[element.Key]);

            // Debug
            Debug.Log(ttk_name + " has been " + ttk_team[element.Key].ToString() + " seconds!!");
        }

      
    }

    void DrawCircle(Texture2D tex, Color c, int radius, Vector2 pixelPos)
    {
        tex.SetPixel((int)pixelPos.x, (int)pixelPos.y, c);

        for (int y = -radius; y <= radius; ++y)
        {
            for (int x = -radius; x <= radius; ++x)
            {
                if (x * x + y * y <= radius * radius)
                {
                    if (PixelWithinLimits(tex.width, tex.height, (int)pixelPos.x + x, (int)pixelPos.y + y))
                    {
                        tex.SetPixel((int)pixelPos.x + x, (int)pixelPos.y + y, c);
                    }
                }
            }
        }

        tex.Apply();
    }

    Texture2D GenerateMapTexture()
    {
        int width = (int)AstarPath.active.data.gridGraph.width;
        int height = (int)AstarPath.active.data.gridGraph.depth;
        return new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    void SaveTexture (Texture2D tex, string name)
    {
        TextureScale.Point(tex, tex.width * 10, tex.height * 10);

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        // Save as an image file
        string folderName =  DateTime.UtcNow.ToString("yyyy - MM - dd HH: mm") + "_" + AppManager.instance._gMode().ToString();
        string folderNameFormated = folderName.Replace("/", " ");
        string folderNameFormated2 = folderNameFormated.Replace(":", " ");
        string path = Application.dataPath + "/../" + "Analytics/" + folderNameFormated2;

        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
        }

        File.WriteAllBytes(path + name, bytes);
    }

    void InitializeTexture (Texture2D tex)
    {
        Texture2D source = new Texture2D(mapTexture.width, mapTexture.height);
        source.SetPixels(mapTexture.GetPixels());
        TextureScale.Point(source, tex.width, tex.height);
        tex.SetPixels(source.GetPixels());
        tex.Apply();
    }



    // Map needs to be located at position (0,0,0)
    void GenerateDeathmap(KeyValuePair<GameObject, List<Vector3>> deaths) // https://docs.unity3d.com/ScriptReference/ImageConversion.EncodeToPNG.html
    {
        Texture2D tex = GenerateMapTexture();
        InitializeTexture(tex);

        foreach (Vector3 position in deaths.Value)
        {
            Vector2 pixelPos = new Vector2(position.x + (float)tex.width / 2f, position.z + (float)tex.height / 2f);
            pixelPos += offset;
            DrawCircle(tex, Color.red, pixelRadius, pixelPos);
        }

        SaveTexture(tex, "/ " + deaths.Key.name + "_deathmap.png");

    }

    void GenerateHeatmap(KeyValuePair<GameObject, List<Vector3>> positions)
    {
        Texture2D tex = GenerateMapTexture();
        InitializeTexture(tex);

        Dictionary<Vector2, int> pixelPosRepetitions = new Dictionary<Vector2, int>();

        foreach (Vector3 position in positions.Value)
        {
            Vector3 nodePos = (Vector3)AstarPath.active.data.gridGraph.GetNearest(position).node.position;
            Vector2 pixelPos = new Vector2(nodePos.x + (float)tex.width / 2f, nodePos.z + (float)tex.height / 2f);
            pixelPos += offset;

            if (pixelPosRepetitions.ContainsKey(pixelPos) == false)
            {
                pixelPosRepetitions.Add(pixelPos, 1);
            }
            else
            {
                ++pixelPosRepetitions[pixelPos];
            }
           
        }

        // Search for most repeated position that will have a certain color
        int max = 0;
        int min = int.MaxValue;

        foreach (var pos in pixelPosRepetitions)
        {
            if (pos.Value > max)
            {
                max = pos.Value;
            }
            else if (pos.Value < min)
            {
                min = pos.Value;
            }
        }

        // Tint pixels using a 1 to max scale

        foreach (var pos in pixelPosRepetitions)
        {
            Color c = Color.black;

            float value = ((((float)pos.Value - (float)min) * (1f - 0f)) / ((float)max - (float)min)) + 0f;
            c.r = value;
            c.g = 1f - value;
            tex.SetPixel((int)pos.Key.x, (int)pos.Key.y, c);
        }


        SaveTexture(tex, "/ " + positions.Key.name + "_heatmap.png");
    }

    void GeneratePickupMap()
    {
        Texture2D tex = GenerateMapTexture();
        InitializeTexture(tex);

        foreach (var element in pickups)
        {
            Vector3 position = element.Key.transform.position;
            Vector2 pixelPos = new Vector2(position.x + (float)tex.width / 2f, position.z + (float)tex.height / 2f);
            pixelPos += offset;
            Pickup.Type type = element.Key.GetComponent<Pickup>().pickupType;
            DrawCircle(tex, (type == Pickup.Type.HEALTH) ? Color.red : Color.yellow, element.Value * pixelRadius, pixelPos);
        }

        SaveTexture(tex, "/Pickup Map.png");
    }

    bool PixelWithinLimits (float width, float height, int x, int y)
    {
        return x >= 0 && y >= 0 && x <= width && y <= height;
    }


}
