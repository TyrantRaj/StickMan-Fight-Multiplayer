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


public class LobbyCreation : MonoBehaviour
{
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button quickJoinBtn;
    [SerializeField] private TMP_InputField roomname;
    //[SerializeField] private TextMeshProUGUI lobbynaemText;

    public int Max_Players = 4;
    private Lobby joinedLobby;

    private void Awake()
    {
        InitializeUnityAuthentication();

        createLobbyBtn.onClick.AddListener(() =>
        {
            CreateLobby(roomname.text, false);
            
            //lobbynaemText.text = joinedLobby.LobbyCode;
            hideUI();
        });

        quickJoinBtn.onClick.AddListener(() =>
        {
            
            QuickJoin();
            hideUI();
        });

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
        catch (AuthenticationException e) { 
            Debug.LogException(e);
        }
        
        
    } 

    public async void CreateLobby(string lobbyname, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyname, Max_Players, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
            });
            showLog();
            NetworkManager.Singleton.StartHost();

        }
        catch (LobbyServiceException e) {
            Debug.Log(e.ToString());
        }
    }

    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            showLog();
            NetworkManager.Singleton.StartClient();

        }
        catch (LobbyServiceException e) {
            Debug.Log(e.ToString());
        }
    }

    private void hideUI()
    {
        gameObject.SetActive(false);
    }

    private void showLog()
    {
        Debug.Log("Lobby Created:" + joinedLobby.Name + "\n Lobby Code:" + joinedLobby.LobbyCode.ToString());
    }
}
