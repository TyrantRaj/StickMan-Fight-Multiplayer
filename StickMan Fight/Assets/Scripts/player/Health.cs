using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    SceneChanger changer;
    private bool isdead = false;
    [SerializeField] Movement movement;
    [SerializeField] IgnoreCollisions ignorecollision;
    [SerializeField] Balance[] balances;
    [SerializeField] Arms arm;

    // Ensure that only the server can modify this variable
    private NetworkVariable<int> health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
    public void TakeDamageServerRpc(int body_part)
    {
        switch (body_part)
        {
            case 0:
                health.Value -= Random.Range(5, 25);
                break;
            case 1:
                health.Value -= Random.Range(5, 10);
                break;
            case 2:
                health.Value -= Random.Range(5, 10);
                break;
            case 3:
                health.Value -= Random.Range(5, 15);
                break;
            default:
                health.Value -= Random.Range(5, 20);
                break;
        }
    }

    [ServerRpc]
    public void InstantKillServerRpc()
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
                ignorecollision.enabled = false;
                arm.enabled = false;
                foreach (Balance balance in balances)
                {
                    balance.enabled = false;
                }
                isdead = true;
            }
        }
    }

    [ClientRpc]
    private void BringAliveClientRpc()
    {
        
        if (isdead)
        {
            movement.enabled = true;
            ignorecollision.enabled = true;
            arm.enabled = true;
            foreach (Balance balance in balances)
            {
                balance.enabled = true;
            }
            isdead = false;
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
