using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SceneChanger : NetworkBehaviour
{
    [SerializeField] PlayerCountTracker playercounttracker;

    public int playerCount = 0;
    public int currentAlivePlayer = 0;
    private bool gameStarted = false;

    [SerializeField] private Button startGameBtn; // Button to start the game

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        startGameBtn.onClick.AddListener(() =>
        {
            if (IsServer)
            {
                StartGame();
            }
            else
            {
                Debug.Log("You are not the server");
            }
            
        });
    }   

    private void Update()
    {
        // Only execute this if the game has started and we are on the server
        if (gameStarted && IsServer)
        {
            //Debug.Log($"Player Count: {playerCount}, Alive Players: {currentAlivePlayer}");

            // If only one player is alive, change to the next scene
            /*if (currentAlivePlayer == 1)
            {
                ChangeScene("Level2");
                Debug.Log("Scene changed to Level2");
                currentAlivePlayer = playerCount; // Reset alive players for the next level
            }*/
        }
    }

    // Method to start the game
    private void StartGame()
    {
        playerCount = playercounttracker.GetPlayerCount();
        Debug.Log("Playercount:" + playerCount);

        if (playerCount > 0) // Ensure there is more than one player
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
        
        currentAlivePlayer = playerCount; // Set alive players to total player count
        Debug.Log($"Player Count: {playerCount}, Alive Players: {currentAlivePlayer}");
    }

    // Method to change the scene, only the server can do this
    public void ChangeScene(string sceneToLoad)
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Only the server can change the scene.");
        }
    }




    /*// Server-side: Called when a player connects
  private void OnPlayerConnected(ulong clientId)
  {
      if (IsServer)
      {
          playerCount++;
          currentAlivePlayer++;
          Debug.Log($"Player {clientId} connected. Total players: {playerCount}");
      }
  }

  // Server-side: Called when a player disconnects
  private void OnPlayerDisconnected(ulong clientId)
  {
      if (IsServer)
      {
          playerCount--;
          currentAlivePlayer--;
          Debug.Log($"Player {clientId} disconnected. Remaining players: {playerCount}");
      }
  }*/
}
