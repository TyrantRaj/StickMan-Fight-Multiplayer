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
    private NetworkVariable<int> health = new NetworkVariable<int>(100);

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
                health.Value = 0;
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

    private void dead()
    {
        if (IsOwner)
        {
            if (!isdead)
            {
                reduce_alivecountServerRpc();
                movement.enabled = false;
                ignorecollision.enabled = false;

                foreach (Balance balance in balances)
                {
                    balance.enabled = false;
                }
                isdead = true;
            }
        }
    }

    [ServerRpc]
    private void reduce_alivecountServerRpc()
    {
        changer.UpdateAliveCountServerRpc();
    }

    [ClientRpc]
    public void ResetHealthClientRpc()
    {
        health.Value = 100; // Set this to your default health value
    }
}
