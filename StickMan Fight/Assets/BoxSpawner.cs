using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class BoxSpawner : MonoBehaviour
{
    [SerializeField] private GameObject boxPrefab; // Reference to the box prefab
    public string activeSceneName = "MovingBox"; // The name of the scene where the box should be spawned

    private void Start()
    {
        // Subscribe to the sceneLoaded event to spawn the box when a new scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when this object is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Box spawned scene loded");
        // Check if the loaded scene is the one where the box should be spawned
        if (scene.name == activeSceneName)
        {
            Debug.Log("Box spawned called");
            SpawnBoxObstacle();
        }
    }

    private void SpawnBoxObstacle()
    {
        Debug.Log("Box spawned");
        GameObject boxInstance = Instantiate(boxPrefab, new Vector3(-12, -0.6f, 0), Quaternion.identity); // Adjust the spawn position as needed
        boxInstance.GetComponent<NetworkObject>().Spawn(); // Ensure it is networked
    }
}
