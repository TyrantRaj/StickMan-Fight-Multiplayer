using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;
    //SceneChanger changer;
    private int currentPlayerIndex = 0;

    private void Awake()
    {
        //changer = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneChanger>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        GameObject player = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.gameObject;

        
        int spawnIndex = currentPlayerIndex % spawnPoints.Count;
        Transform spawnPoint = spawnPoints[spawnIndex];

       
        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        currentPlayerIndex++;
        //changer.playercount++;
    }
}
