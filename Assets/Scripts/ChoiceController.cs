using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

public class ChoiceController : MonoBehaviour, ISceneController
{
    // Assuming these prefabs are assigned in the Unity Editor
    public GameObject[] prefabs;
    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    public Material[] materials; // Materials are assigned in the Unity Editor
    private Dictionary<string, Material> materialDict = new Dictionary<string, Material>();

    private void Awake()
    {
        // Initialize prefab dictionary
        foreach (var prefab in prefabs)
        {
            if (!prefabDict.ContainsKey(prefab.name))
            {
                prefabDict.Add(prefab.name, prefab);
                // Debug.Log("Added prefab: " + prefab.name);
            }
        }

        // Initialize material dictionary
        foreach (var material in materials)
        {
            if (!materialDict.ContainsKey(material.name))
            {
                materialDict.Add(material.name, material);
            }
        }
    }

    public void InitializeScene(Dictionary<string, object> parameters)
    {
        Debugger.Log("InitializeScene called.");

        // Path to scene configuration JSON
        string configFile = parameters["configFile"].ToString();

        // Load and parse JSON
        string jsonPath = Path.Combine(Application.streamingAssetsPath, configFile);
        string jsonString = File.ReadAllText(jsonPath);
        SceneConfig config = JsonConvert.DeserializeObject<SceneConfig>(jsonString);

        // Instantiate objects
        foreach (var obj in config.objects)
        {
            if (prefabDict.TryGetValue(obj.type, out GameObject prefab))
            {
                Vector3 position = CalculatePosition(obj.position.radius, obj.position.angle);
                GameObject instance = Instantiate(prefab, position, Quaternion.identity);

                // Set scale, Optionally flip the object if flip is true, set flip my scale * -1 in x axis

                if (obj.flip)
                {
                    instance.transform.localScale = new Vector3(
                        obj.scale.x * -1,
                        obj.scale.y,
                        obj.scale.z
                    );
                }
                else
                {
                    instance.transform.localScale = new Vector3(
                        obj.scale.x,
                        obj.scale.y,
                        obj.scale.z
                    );
                }

                // Optionally apply material
                if (
                    !string.IsNullOrEmpty(obj.material)
                    && materialDict.TryGetValue(obj.material, out Material material)
                )
                {
                    instance.GetComponent<Renderer>().material = material;
                }
            }
        }
        ClosedLoop[] closedLoopComponents = FindObjectsOfType<ClosedLoop>();
        Debugger.Log("Number of ClosedLoop scripts found: " + closedLoopComponents.Length, 4);

        foreach (ClosedLoop cl in closedLoopComponents)
        {
            Debugger.Log(
                "Setting values for ClosedLoop script..." + config.closedLoopOrientation,
                4
            );
            cl.SetClosedLoopOrientation(config.closedLoopOrientation);
            cl.SetClosedLoopPosition(config.closedLoopPosition);
        }
        // Read and set the background color of cameras
        if (config.backgroundColor != null)
        {
            Color bgColor = new Color(
                config.backgroundColor.r,
                config.backgroundColor.g,
                config.backgroundColor.b,
                config.backgroundColor.a
            );
            Camera[] cameras = GameObject
                .FindGameObjectsWithTag("MainCamera")
                .Select(obj => obj.GetComponent<Camera>())
                .ToArray();

            foreach (Camera cam in cameras)
            {
                if (cam != null)
                {
                    cam.backgroundColor = bgColor;
                }
            }
        }
        // TODO: Set sky and grass textures
        // Start the coroutine from here
        StartCoroutine(DelayedOnLoaded(0.05f));
    }

    // Coroutine to delay the execution of OnLoaded
    private IEnumerator DelayedOnLoaded(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnLoaded();
    }

    private void OnLoaded()
    {
        ClosedLoop[] closedLoopComponents = FindObjectsOfType<ClosedLoop>();
        foreach (ClosedLoop cl in closedLoopComponents)
        {
            cl.ResetPosition();
            cl.ResetRotation();
        }
    }

    private Vector3 CalculatePosition(float radius, float angle)
    {
        float x = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        float z = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        return new Vector3(x, 0, z); // Assuming y is always 0
    }

    // Update SceneConfig and other classes as needed to reflect JSON changes
}

[System.Serializable]
public class SceneConfig
{
    public SceneObject[] objects;
    public bool closedLoopOrientation;
    public bool closedLoopPosition;
    public ColorConfig backgroundColor;
}

[System.Serializable]
public class SceneObject
{
    public string type;
    public Position position;
    public string material;
    public ScaleConfig scale;
    public bool flip;
    // Include other properties as before
}

[System.Serializable]
public class Position
{
    public float radius;
    public float angle;
}

[System.Serializable]
public class ScaleConfig
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class ColorConfig
{
    public float r;
    public float g;
    public float b;
    public float a;
}
