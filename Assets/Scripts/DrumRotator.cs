using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public enum RotationAxis
{
    Yaw,
    Pitch,
    Roll
}

[System.Serializable]
public class RotationConfig
{
    public float speed;
    public bool clockwise;
    public float duration;
    public string externalRotationAxis;
}

[System.Serializable]
public class RotationConfigList
{
    public List<RotationConfig> rotationConfigs;
}

public class DrumRotator : MonoBehaviour
{
    public GameObject drum;
    public Quaternion initialRotation;

    public string configFilePath = "rotationConfigs.json";
    private int currentIndex = 0;
    private List<RotationConfig> configs;

    private Vector3 StringToAxis(string axisName)
    {
        switch (axisName)
        {
            case "Pitch":
                return Vector3.right;
            case "Yaw":
                return Vector3.up;
            case "Roll":
                return Vector3.forward;
            default:
                return Vector3.zero;
        }
    }

    void Start()
    {
        drum = this.gameObject;
        initialRotation = drum.transform.rotation;

        configs = LoadRotationConfigsFromJson(configFilePath);

        if (configs != null)
        {
            StartCoroutine(RotateDrum());
        }
        else
        {
            Debug.LogError("Failed to load rotation configs from " + configFilePath);
        }
    }

    private List<RotationConfig> LoadRotationConfigsFromJson(string path)
    {
        if (File.Exists(path))
        {
            string jsonText = File.ReadAllText(path);
            var configList = JsonUtility.FromJson<RotationConfigList>(jsonText);

            if (configList != null)
            {
                return configList.rotationConfigs;
            }
        }

        return null;
    }

    private IEnumerator RotateDrum()
    {
        if (configs == null)
        {
            throw new Exception("Rotation configs list is null.");
        }

        while (true)
        {
            RotationConfig config = configs[currentIndex];

            if (config == null)
            {
                throw new Exception("Rotation config is null.");
            }

            // Reset the rotation to the initial rotation
            drum.transform.rotation = initialRotation;

            Vector3 axis = StringToAxis(config.externalRotationAxis);

            // // Convert speed from degrees/second to degrees/frame
            // float speedPerFrame = config.speed / 60f;

            // Assuming deltaTime represents the time elapsed since the last frame
            float speedPerSecond = config.speed; // Speed in degrees/second
            float speedPerFrame = speedPerSecond * Time.deltaTime; // Speed in degrees/frame

            // Adjust speed for clockwise/counter-clockwise
            if (!config.clockwise)
            {
                speedPerFrame *= -1;
            }

            // Calculate total rotation
            float totalRotation = 0;
            while (totalRotation < (config.speed * config.duration))
            {
                drum.transform.Rotate(axis, speedPerFrame);
                totalRotation += Math.Abs(speedPerFrame);
                yield return null;
            }
            Debug.Log("Finished rotation " + currentIndex + " of " + configs.Count);

            // Increment index or reset to 0 if end of list
            currentIndex = (currentIndex + 1) % configs.Count;
        }
    }
}
