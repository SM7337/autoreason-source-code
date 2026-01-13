using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadObject
{
    public string roadTag;
    public GameObject obj;
    public Vector3 offset;
}

public class RoadCreator : MonoBehaviour
{
    public RoadObject[] roads;

    public GameObject mainCar, placementCar;
    public GameObject selectedCar;
    
    public string currentRoadTag;
    public GameObject currentlySelected;
    public Vector3 currentOffset;
    public float currentRotation;
    public bool selected;
    public LayerMask groundLayer;
    public GameObject fullCanvas;
    
    public List<GameObject> roadObjects = new List<GameObject>();

    public Vector3 currentPosition;
    public bool canPlaceRoad;

    public bool placedStart, placedEnd;

    public RoadSaveLoad rsl;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void Update()
    {
        if (currentlySelected != null && selected)
        {
            Vector3 worldPos = GetMouseWorldPos();
            currentlySelected.transform.position = new Vector3(Mathf.Round(worldPos.x), 0,  Mathf.Round(worldPos.z));
            currentlySelected.transform.position += currentOffset;
            currentPosition = currentlySelected.transform.position;

            canPlaceRoad = CanPlaceCheck();
            RoadScript rs = currentlySelected.GetComponent<RoadScript>();

            rs.stage = canPlaceRoad ? 1 : 0;
            
            currentlySelected.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

            if (Input.GetMouseButtonDown(1))
            {
                currentRotation += 90;
                currentRotation = Mathf.DeltaAngle(0f, currentRotation);
            }
            
            if (Input.GetMouseButtonDown(0) && canPlaceRoad)
            {
                rs.stage = 2;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
                {
                    roadObjects.Add(currentlySelected);
                    
                    if (currentRoadTag == "end" && !placedEnd)
                    {
                        placedEnd = true;
                    }
                
                    if (currentRoadTag == "start" && !placedStart)
                    {
                        placedStart = true;
                    }
                    
                    currentlySelected = null;
                }
                
                if (!placedEnd)
                    Select(currentRoadTag);
            }
        }

        if (selectedCar != null)
        {
            fullCanvas.SetActive(false);

            Vector3 worldPos = GetMouseWorldPos();
            selectedCar.transform.position = new Vector3(Mathf.Round(worldPos.x), 1f, Mathf.Round(worldPos.z));
            selectedCar.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

            if (Input.GetMouseButtonDown(1))
            {
                currentRotation += 90f;
                currentRotation = Mathf.Repeat(currentRotation, 360f);
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int roadLayerMask = 1 << LayerMask.NameToLayer("Road");

            if (Physics.Raycast(ray, 1000f, roadLayerMask))
            {
                selectedCar.GetComponent<PlacementVisualsCarScript>().isGreen = true;
                if ((Input.GetMouseButtonDown(0)))
                {
                    Instantiate(mainCar, selectedCar.transform.position, selectedCar.transform.rotation);
                    Destroy(selectedCar);
                    Camera.main.GetComponent<CameraFollow>().simulationMode = true;
                    fullCanvas.SetActive(false);
                    GameController gc = GameObject.FindGameObjectWithTag("GameController")
                        .GetComponent<GameController>();
                    gc.carSpawnPosition = selectedCar.transform.position;
                    gc.carSpawnRotation = selectedCar.transform.rotation;
                    this.enabled = false;
                }
            }
            else
            {
                selectedCar.GetComponent<PlacementVisualsCarScript>().isGreen = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SaveTrack();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            LoadTrack();
        }
    }

    private void ResetPointer()
    {
        Destroy(currentlySelected);
        currentlySelected = null;
        currentRoadTag = "";
    }
    
    private Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);   // World Position
        }

        return Vector3.zero;
    }

    private bool CanPlaceCheck()
    {
        if (roadObjects.Count == 0)
            return true;

        if (currentlySelected.GetComponent<RoadScript>().isOverlapping)
            return false;

        foreach (GameObject road in roadObjects)
        {
            RoadScript rs = road.GetComponent<RoadScript>();
            
            Transform[] checkList = currentRoadTag switch
            {
                "straight" => rs.possibleTransformsStraight,
                "curve"    => rs.possibleTransformsCurve,
                "end" => rs.possibleTransformsEnd,
                "start" => rs.possibleTransformsEnd, 
                _ => rs.possibleTransformsStraight
            };

            foreach (Transform t in checkList)
            {
                // Match position?
                if (Vector3.Distance(currentPosition, t.position) < 0.01f)
                {
                    float wantedRot = t.eulerAngles.y;
                    float diff = Mathf.Abs(Mathf.DeltaAngle(currentRotation, wantedRot));

                    // Allow small tolerance so A and B can have DIFFERENT valid angles
                    if (diff < 1f)
                    {
                        return true; // This road approves placement
                    }
                }
            }
        }

        return false; // No road approved it
    }
    
    public void Select(string roadTag)
    {
        if (currentlySelected != null)
        {
            Destroy(currentlySelected);
        }

        if (placedEnd) return;
        
        if (!placedStart && roadTag != "start") return;
        
        if (placedStart  && roadTag == "start") return;
        
        GameObject searchedObj = null;
        foreach (RoadObject road in roads)
        {
            if (road.roadTag == roadTag)
            {
                searchedObj = road.obj;
                currentOffset = road.offset;
                break;
            }
        }
        
        GameObject go = Instantiate(searchedObj, transform);
        go.GetComponent<RoadScript>().rtag = roadTag;
        currentlySelected = go;
        currentRoadTag = roadTag;
        selected = true;
    }

    public void Undo()
    {
        ResetPointer();

        if (roadObjects.Count > 0)
        {
            GameObject tr = roadObjects[^1];
            if (tr.GetComponent<RoadScript>().rtag == "end")
            {
                placedEnd = false;
            }

            if (tr.GetComponent<RoadScript>().rtag == "start")
            {
                placedStart = false;
            }

            roadObjects.RemoveAt(roadObjects.Count - 1);
            Destroy(tr);
        }
        
        selected = false;
    }
    
    public void Clear()
    {
        ResetPointer();

        if (roadObjects.Count > 0)
        {
            placedEnd = false;
            placedStart = false;
            foreach (GameObject road in roadObjects)
            {
                Destroy(road);
            }
            
            roadObjects.Clear();
        }
        
        selected = false;
    }

    public void PlaceCar()
    {
        ResetPointer();
        if (!placedEnd)  return;
        selected = false;
        selectedCar = Instantiate(placementCar);
    }

    public void SaveTrack()
    {
        if (!placedEnd) return;
        rsl.Save(roadObjects);
    }

    public void LoadTrack()
    {
        rsl.LoadFile();
        Debug.Log("Loaded");
    }
}
