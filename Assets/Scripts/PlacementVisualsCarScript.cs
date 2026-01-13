using UnityEngine;

public class PlacementVisualsCarScript : MonoBehaviour
{
    public bool isGreen;

    public MeshRenderer[] meshes;

    public Material red, green;
    private Material currentMat;
    
    private void Update()
    {
        currentMat = isGreen ? green : red;
        
        foreach (MeshRenderer mr in meshes)
        {
            mr.material = currentMat;
        }
    }
}
