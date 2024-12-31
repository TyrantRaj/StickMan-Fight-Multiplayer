using UnityEngine;
using Unity.Netcode;

public class TakeDamage : NetworkBehaviour
{
    [SerializeField] Health health;

    
    [SerializeField] private GameObject bloodParticlePrefab;

    public void TakeDamageAction(Vector3 bulletPosition)
    {
        if (IsOwner)
        {
            
            health.TakeDamageServerRpc();

            
            PlayParticleSystemServerRpc();
        }
    }

    public void InstanttKill()
    {
        if (IsOwner)
        {
            health.InstantKillServerRpc();
        }
    }

    public void KilledBySpike()
    {
        if (IsOwner)
        {
            health.KillbySpikeServerRpc();
        }
    }

    [ServerRpc]
    private void PlayParticleSystemServerRpc()
    {
        
        PlayParticleSystemClientRpc();
    }

    
    [ClientRpc]
    private void PlayParticleSystemClientRpc()
    {
        
        if (bloodParticlePrefab != null)
        {
            
            Vector3 spawnPosition = transform.position;

            
            Quaternion rotation = Quaternion.FromToRotation(Vector2.right, transform.right);

            
            Instantiate(bloodParticlePrefab, spawnPosition, rotation);
        }
    }

}
