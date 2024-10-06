using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Services.Lobbies.Models;

public class SceneChanger : NetworkBehaviour
{
    [SerializeField] private ScoreTracker scoretracker;
    private SpawnObjects spawnobjects;
    private float remainingTime;
    private bool isCountingDown = false;
    [SerializeField] private TMP_Text SceneChangeText;
    [SerializeField] private PlayerCountTracker playercounttracker;
    [SerializeField] private Transform[] spawnPoints;
    GameObject lobbycode;
    public int playerCount = 0;
    public float SceneChangeDelay;
    public int currentAlivePlayer = 0;
    private bool gameStarted = false;

    [SerializeField] private Button startGameBtn;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded; // Register scene load callback
        spawnobjects = GetComponent<SpawnObjects>();
    }

    private new void OnDestroy()
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
            spawnobjects.NewSceneLoaded();
        }
    }

    private void StartGame()
    {
        playerCount = playercounttracker.GetPlayerCount();

        if (playerCount > 0)
        {
            lobbycode.gameObject.SetActive(false);
            gameStarted = true;
            scoretracker.InitializePlayerScores();
            AssignPlayerInfo();
            StartSceneChangerCountDown();
            StartCoroutine(ChangeSceneWithDelay("Level1", SceneChangeDelay)); // Add delay before starting Level1
        }
        else
        {
            Debug.LogWarning("Not enough players to start the game.");
        }
    }

    private void AssignPlayerInfo()
    {
        currentAlivePlayer = playerCount;
        //Debug.Log($"Player Count: {playerCount}, Alive Players: {currentAlivePlayer}");
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
            //Debug.Log($"Changing scene to {sceneToLoad}");
            NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("Only the server can change the scene.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateAliveCountServerRpc()
    {
        if (!IsServer || !gameStarted) return;

        currentAlivePlayer--; // Reduce the count of alive players
        //Debug.Log($"Alive players reduced to: {currentAlivePlayer}");

        if (currentAlivePlayer <= 1 && gameStarted)
        {
            scoretracker.CheckAlivePlayers();
            scoretracker.CheckWinner();
            StartSceneChangerCountDown();
            AssignPlayerInfo();
            StartCoroutine(ChangeSceneWithDelay("Level2", SceneChangeDelay));
        }
    }
    private void Start()
    {
        lobbycode = GameObject.FindGameObjectWithTag("Lobbycode");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsServer)
        {
            ResetPlayerHealthOnServer();
            spawnobjects.NewSceneLoaded();
            spawnobjects.DestroyAllGuns();
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

            // Sync this countdown to clients
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
        SceneChangeText.text = time.ToString();
    }

    [ClientRpc]
    private void UpdateSceneChangerTextClientRpc()
    {
        SceneChangeText.enabled = !SceneChangeText.enabled;
    }

    
   
}
