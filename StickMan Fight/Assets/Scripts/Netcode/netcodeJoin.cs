using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class netcodeJoin : MonoBehaviour
{
    LobbyCreation LobbyCreation;

    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    

    private void Awake()
    {
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host Started");
            hideUI();
        });

        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Host Started");
            hideUI();
        });

       
    }

    private void hideUI()
    {
        gameObject.SetActive(false);
    }
}
