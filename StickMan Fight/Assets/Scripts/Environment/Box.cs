using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Box : NetworkBehaviour
{
    public int boxHealth = 100;
    [SerializeField] private GameObject BoxPS;

    public void PlayPS()
    {
        PlayParticleSystemServerRpc();
    }

    
    [ServerRpc]
    private void PlayParticleSystemServerRpc()
    {
        
        PlayParticleSystemClientRpc();
    }

    
    [ClientRpc]
    private void PlayParticleSystemClientRpc()
    {
        
        if (BoxPS != null)
        {
            
            Vector3 spawnPosition = transform.position;

            
            Quaternion rotation = Quaternion.FromToRotation(Vector2.right, transform.right);

            
            Instantiate(BoxPS, spawnPosition, rotation);
        }
    }
}


