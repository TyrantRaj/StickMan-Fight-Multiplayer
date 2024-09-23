using UnityEngine;
using Unity.Netcode;

public class TakeDamage : NetworkBehaviour
{
    [SerializeField] Health health;
    [SerializeField] int body_part_index;

    // Add a reference to the blood particle system prefab
    [SerializeField] private GameObject bloodParticlePrefab;

    public void TakeDamageAction(Vector3 bulletPosition)
    {
        if (IsOwner)
        {
            // Send an RPC to the server to handle the damage
            health.TakeDamageServerRpc(body_part_index);

            // Call the Server RPC to play the particle system across clients, passing the bullet's position
            PlayParticleSystemServerRpc(bulletPosition);
        }
    }

    // Server RPC to trigger the particle system on all clients
    [ServerRpc]
    private void PlayParticleSystemServerRpc(Vector3 bulletPosition)
    {
        // Call Client RPC to play the particle effect for all clients, passing the bullet's position
        PlayParticleSystemClientRpc(bulletPosition);
    }

    // Client RPC to play the particle system on each client and rotate it towards the bullet hit direction
    [ClientRpc]
    private void PlayParticleSystemClientRpc(Vector3 bulletPosition)
    {
        // Instantiate and play the blood particle effect locally on each client
        if (bloodParticlePrefab != null)
        {
            // Get the position where the particle should spawn (e.g., current object position)
            Vector3 spawnPosition = transform.position;

            // Calculate the direction from the hit point (player) to the bullet's position
            Vector3 direction = spawnPosition - bulletPosition;

            // Calculate the rotation to make the particles face the bullet direction
            Quaternion rotation = Quaternion.LookRotation(Vector3.right, direction);

            // Instantiate the blood particle system at the hit point with the calculated rotation
            Instantiate(bloodParticlePrefab, spawnPosition, rotation);
        }
    }
}
