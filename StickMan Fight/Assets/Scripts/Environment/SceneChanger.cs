using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChanger : NetworkBehaviour
{
    [SerializeField] PlayerCountTracker playercounttracker;
    [SerializeField] Transform[] spawnPoints; // Array of spawn points
    public int playerCount = 0;
    public int currentAlivePlayer = 0;
    private bool gameStarted = false;

    [SerializeField] private Button startGameBtn;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded; // Register scene load callback
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unregister scene load callback
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            startGameBtn.onClick.AddListener(() =>
            {
                StartGame();
            });
        }
        else
        {
            startGameBtn.gameObject.SetActive(false);
        }
    }

    private void StartGame()
    {
        playerCount = playercounttracker.GetPlayerCount();
        Debug.Log("Player count: " + playerCount);

        if (playerCount > 1)
        {
            gameStarted = true;
            AssignPlayerInfo();
            StartCoroutine(ChangeSceneWithDelay("Level1", 3f)); // Add 3 seconds delay before starting Level1
        }
        else
        {
            Debug.LogWarning("Not enough players to start the game.");
        }
    }

    private void AssignPlayerInfo()
    {
        currentAlivePlayer = playerCount;
        Debug.Log($"Player Count: {playerCount}, Alive Players: {currentAlivePlayer}");
    }

    // Change scene after a delay
    private IEnumerator ChangeSceneWithDelay(string sceneToLoad, float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        ChangeScene(sceneToLoad);
    }

    public void ChangeScene(string sceneToLoad)
    {
        if (IsHost)
        {
            Debug.Log($"Changing scene to {sceneToLoad}");
            NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("Only the server can change the scene.");
        }
    }

    [ServerRpc]
    public void UpdateAliveCountServerRpc()
    {
        if (!IsServer && gameStarted) return;

        currentAlivePlayer--; // Reduce the count of alive players
        Debug.Log($"Alive players reduced to: {currentAlivePlayer}");

        if (currentAlivePlayer == 1 && gameStarted)
        {
            StartCoroutine(ChangeSceneWithDelay("Level2", 3f)); // Add 3 seconds delay before switching to Level2
            AssignPlayerInfo();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsServer)
        {
            ResetPlayerHealthOnServer(); 
        }
    }

    private void ResetPlayerHealthOnServer()
    {
        foreach (var health in FindObjectsOfType<Health>())
        {
            health.ResetHealthServerRpc(); // Use server to reset health for all players
        }
    }

 
}
