using UnityEngine;

public class RoadScript : MonoBehaviour
{
    public string rtag;
    
    public Transform[] possibleTransformsStraight;
    public Transform[] possibleTransformsCurve;
    public Transform[] possibleTransformsEnd;
    public bool isOverlapping;

    public int stage = 0;

    public MeshRenderer roadMesh, barrierMesh;
    public Material barrierRed, barrierGreen, normalRed, normalGreen, normalRoad, normalBarrier;

    private void Update()
    {
        if (stage == 0)
        {
            // Red
            roadMesh.material = normalRed;
            barrierMesh.material = barrierRed;
        }
        else if (stage == 1)
        {
            // Green
            roadMesh.material  = normalGreen;
            barrierMesh.material = barrierGreen;
        }
        else
        {
            // Normal
            roadMesh.material = normalRoad;
            barrierMesh.material = normalBarrier;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Road"))
        {
            isOverlapping = true;
        }
        else
        {
            isOverlapping = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Road"))
        {
            isOverlapping = false;
        }
    }
}
