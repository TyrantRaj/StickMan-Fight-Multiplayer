using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    bool isBulletHit = false;
    public ulong bulletID = 5;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float despawnTime = 5f;
    [SerializeField] private GameObject BulletExplodeParticlePrefab;

    private void Start()
    {
        StartCoroutine(DespawnAfterTime(despawnTime));
    }

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

                // Play particle effect immediately for all clients
                PlayParticleSystemClientRpc(transform.position);
                //DestroyBullet();
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

                if (health <= 0)
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
        else if (collision.gameObject.CompareTag("Ground"))
        {
            
                PlayParticleSystemClientRpc(transform.position);
                rb.velocity = Vector3.zero;
                isBulletHit = true;
            
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

    private void DestroyBullet()
    {
        if (IsServer)
        {
            NetworkObject.Despawn();
        }
        else
        {
            // Call server RPC to confirm destruction on the server
            DestroyBulletServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyBulletServerRpc()
    {
        NetworkObject.Despawn();
    }

    private IEnumerator DespawnAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyBullet();
    }

    [ClientRpc]
    private void PlayParticleSystemClientRpc(Vector3 position)
    {
        if (BulletExplodeParticlePrefab != null)
        {
            // Instantiate the particle effect on all clients
            Instantiate(BulletExplodeParticlePrefab, position, Quaternion.identity);
        }
    }
}
