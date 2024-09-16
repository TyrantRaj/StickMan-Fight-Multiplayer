using Unity.Netcode;
using UnityEngine;

public class PlayerCountTracker : MonoBehaviour
{
    void Start()
    {

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Player Count: " + GetPlayerCount());
        }
        else
        {
            Debug.Log("not working");
        }
    }

    // Method to get the current player count
    public int GetPlayerCount()
    {
        if (NetworkManager.Singleton != null)
        {
            return NetworkManager.Singleton.ConnectedClients.Count;
        }
        else
        {
            Debug.LogError("NetworkManager not found.");
            return 0;
        }
        
    }
}
