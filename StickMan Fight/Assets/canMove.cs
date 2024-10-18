/*using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class canMove : MonoBehaviour
{
    public string activeSceneName = "MovingBox"; // Set this to the name of the scene where the object should be active
    [SerializeField] private BoxObstacleMovement boxScript;

    private void Start()
    {
        // Subscribe to sceneLoaded event to detect when a new scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Check if the object should be enabled at start
        CheckSceneActivation(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from the event
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneActivation(scene); // Check if the object should be enabled or disabled based on the active scene
    }

    private void CheckSceneActivation(Scene scene)
    {
        if (scene.name == activeSceneName)
        {
            boxScript.canMove = true; // Enable movement in the specified scene
            Debug.Log("Movement enabled for scene: " + activeSceneName);
        }
        else
        {
            boxScript.canMove = false; // Disable movement otherwise
            Debug.Log("Movement disabled for scene: " + scene.name);
        }
    }
}
*/