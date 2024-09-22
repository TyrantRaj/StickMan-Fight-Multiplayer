using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SpawnPlayerOnScene : NetworkBehaviour
{
    public Transform[] spawnPoints; // Assign different spawn points from the inspector
    private int currentSpawnPt = 0;

    private void Start()
    {
        if (IsServer)
        {
            // Handle scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
            SpawnPlayerForScene(); // Spawn when the game starts
        }
    }

    private void OnDestroy()
    {
        if (IsServer)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to avoid issues when destroyed
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsServer)
        {
            SpawnPlayerForScene();
        }
    }

    private void SpawnPlayerForScene()
    {
        // Ensure there are spawn points available in the current scene
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned for this scene!");
            return;
        }

        // Iterate through all connected clients and set their positions
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject player = client.PlayerObject.gameObject;

            if (player != null)
            {
              

                // Assign player to a spawn point based on currentSpawnPt index
                Transform spawnPoint = spawnPoints[currentSpawnPt % spawnPoints.Length];
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;

                // Debugging the player's new position after setting it
                

                currentSpawnPt++; // Move to next spawn point for the next player
            }
            
        }
    }
}
