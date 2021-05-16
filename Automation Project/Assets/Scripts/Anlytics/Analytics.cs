using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Analytics : MonoBehaviour
{
    [SerializeField]
    [Range(1, 30)]
    int deathMapPixelRadius = 14;

    public static Analytics instance;

    Dictionary<GameObject, List<Vector3>> deaths;


    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        deaths = new Dictionary<GameObject, List<Vector3>>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDeath(GameObject go)
    {
        if (deaths.ContainsKey(go))
        {
            deaths[go].Add(go.transform.position);
        }
        else
        {
            deaths.Add(go, new List<Vector3> { go.transform.position } );
        }
    }

    public void OnAppShutdown()
    {
        foreach (KeyValuePair<GameObject, List<Vector3>> element in deaths)
        {
            GenerateDeathmap(element);
        }
    }


    // Map needs to be located at position (0,0,0)
    bool GenerateDeathmap(KeyValuePair<GameObject, List<Vector3>> deaths) // https://docs.unity3d.com/ScriptReference/ImageConversion.EncodeToPNG.html
    {
        // Create a texture 
        GameObject map = GameObject.FindGameObjectWithTag("map");
        int width = (int)(map.transform.localScale.x * 1000f);
        int height = (int)(map.transform.localScale.z * 1000f);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Convert deathmap to pixel colors
        Color[] colors = new Color[width * height];

        for (int i = 0; i < width; ++i)
        {
            colors[i] = Color.black;
        }

        tex.SetPixels(colors);

        foreach (Vector3 position in deaths.Value)
        {
            Vector2 pixelPos = new Vector2(position.x + (float)width / 2f, position.z + (float)height / 2f);
            tex.SetPixel((int)pixelPos.x, (int)pixelPos.y, Color.red);

            for (int y = -deathMapPixelRadius; y <= deathMapPixelRadius; ++y)
            {
                for (int x = -deathMapPixelRadius; x <= deathMapPixelRadius; ++x)
                {
                    if (x * x + y * y <= deathMapPixelRadius * deathMapPixelRadius)
                    {
                        if (PixelWithinLimits(width, height, (int)pixelPos.x + x, (int)pixelPos.y + y))
                        {
                            tex.SetPixel((int)pixelPos.x + x, (int)pixelPos.y + y, Color.red);
                        }
                    }
                }
            }    
        }

        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Object.Destroy(tex);

        // Save as an image file
        File.WriteAllBytes(Application.dataPath + "/Analytics" + deaths.Key.name + " deathmap.png", bytes);

        return true;
    }

    bool PixelWithinLimits (float width, float height, int x, int y)
    {
        return x >= 0 && y >= 0 && x <= width && y <= height;
    }


}
