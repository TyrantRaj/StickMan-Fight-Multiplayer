using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro; 

public class playerInfo : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> playerNetworkName = new NetworkVariable<FixedString128Bytes>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);

    [SerializeField] private TextMeshProUGUI playerNameDisplay;

    private void Start()
    {
        if (IsOwner)
        {
            RequestSetPlayerNameServerRpc(GameObject.FindGameObjectWithTag("Lobby").GetComponent<LobbyCreation>().playerName);
        }

        
        playerNetworkName.OnValueChanged += OnPlayerNameChanged;

        
        OnPlayerNameChanged(new FixedString128Bytes(), playerNetworkName.Value);
    }

    [ServerRpc]
    private void RequestSetPlayerNameServerRpc(string playerName, ServerRpcParams rpcParams = default)
    {
        
        playerNetworkName.Value = new FixedString128Bytes(playerName);
        Debug.Log("Server set player name: " + playerNetworkName.Value.ToString());
    }

    private void OnPlayerNameChanged(FixedString128Bytes oldName, FixedString128Bytes newName)
    {
        playerNameDisplay.text = newName.ToString();
    }

    private new void OnDestroy()
    {
        
        playerNetworkName.OnValueChanged -= OnPlayerNameChanged;
    }
}
