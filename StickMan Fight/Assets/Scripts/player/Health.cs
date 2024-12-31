using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    private bool isInstantKill = false;
    SceneChanger changer;
    public bool isdead = false;
    [SerializeField] squareplayermovement movement;
    [SerializeField] Rigidbody2D[] rigidbodies;
    [SerializeField] Arms arm;

    // Ensure that only the server can modify this variable
    public NetworkVariable<int> health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private TextMeshProUGUI healthtext;

    private void Awake()
    {
        changer = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SceneChanger>();
        
    }

    private void Start()
    {
        health.OnValueChanged += OnHealthChanged; // Update UI for all clients
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (health.Value <= 0)
            {
                healthtext.text = "0";


                dead();
            }
            else
            {
                healthtext.text = health.Value.ToString();
            }
        }
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        healthtext.text = newValue.ToString();
    }

    [ServerRpc]
    public void TakeDamageServerRpc()
    {
        health.Value -= Random.Range(5, 25);
    }

    [ServerRpc]
    public void InstantKillServerRpc()
    {
        isInstantKill = true;
        health.Value = 0;
    }

    [ServerRpc]
    public void KillbySpikeServerRpc()
    {
        health.Value = 0;
    }

    private void dead()
    {
        if (IsOwner)
        {
            if (!isdead)
            {
                reduce_alivecountServerRpc();
                movement.enabled = false;
                
                arm.enabled = false;
            
                isdead = true;
            }
        }
    }



    [ClientRpc]
    private void BringAliveClientRpc()
    {
        StartCoroutine(bringAlivecount());
    }

    IEnumerator bringAlivecount()
    {
        yield return new  WaitForSeconds(1f);
        if (isdead)
        {
            movement.enabled = true;
            
            arm.enabled = true;

          


            isdead = false;
            isInstantKill = false;
        }
    }

    [ServerRpc]
    private void reduce_alivecountServerRpc()
    {
        changer.UpdateAliveCountServerRpc();
    }

    // Health reset is done on the server and synced to all clients
    [ServerRpc(RequireOwnership = false)]
    public void ResetHealthServerRpc()
    {
        health.Value = 100; // Reset health to default value on server
        BringAliveClientRpc();
    }
}
