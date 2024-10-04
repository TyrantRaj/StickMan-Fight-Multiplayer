using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SpawnObjects : NetworkBehaviour
{
    [SerializeField] GameObject[] Guns;
    int currentGunsInScene = 0;
    string Current_scene_name;
    float Spawn_count = 5;
    Transform[] gun_spwan_pts;

    public void NewSceneLoaded()
    {
        Current_scene_name = SceneManager.GetActiveScene().name;
        GunSpwanPts();
    }

    private void GunSpwanPts()
    {
        GameObject[] gunSpawnObjects = GameObject.FindGameObjectsWithTag("GunSpwanPT");
        
        gun_spwan_pts = new Transform[gunSpawnObjects.Length];

        for (int i = 0; i < gunSpawnObjects.Length; i++)
        {
            gun_spwan_pts[i] = gunSpawnObjects[i].transform;
        }
    }

    private void Update()
    {
        if (!IsServer || currentGunsInScene >= 5) return;

        Spawn_count -= Time.deltaTime;

        if (Spawn_count < 0)
        {
            SpawnGunServerRpc();
            Spawn_count = 5;
        }
    }

    [ServerRpc]
    private void SpawnGunServerRpc()
    {
        if (gun_spwan_pts.Length > 0 && Guns.Length > 0)
        {
            int randomIndex = Random.Range(0, gun_spwan_pts.Length);
            int randomGunIndex = Random.Range(0, Guns.Length);

            GameObject gunInstance = Instantiate(Guns[randomGunIndex], gun_spwan_pts[randomIndex].position, Quaternion.identity);

            NetworkObject networkObject = gunInstance.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(); // Spawn it across all clients
                currentGunsInScene += 1;
            }
            else
            {
                Debug.LogError("The gun prefab does not have a NetworkObject component.");
            }
        }
    }

    [ServerRpc]
    private void DestroyAllGunServerRpc()
    {
        // Find all guns in the scene
        GameObject[] gunsInScene = GameObject.FindGameObjectsWithTag("Gun");
        currentGunsInScene = 0;
        // Loop through all found guns and despawn them
        foreach (GameObject gun in gunsInScene)
        {
            NetworkObject networkObject = gun.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                if (networkObject.IsSpawned)
                {
                    networkObject.Despawn(); // Despawn the object across the network
                }      
            }
            else
            {
                Debug.LogError("The gun does not have a NetworkObject component.");
            }
        }
    }

    public void DestroyAllGuns()
    {
        // This method can be called from the client or server, but only the server should handle the despawn
        if (IsServer)
        {
            DestroyAllGunServerRpc(); // Server-side gun destruction
            Spawn_count = 5;
        }
        else
        {
            Debug.LogWarning("Only the server can despawn objects.");
        }
    }
}
