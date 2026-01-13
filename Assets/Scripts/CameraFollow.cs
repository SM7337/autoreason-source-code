using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public bool simulationMode;
    
    public Transform carTransform;
    public Camera cam;

    [Header("Camera Settings")]
    public float distance = 6f;
    public float height = 2f;
    public float mouseSensitivity = 3f;

    [Header("Smoothing")]
    public float followSmooth = 5f;
    public float rotationSmooth = 6f;
    public float recenterDelay = 1.5f;

    float yaw;
    float pitch = 10f;
    float idleTimer;


    [Header("Creator Mode Values")]
    public float creatorSpeed;
    public float scrollSpeed;
    public float x;
    public float z;
    public float minSize;
    public float maxSize;
    private float currentSize;

    void Start()
    {
        cam.orthographic = true;
        currentSize = maxSize;
        yaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (simulationMode)
        {
            if (carTransform == null)
                carTransform = GameObject.FindGameObjectWithTag("LookAt").transform;
            cam.orthographic = false;
            Cursor.lockState = CursorLockMode.Locked;
            Sim();
        }
        else
        {
            cam.orthographic = true;
            Create();
        }
    }

    void Sim()
    {
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");


        if (Mathf.Abs(mx) > 0.01f || Mathf.Abs(my) > 0.01f)
        {
            idleTimer = 0f;
            if (Input.GetMouseButton(0))
            {
                yaw += mx * mouseSensitivity * 100f * Time.deltaTime;
                pitch -= my * mouseSensitivity * 100f * Time.deltaTime;
            }
        }
        else
        {
            idleTimer += Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, -20f, 45f);

        // Auto-recenter behind vehicle
        if (idleTimer > recenterDelay)
        {
            yaw = Mathf.LerpAngle(
                yaw,
                carTransform.eulerAngles.y,
                Time.deltaTime * rotationSmooth
            );
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPosition =
            carTransform.position
            - rotation * Vector3.forward * distance
            + Vector3.up * height;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * followSmooth
        );

        transform.LookAt(carTransform.position + Vector3.up * 1.5f);
    }

    void Create()
    {
        // Zoom
        currentSize = Mathf.Clamp(
            currentSize - Input.GetAxis("Mouse ScrollWheel") * scrollSpeed,
            minSize,
            maxSize
        );
        cam.orthographicSize = currentSize;

        // Middle mouse drag
        if (Input.GetMouseButton(2))
        {
            float mouseX = -Input.GetAxis("Mouse X") * creatorSpeed * Time.deltaTime;
            float mouseY = -Input.GetAxis("Mouse Y") * creatorSpeed * Time.deltaTime;

            Vector3 movement = new Vector3(mouseX, 0f, mouseY);
            transform.position += movement;
        }

        // Clamp position
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -x, x);
        pos.z = Mathf.Clamp(pos.z, -z, z);
        pos.y = 90f;

        transform.position = pos;
    }
}