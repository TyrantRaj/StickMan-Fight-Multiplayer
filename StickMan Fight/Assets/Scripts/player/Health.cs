using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] Movement movement;
    [SerializeField] IgnoreCollisions ignorecollision;
    [SerializeField] Balance[] balances;
    // Use NetworkVariable for health
    private NetworkVariable<int> health = new NetworkVariable<int>(100);

    [SerializeField] private TextMeshProUGUI healthtext;

    private void Start()
    {
        // Ensure only the owner of the object controls the health updates
        if (IsOwner)
        {
            health.OnValueChanged += OnHealthChanged;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (health.Value < 1)
        {
            health.Value = 0;
            healthtext.text = "0";
            //Debug.Log("Player Dead");
            //dead();
            stickmanRagdoll.OnDeath();
        }
        else
        {
            healthtext.text = health.Value.ToString();
        }
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        healthtext.text = newValue.ToString();
    }

    // This function can be called by the server to reduce health
    [ServerRpc]
    public void TakeDamageServerRpc(int body_part)
    {
        if (!IsServer) return;  // Only the server should modify the health

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

        // Ensure health is synchronized across all clients
        if (health.Value < 0)
        {
            health.Value = 0;
        }
    }

    private void dead()
    {
        if (IsOwner)
        {
            movement.enabled = false;
            ignorecollision.enabled = false;

            foreach (Balance balance in balances)
            {

                balance.enabled = false;
            }
        }
    }
}
