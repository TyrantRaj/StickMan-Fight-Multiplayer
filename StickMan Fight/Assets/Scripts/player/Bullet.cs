using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    bool isBulletHit = false;
    public ulong bulletID = 5; 
    [SerializeField] private float despawnTime = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBulletHit) return; // If the bullet has already hit something, exit

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
            NetworkObject.Despawn();
        }
    }
}
