using UnityEngine;
using Unity.Netcode;

public class TakeDamage : NetworkBehaviour
{
    [SerializeField] Health health;
    [SerializeField] int body_part_index;

    public void TakeDamageAction()
    {
        if (IsOwner)
        {
            // Send an RPC to the server to handle the damage
            health.TakeDamageServerRpc(body_part_index);
        }
    }
}
