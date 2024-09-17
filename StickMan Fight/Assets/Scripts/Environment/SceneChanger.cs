using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChanger : NetworkBehaviour
{
    [SerializeField] PlayerCountTracker playercounttracker;
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
            ChangeScene("Level1");
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
            ChangeScene("Level2");
            AssignPlayerInfo();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsServer)
        {
            ResetPlayerHealthOnServerServerRpc(); // Call server-side method to reset health
        }
    }

    [ServerRpc]
    private void ResetPlayerHealthOnServerServerRpc()
    {
        foreach (var health in FindObjectsOfType<Health>())
        {
            health.ResetHealthClientRpc(); // Notify all clients to reset health
        }
    }
}
