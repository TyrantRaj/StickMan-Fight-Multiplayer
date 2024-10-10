using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneChanger : NetworkBehaviour
{
    public int maxRounds;
    private int currentRound = 0;
    public string[] scenesNames;
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
            spawnobjects.NewSceneLoaded();
        }
    }

    private void StartGame()
    {
        playerCount = playercounttracker.GetPlayerCount();

        if (playerCount > 0)
        {
            lobbycode.SetActive(false);
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
    }

    private IEnumerator ChangeSceneWithDelay(string sceneToLoad, float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        ChangeScene(sceneToLoad);
    }

    public void ChangeScene(string sceneToLoad)
    {
        if (IsHost)
        {
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

        if (currentRound == maxRounds)
        {
            scoretracker.GameOverSceneClientRpc();
        }
        else if (currentAlivePlayer <= 1 && gameStarted)
        {
            scoretracker.CheckAlivePlayers();
            scoretracker.CheckWinner();
            StartSceneChangerCountDown();
            AssignPlayerInfo();
            StartCoroutine(ChangeSceneWithDelay(GetRandomScene(), SceneChangeDelay));
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

            if (!isCountingDown)
            {
                StartSceneChangerCountDown(); // Start countdown after new scene
            }
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
        SetAllPlayerMovementClientRpc(false);  // Disable player movement during countdown

        UpdateSceneChangerTextClientRpc();
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            // Update countdown on the host/server
            SceneChangeText.text = Mathf.Ceil(remainingTime).ToString();

            // Sync countdown with clients
            UpdateCountdownClientRpc(Mathf.Ceil(remainingTime));

            yield return null;  // Wait for the next frame
        }

        // When countdown reaches zero
        SceneChangeText.text = "Starting...";
        UpdateSceneChangerTextClientRpc();

        SetAllPlayerMovementClientRpc(true);  // Re-enable player movement

        isCountingDown = false; // Reset the countdown flag
    }

    [ClientRpc]
    private void SetAllPlayerMovementClientRpc(bool enable)
    {
        // Find all Movement scripts in the scene and toggle their movement
        Movement[] players = FindObjectsOfType<Movement>();

        foreach (Movement player in players)
        {
            if (player.IsOwner)
            {
                player.SetMovement(enable);
                player.freezePlayer(!enable);
            }
        }
    }

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

    private string GetRandomScene()
    {
        currentRound++;
        return scenesNames[Random.Range(0, scenesNames.Length)];
    }

    public void Mainmenu()
    {
        scoretracker.gameoverUI.SetActive(false);

        if (IsHost)
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }

    [ClientRpc]
    private void NotifyMainMenuClientRpc()
    {
        ChangeScene("Menu");
    }

    public void Replay()
    {
        scoretracker.gameoverUI.SetActive(false);  // Hide the game over UI for all players

        if (IsHost)
        {
            ChangeScene("Lobby");
        }
        else
        {
            NotifyReplayClientRpc();  // Notify non-host clients about the replay
        }
    }

    [ClientRpc]
    private void NotifyReplayClientRpc()
    {
        ChangeScene("Lobby");
    }
}
