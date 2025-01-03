using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Collections;


public class LobbyCreation : MonoBehaviour
{
    private bool isRoomPrivate = false;

    public Button privateButton;
    public Button publicButton;

    public Color selectedColor = Color.green;   // Highlight color
    public Color normalColor = Color.white;


    [SerializeField] GameObject CreateRoomPanel;
    [SerializeField] GameObject JoinRoomPanel;
    [SerializeField] GameObject MenuUI;
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button quickJoinBtn;
    [SerializeField] private TMP_InputField roomname;
    [SerializeField] private Button joincodeBtn;
    [SerializeField] private TMP_InputField joincodeInput;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_InputField playerNameInput;

    [SerializeField] private Button CreateRoomBtn;
    [SerializeField] private Button JoinRoomBtn;

    public string playerName;
    public int Max_Players = 4;
    public Lobby joinedLobby;
    private float heartbeattimer;

    private void Start()
    {
        playerName = "Tyrant" + Random.Range(0, 10);
        //Debug.Log(playerName);

        privateButton.onClick.AddListener(OnPrivateButtonClicked);
        publicButton.onClick.AddListener(OnPublicButtonClicked);
    }

    private void OnPrivateButtonClicked()
    {
        Debug.Log("Room set to private");
        isRoomPrivate = true;
        ResetButtonColors();


        privateButton.image.color = selectedColor;
    }

    private void OnPublicButtonClicked()
    {
        Debug.Log("Room set to public");
        isRoomPrivate = false;
        
        ResetButtonColors();

        publicButton.image.color = selectedColor;
    }

    private void ResetButtonColors()
    {
        privateButton.image.color = normalColor;
        publicButton.image.color = normalColor;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneLoad(scene);
    }

    // Define the actions you want to perform on scene load
    private void HandleSceneLoad(Scene scene)
    {
        if (scene.name == "Menu")
        {
            MenuUI.SetActive(true);
        }
       /* else if (scene.name == "GameScene")
        {
            
            Debug.Log("Game Scene Loaded");
        }*/
        
    }


private void Awake()
    {
        
        InitializeUnityAuthentication();

        createLobbyBtn.onClick.AddListener(() =>
        {
            CreateRoomPanel.gameObject.SetActive(true);
        });

        CreateRoomBtn.onClick.AddListener(() =>
        {
            CreateLobby(roomname.text, playerName, isRoomPrivate);

            hideUI();
        });

        quickJoinBtn.onClick.AddListener(() =>
        {

            QuickJoin(playerNameInput.text);
            hideUI();
        });

        joincodeBtn.onClick.AddListener(() =>
        {
            JoinRoomPanel.gameObject.SetActive(true);
            
        });

        JoinRoomBtn.onClick.AddListener(() =>
        {
            joinwithCode(joincodeInput.text, playerNameInput.text);
            hideUI();
        });

    }

    private void Update()
    {
        HandleHeartBeat();
    }

    private void HandleHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartbeattimer -= Time.deltaTime;
            if (heartbeattimer < 0)
            {
                float heartbeattimerMax = 15f;
                heartbeattimer = heartbeattimerMax;
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);

            }
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async void InitializeUnityAuthentication()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                InitializationOptions initializationoptions = new InitializationOptions();
                initializationoptions.SetProfile(Random.Range(0, 10000).ToString());
                await UnityServices.InitializeAsync();

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (AuthenticationException e)
        {
            Debug.LogException(e);
        }


    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(Max_Players - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            
            Debug.Log(e);
            return default;
            
        }

        
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e) { 
            Debug.Log(e);
            return default;
        }
        
    }

    private async Task<JoinAllocation> JoinRelay(string joincode)
    {
        try
        {
            JoinAllocation joinallocation = await RelayService.Instance.JoinAllocationAsync(joincode);
            return joinallocation;
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
            return default;
        }
        
    }

    public async void CreateLobby(string lobbyname,string playerName, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync("a", Max_Players, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = GetPlayer()
            });

            //multiplayer code start
            Allocation allocation = await AllocateRelay();

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member,relayJoinCode)}

                }
            });
            //multiplayer code end
            showLog();
            
            NetworkManager.Singleton.StartHost();
            
           

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.ToString());
            
        }
    }

    public async void QuickJoin(string playerName)
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);

            //multiplayer code start
            string relayJoinCode =  joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinallocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinallocation, "dtls"));
            //multiplayer code end
            showLog();
           
            NetworkManager.Singleton.StartClient();

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.ToString());
        }
    }

    public async void joinwithCode(string code,string playerName)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions { 
                Player = GetPlayer()
            };
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);

            //multiplayer code start
            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinallocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinallocation, "dtls"));
            //multiplayer code end
            NetworkManager.Singleton.StartClient();
            
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    private void hideUI()
    {
        //gameObject.SetActive(false);
        MenuUI.SetActive(false);
        CreateRoomPanel.SetActive(false);
        JoinRoomPanel.SetActive(false);
        //changer.ChangeScene("Lobby");
        SceneManager.LoadScene("Lobby");
        //NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void showLog()
    {
        //Debug.Log("Lobby Created:" + joinedLobby.Name + "\n Lobby Code:" + joinedLobby.LobbyCode.ToString());
        codeText.text = joinedLobby.LobbyCode.ToString();
    }


    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject> {
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName) }
                    }
        };
    }
}