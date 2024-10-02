using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    bool isBulletHit = false;
    public ulong bulletID = 5;
    [SerializeField] private float despawnTime = 5f;
    [SerializeField] private GameObject BulletExplodeParticlePrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBulletHit) return; // If the bullet has already hit something, exit

        // Handle bullet hitting body parts
        if (collision.gameObject.CompareTag("bodypart") ||
            collision.gameObject.CompareTag("LeftArm") ||
            collision.gameObject.CompareTag("RightArm"))
        {
            var playerHealth = collision.gameObject.GetComponentInParent<Health>();
            if (playerHealth != null && playerHealth.OwnerClientId != bulletID)
            {
                collision.gameObject.GetComponent<TakeDamage>().TakeDamageAction(this.gameObject.transform.position);
                isBulletHit = true;
            }
        }
        // Handle bullet hitting a box
        else if (collision.gameObject.CompareTag("Box"))
        {
            var networkObject = collision.gameObject.GetComponent<NetworkObject>();
            isBulletHit = true;
            if (networkObject != null && IsServer)
            {
                int health = networkObject.gameObject.GetComponent<Box>().boxHealth;

                if (health < 0)
                {
                    DestroyBoxServerRpc(networkObject.NetworkObjectId);
                }
                else
                {
                    networkObject.gameObject.GetComponent<Box>().boxHealth -= 10;
                    networkObject.gameObject.GetComponent<Box>().PlayPS();
                }
            }
        }
        // Handle bullet hitting the ground or any other object
        else if (collision.gameObject.CompareTag("Ground"))
        {
            
            if (IsServer)
            {
                PlayParticleSystemServerRpc(); // Pass the hit direction to the particle system
                NetworkObject.Despawn();
                // Delay despawning until after the particle system has finished playing
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyBoxServerRpc(ulong networkObjectId)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject);

        if (networkObject != null)
        {
            Destroy(networkObject.gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(DespawnAfterTime(despawnTime));
    }

    private IEnumerator DespawnAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        if (IsServer)
        {
            StartCoroutine(DelayedDespawn());
        }
    }

    private IEnumerator DelayedDespawn(float delay = 0f)
    {
        yield return new WaitForSeconds(delay); // Wait before despawning (for particle system to finish)
        if (NetworkObject.IsSpawned) // Ensure the NetworkObject is still spawned before despawning
        {
            NetworkObject.Despawn();
        }
    }

    [ServerRpc]
    private void PlayParticleSystemServerRpc()
    {
        if (NetworkObject.IsSpawned) // Ensure the NetworkObject is spawned before calling the RPC
        {
            PlayParticleSystemClientRpc();
        }
    }

    [ClientRpc]
    private void PlayParticleSystemClientRpc()
    {
        if (BulletExplodeParticlePrefab != null)
        {

            Vector3 spawnPosition = transform.position;


            Quaternion rotation = Quaternion.FromToRotation(Vector2.right, transform.right);


            Instantiate(BulletExplodeParticlePrefab, spawnPosition, rotation);
        }
    }
}
