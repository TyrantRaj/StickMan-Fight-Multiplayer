using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneChanger : NetworkBehaviour
{
    private float remainingTime;
    private bool isCountingDown = false; // Flag to avoid starting the countdown multiple times
    [SerializeField] private TMP_Text SceneChangeText;
    [SerializeField] PlayerCountTracker playercounttracker;
    [SerializeField] Transform[] spawnPoints; // Array of spawn points
    public int playerCount = 0;
    public float SceneChangeDelay;
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
            startGameBtn.gameObject.SetActive(true);
            startGameBtn.onClick.AddListener(() =>
            {
                StartGame();
            });
        }
    }

    private void StartGame()
    {
        playerCount = playercounttracker.GetPlayerCount();
        Debug.Log("Player count: " + playerCount);

        if (playerCount > 0)
        {
            gameStarted = true;
            AssignPlayerInfo();
            StartSceneChangerCountDown();
            StartCoroutine(ChangeSceneWithDelay("Level1", SceneChangeDelay)); // Add 3 seconds delay before starting Level1
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
        //SceneChangeText.enabled = false;
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
            StartSceneChangerCountDown();
            AssignPlayerInfo();
            //ResetPlayerHealthOnServer();
            StartCoroutine(ChangeSceneWithDelay("Level2", SceneChangeDelay));
            
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


    

    private void StartSceneChangerCountDown()
    {
        if (!isCountingDown)
        {
            //SceneChangeText.enabled = true;
            remainingTime = SceneChangeDelay;  // Initialize remaining time
            StartCoroutine(UpdateCountdown());
            isCountingDown = true; // Set flag to prevent multiple countdowns
        }
    }


    private IEnumerator UpdateCountdown()
    {
        UpdateSceneChangerTextClientRpc();
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            // Update the countdown for the host or server
            SceneChangeText.text = Mathf.Ceil(remainingTime).ToString();  // Show whole seconds

            // Optionally, sync this countdown to clients if needed
            UpdateCountdownClientRpc(Mathf.Ceil(remainingTime));

            // Wait for the next frame
            yield return null;
        }

        // When countdown reaches zero
        SceneChangeText.text = "Starting...";
        UpdateSceneChangerTextClientRpc();
        isCountingDown = false; // Reset the countdown flag
    }

    // Sync the countdown with clients
    [ClientRpc]
    private void UpdateCountdownClientRpc(float time)
    {
        //SceneChangeText.enabled = true;
        //UpdateSceneChangerTextClientRpc();
        SceneChangeText.text = time.ToString();
    }

    [ClientRpc]
    private void UpdateSceneChangerTextClientRpc()
    {
        if (SceneChangeText.enabled)
        {
            SceneChangeText.enabled = false;
        }
        else
        {
            SceneChangeText.enabled = true;
        }
    }
        
}
