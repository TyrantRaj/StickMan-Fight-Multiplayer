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
    [SerializeField] GameObject MenuUI;
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button quickJoinBtn;
    [SerializeField] private TMP_InputField roomname;
    [SerializeField] private Button joincodeBtn;
    [SerializeField] private TMP_InputField joincodeInput;
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_InputField playerNameInput;

    public string playerName;
    public int Max_Players = 4;
    public Lobby joinedLobby;
    private float heartbeattimer;

    private void Start()
    {
        playerName = "Tyrant" + Random.Range(0, 10); 
        //Debug.Log(playerName);
    }

    private void Awake()
    {
        InitializeUnityAuthentication();

        createLobbyBtn.onClick.AddListener(() =>
        {
            CreateLobby(roomname.text,playerName, false);

            hideUI();
        });

        quickJoinBtn.onClick.AddListener(() =>
        {

            QuickJoin(playerNameInput.text);
            hideUI();
        });

        joincodeBtn.onClick.AddListener(() =>
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
        

            /*Allocation allocation = await AllocateRelay();

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member,relayJoinCode)}

                }
            });*/

            showLog();
            
            NetworkManager.Singleton.StartHost();
            PrintPlayers(joinedLobby);
            //SetPlayerNameOnServerRpc(playerName, NetworkManager.Singleton.LocalClientId);

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

            /*string relayJoinCode =  joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinallocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinallocation, "dtls"));
*/
            showLog();
            PrintPlayers(joinedLobby);
            NetworkManager.Singleton.StartClient();
            //SetPlayerNameOnServerRpc(playerName, NetworkManager.Singleton.LocalClientId);

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

            /*string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinallocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinallocation, "dtls"));*/

            NetworkManager.Singleton.StartClient();
            PrintPlayers(joinedLobby);
            //SetPlayerNameOnServerRpc(playerName, NetworkManager.Singleton.LocalClientId);
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

    private void PrintPlayers(Lobby lobby)
    {
        //Debug.Log("Players in Lobby " + lobby.Name);
        foreach (Player player in lobby.Players) {
            //Debug.Log(player.Id+" " + player.Data["PlayerName"].Value);
        }
    }

    
}