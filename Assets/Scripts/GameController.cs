using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public Vector3 carSpawnPosition;
    public Quaternion carSpawnRotation;

    public Transform car;

    public GameObject carPrefab;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync(0);
        }

        if (GameObject.FindGameObjectWithTag("Car"))
        {
            car = GameObject.FindGameObjectWithTag("Car").transform;
        }
        
        if (Input.GetKeyDown(KeyCode.S) && car != null)
        {
            Destroy(car.gameObject);
            Instantiate(carPrefab, carSpawnPosition, carSpawnRotation);
        }
    }
}
