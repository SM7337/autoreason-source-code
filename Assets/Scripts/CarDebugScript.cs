using UnityEngine;
using UnityEngine.SceneManagement;

public class CarDebugScript : MonoBehaviour
{
    private float rayDistance, forwardRayDistance;
    private Vector3 rayOffset;
    private Vector3 leftDir, rightDir;
    private RaycastHit front, left, right;
    
    public LineRenderer frontLine;
    public LineRenderer leftLine;
    public LineRenderer rightLine;

    public Material red, green;

    public bool showDebug;

    private void Awake()
    {
        LineRenderers lr = GameObject.FindGameObjectWithTag("LRS").GetComponent<LineRenderers>();

        frontLine = lr.front;
        leftLine = lr.left;
        rightLine = lr.right;

        frontLine.useWorldSpace = true;
        leftLine.useWorldSpace = true;
        rightLine.useWorldSpace = true;

        showDebug = true;
    }
    
    private void Update()
    {
        CarAI ca = GetComponent<CarAI>();
        rayDistance = ca.rayDistance;
        forwardRayDistance = ca.forwardRayDistance;
        rayOffset = ca.rayOffset;
        leftDir = ca.leftDir;
        rightDir = ca.rightDir;
        front = ca.frontHit;
        left = ca.leftHit;
        right = ca.rightHit;

        if (showDebug)
        {
            Vector3 origin = transform.position + rayOffset;
        
            frontLine.enabled = true;
            frontLine.transform.GetChild(0).gameObject.SetActive(true);
            leftLine.enabled = true;
            leftLine.transform.GetChild(0).gameObject.SetActive(true);
            rightLine.enabled = true;
            rightLine.transform.GetChild(0).gameObject.SetActive(true);
            
            DrawLine(frontLine, origin, transform.forward, forwardRayDistance, front);
            DrawLine(leftLine, origin, leftDir, rayDistance, left);
            DrawLine(rightLine, origin, rightDir, rayDistance, right);
        }
        else
        {
            frontLine.enabled = false;
            frontLine.transform.GetChild(0).gameObject.SetActive(false);
            leftLine.enabled = false;
            leftLine.transform.GetChild(0).gameObject.SetActive(false);
            rightLine.enabled = false;
            rightLine.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    
    void DrawLine(
        LineRenderer lr,
        Vector3 origin,
        Vector3 direction,
        float distance,
        RaycastHit hit
    )
    {
        lr.SetPosition(0, origin);
        Transform sp = lr.transform.GetChild(0);

        if (hit.collider != null)
        {
            lr.SetPosition(1, hit.point);
            sp.gameObject.SetActive(true);
            sp.position = hit.point;
            lr.material = red;
        }
        else
        {
            lr.SetPosition(1, origin + direction * distance);
            sp.gameObject.SetActive(false);
            lr.material = green;
        }
    }
}
