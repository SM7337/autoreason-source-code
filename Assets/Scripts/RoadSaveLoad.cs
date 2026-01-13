using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
public class RoadObjectSaveData
{
    public string roadTag;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class RoadSaveWrapper
{
    public List<RoadObjectSaveData> roads;
}

public class RoadSaveLoad : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void DownloadFile(string filename, string data);
    [DllImport("__Internal")] private static extern void OpenFile(string gameObjectName);
#endif
    
    public string savePath;
    public RoadCreator rc;
    
    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "save.artrack");
    }
    
    public void Save(List<GameObject> roads)
    {
        List<RoadObjectSaveData> roadSaves = new();
        foreach (GameObject r in roads)
        {
            RoadScript rs = r.GetComponent<RoadScript>();
            RoadObjectSaveData rsd = new RoadObjectSaveData();
            rsd.roadTag = rs.rtag;
            rsd.position = r.transform.position;
            rsd.rotation = r.transform.rotation;
            roadSaves.Add(rsd);
        }
        
        RoadSaveWrapper wrapper = new RoadSaveWrapper { roads = roadSaves };

        string json = JsonUtility.ToJson(wrapper, true);
#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFile("save.artrack", json);
#else
        File.WriteAllText(savePath, json);
        Debug.Log("Saved roads to " + savePath);
#endif
    }

    public void LoadFile()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenFile(gameObject.name);
#else
        string json = File.ReadAllText(savePath);
        OnFileLoaded(json);
#endif
    }

    public void OnFileLoaded(string json)
    {
        RoadSaveWrapper wrapper = JsonUtility.FromJson<RoadSaveWrapper>(json);
        
        if (wrapper == null || wrapper.roads == null)
        {
            Debug.Log("Invalid save file.");
            return;
        }

        rc.Clear();

        List<GameObject> roads = new();
        
        foreach (RoadObjectSaveData rs in wrapper.roads)
        {
            foreach (RoadObject r in rc.roads)
            {
                if (r.roadTag == rs.roadTag)
                {
                    GameObject newRoad = Instantiate(r.obj, rs.position, rs.rotation);
                    newRoad.GetComponent<RoadScript>().stage = 2;
                    newRoad.GetComponent<RoadScript>().rtag = rs.roadTag;
                    roads.Add(newRoad);
                }
            }
        }

        foreach (GameObject r in roads)
        {
            rc.roadObjects.Add(r);
        }
        rc.placedStart = true;
        rc.placedEnd = true;
    }
}