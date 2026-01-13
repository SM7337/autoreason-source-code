using UnityEngine;

public class CarAI : MonoBehaviour
{
    public float speedKPH;
    public float maxSpeedKPH;
    public float minSpeedKPH;
    
    public Rigidbody rb;
    public Transform com;
    
    [Header("Wheel Colliders")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Wheel Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Driving Settings")]
    public float motorTorque = 200f;
    public float brakeTorque = 300f;
    public float steerStrength = 30f;
    
    public AudioSource engineSound;

    [Header("AI Settings")]
    public float rayDistance = 4.5f;
    public float forwardRayDistance = 7f;
    public Vector3 rayOffset;
    
    float steerInput = 0f;
    float brakeInput = 0f;
    
    [HideInInspector]
    public Vector3 leftDir;
    public Vector3 rightDir;
    public RaycastHit frontHit, leftHit, rightHit;
    public bool isBraking;
    public float steerOutput;

    private void Start()
    {
        rb.centerOfMass = com.localPosition;
        engineSound.Play();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
            GetComponent<CarDebugScript>().showDebug = !GetComponent<CarDebugScript>().showDebug;
        
        PlayEngineSound();
    }
    
    void FixedUpdate()
    {
        speedKPH = rb.linearVelocity.magnitude * 3.6f;
        Sense();
        Drive();
        UpdateWheels();
    }

    // --- AI Logic ---
    void Sense()
    {
        brakeInput = 0f;
        
        leftDir  = Quaternion.AngleAxis(-25f, transform.up) * transform.forward;
        rightDir = Quaternion.AngleAxis(25f,  transform.up) * transform.forward;
        
        bool frontDetected = Physics.Raycast(transform.position + rayOffset, transform.forward, out frontHit, forwardRayDistance);
        bool leftDetected = Physics.Raycast(transform.position + rayOffset, leftDir, out leftHit, rayDistance);
        bool rightDetected = Physics.Raycast(transform.position + rayOffset, rightDir, out rightHit, rayDistance);

        if (frontDetected)
        {
            if (frontHit.distance < 5f)
            {
                brakeInput = brakeTorque * 5;
            }
            else if (speedKPH > minSpeedKPH)
            {
                brakeInput = brakeTorque;
            }
            else
            {
                brakeInput = 0f;
            }
        }
        else
        {
            brakeInput = 0;
        }

        float leftDist  = leftDetected  ? leftHit.distance  : rayDistance;
        float rightDist = rightDetected ? rightHit.distance : rayDistance;
        
        float diff = rightDist - leftDist;  
        
        float targetSteer = Mathf.Clamp(diff * 15f, -steerStrength, steerStrength);
        
        steerInput = Mathf.Lerp(steerInput, targetSteer, Time.fixedDeltaTime * 60f);
    }

    // --- Car Control ---
    void Drive()
    {
        isBraking = brakeInput > 0;
        steerOutput = steerInput * 90 / 30;
            
        // Steering
        frontLeft.steerAngle = steerInput;
        frontRight.steerAngle = steerInput;

        // Braking
        frontLeft.brakeTorque = brakeInput;
        frontRight.brakeTorque = brakeInput;
        rearLeft.brakeTorque = brakeInput;
        rearRight.brakeTorque = brakeInput;

        // Acceleration
        if (brakeInput == 0f && maxSpeedKPH > speedKPH)
        {
            rearLeft.motorTorque = motorTorque;
            rearRight.motorTorque = motorTorque;
        }
        else
        {
            rearLeft.motorTorque = 0f;
            rearRight.motorTorque = 0f;
        }
    }

    // --- Visual Wheels ---
    void UpdateWheels()
    {
        UpdateWheel(frontLeft, frontLeftMesh);
        UpdateWheel(frontRight, frontRightMesh);
        UpdateWheel(rearLeft, rearLeftMesh);
        UpdateWheel(rearRight, rearRightMesh);
    }

    void UpdateWheel(WheelCollider col, Transform mesh)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
    
    private void PlayEngineSound()
    {
        if (engineSound == null) return;

        float maxSpeed = 40f;

        // Clamp speed to expected range
        float normalizedSpeed = Mathf.Clamp01(speedKPH / maxSpeed);

        // Idle settings
        float idlePitch = 0.8f;
        float maxPitch = 1.6f;

        float idleVolume = 0.2f;
        float maxVolume = 0.8f;

        // Apply sound changes
        engineSound.pitch = Mathf.Lerp(idlePitch, maxPitch, normalizedSpeed);
        engineSound.volume = Mathf.Lerp(idleVolume, maxVolume, normalizedSpeed);

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (engineSound.isPlaying)
            {
                engineSound.Stop();
            }
            else
            {
                engineSound.Play();
            }
        }
    }
}
