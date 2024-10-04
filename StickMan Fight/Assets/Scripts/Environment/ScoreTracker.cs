using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ScoreTracker : NetworkBehaviour
{
    private int trackplayer = 0;
    private int[] score;
    [SerializeField] private TMP_Text WinnerTxt;

    private List<GameObject> playerGameObjects = new List<GameObject>();
    private void Start()
    {
        score = new int[4];
    }
    // Call this when you want to initialize the score and get players
    public void InitializePlayerScores()
    {
        GetAllPlayerGameObjects();

        // Initialize score array based on the number of players
        score = new int[playerGameObjects.Count];
    }

    public void GetAllPlayerGameObjects()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            // Clear the list before adding players
            playerGameObjects.Clear();

            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                NetworkObject playerNetworkObject = client.Value.PlayerObject;

                if (playerNetworkObject != null)
                {
                    playerGameObjects.Add(playerNetworkObject.gameObject);
                }
            }
        }
    }

    public void CheckAlivePlayers()
    {
        // Ensure that the score array and player list have been initialized
        if (score == null || playerGameObjects == null)
        {
            Debug.LogError("Player scores or game objects not initialized.");
            return;
        }

        trackplayer = 0;

        foreach (GameObject player in playerGameObjects)
        {
            if (trackplayer < score.Length) // Check to prevent out-of-bounds error
            {
                if (player.GetComponent<Health>().health.Value > 0)
                {
                    score[trackplayer] += 1;
                    Debug.Log("alive player is " + trackplayer.ToString());
                }
            }
            trackplayer++;
        }

        // Reset trackplayer to 0 after processing
        trackplayer = 0;
    }

    public void CheckWinner()
    {
        if (score == null)
        {
            //Debug.LogError("Score array is not initialized.");
            return;
        }

        int winnerNo = -1;
        int currentHighestScore = -1;  // Start with -1 to ensure we capture scores starting at 0

        for (int i = 0; i < score.Length; i++)
        {
            if (score[i] >= currentHighestScore)  // Use >= to handle ties at 0 or higher
            {
                currentHighestScore = score[i];
                winnerNo = i;
            }
        }
        WinnerTxt.text = winnerNo.ToString();
        //Debug.Log("Winner is player: " + winnerNo.ToString());
        
    }

}
