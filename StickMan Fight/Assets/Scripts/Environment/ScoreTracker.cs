using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Collections;

public class ScoreTracker : NetworkBehaviour
{
    [SerializeField] GameObject[] PlayerScoreGameObjects;
    [SerializeField] SceneChanger scenechanger;
    [SerializeField]  TextMeshProUGUI[] player_names;
    [SerializeField]  TextMeshProUGUI[] player_scores;
    [SerializeField] public GameObject gameoverUI;
    [SerializeField] Button pause_btn;
    [SerializeField] Button Replay_btn;
    [SerializeField] Button Mainmenu_btn;
    private int trackplayer = 0;
    private int[] score;
    

    private List<GameObject> playerGameObjects = new List<GameObject>();

    private void Awake()
    {
        pause_btn.onClick.AddListener(() =>
        {
            if (gameoverUI != null)
            {
                if (gameoverUI.activeSelf)
                {
                    gameoverUI.SetActive(false);
                }
                else
                {
                    RequestScoreboardUpdateServerRpc();
                    gameoverUI.SetActive(true);
                }
            }
        });

        Replay_btn.onClick.AddListener(() =>
        {
            scenechanger.Replay();
        });

        Mainmenu_btn.onClick.AddListener(() =>
        {
            scenechanger.Mainmenu();
        });
    }

    private void Start()
    {
        score = new int[4];  
    }

    public void InitializePlayerScores()
    {
        GetAllPlayerGameObjects();
        score = new int[playerGameObjects.Count];  
    }

    public void GetAllPlayerGameObjects()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
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
        if (score == null || playerGameObjects == null)
        {
            Debug.LogError("Player scores or game objects not initialized.");
            return;
        }

        trackplayer = 0;

        foreach (GameObject player in playerGameObjects)
        {
            if (trackplayer < score.Length)
            {
                if (player.GetComponent<Health>().health.Value > 0)
                {
                    score[trackplayer] += 1;
                    Debug.Log("Alive player is " + trackplayer.ToString());
                }
            }
            trackplayer++;
        }

        trackplayer = 0;

        // After updating the scores, sync them with clients
        UpdatePlayerScoresClientRpc(score);
    }

    public void CheckWinner()
    {
        if (score == null) return;

        int winnerNo = -1;
        int currentHighestScore = -1;

        for (int i = 0; i < score.Length; i++)
        {
            if (score[i] >= currentHighestScore)
            {
                currentHighestScore = score[i];
                winnerNo = i;
            }
        }

       // WinnerTxt.text = "Winner: Player " + (winnerNo + 1).ToString();  // Display winner (assuming 1-based index)
    }

    private void setScoreBoad()
    {
        if (!IsServer) return;

        GetAllPlayerGameObjects();

        FixedString128Bytes[] playerNames = new FixedString128Bytes[playerGameObjects.Count];

        for (int i = 0; i < playerGameObjects.Count; i++)
        {
            var playerInfo = playerGameObjects[i].GetComponent<playerInfo>();
            if (playerInfo != null)
            {
                playerNames[i] = playerInfo.playerNetworkName.Value;
            }
        }

        UpdatePlayerNamesClientRpc(playerNames);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestScoreboardUpdateServerRpc(ServerRpcParams rpcParams = default)
    {
        setScoreBoad();
        //CheckAlivePlayers();  // Update the scores after retrieving player data
    }

    [ClientRpc]
    private void UpdatePlayerNamesClientRpc(FixedString128Bytes[] playerNames)
    {
        for (int i = 0; i < playerNames.Length; i++)
        {
            PlayerScoreGameObjects[i].SetActive(true);
            player_names[i].text = playerNames[i].ToString();
        }
    }

    [ClientRpc]
    private void UpdatePlayerScoresClientRpc(int[] playerScores)
    {
        for (int i = 0; i < playerScores.Length; i++)
        {
            player_scores[i].text = playerScores[i].ToString();  // Update each player's score on the scoreboard
        }
    }

    [ClientRpc]
    public void GameOverSceneClientRpc()
    {
        Replay_btn.gameObject.SetActive(true);
        Mainmenu_btn.gameObject.SetActive(true);
        gameoverUI.SetActive(true);
    }

    
}
