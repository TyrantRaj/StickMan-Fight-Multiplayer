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
            SceneManager.sceneLoaded += OnSceneLoaded;
            SpawnPlayerForSceneServerRpc();
        }
    }

    private new void OnDestroy()
    {
        if (IsServer)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsServer)
        {
            SpawnPlayerForSceneServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnPlayerForSceneServerRpc()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned for this scene!");
            return;
        }

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject player = client.PlayerObject.gameObject;

            if (player != null)
            {
                Transform spawnPoint = spawnPoints[currentSpawnPt % spawnPoints.Length];
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;

                // Send the updated position and rotation to the client
                UpdatePlayerPositionClientRpc(spawnPoint.position, spawnPoint.rotation, client.ClientId);

                currentSpawnPt++;
            }
        }
    }

    // ClientRpc to update the player position on each client
    [ClientRpc]
    private void UpdatePlayerPositionClientRpc(Vector3 newPosition, Quaternion newRotation, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Update the player's position and rotation on the client
            GameObject player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject;
            player.transform.position = newPosition;
            player.transform.rotation = newRotation;
        }
    }
}
