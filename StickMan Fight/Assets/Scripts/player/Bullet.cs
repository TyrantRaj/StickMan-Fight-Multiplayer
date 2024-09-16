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

        // Check if the collision is with a body part
        if (collision.gameObject.CompareTag("bodypart") ||
            collision.gameObject.CompareTag("LeftArm") ||
            collision.gameObject.CompareTag("RightArm"))
        {
            var playerHealth = collision.gameObject.GetComponentInParent<Health>();
            if (playerHealth != null && playerHealth.OwnerClientId != bulletID)
            {
                
                collision.gameObject.GetComponent<TakeDamage>().TakeDamageAction();
                isBulletHit = true; 
            }
        }
    }

    private void Start()
    {
        // Start the coroutine to despawn after a delay
        StartCoroutine(DespawnAfterTime(despawnTime));
    }

    private IEnumerator DespawnAfterTime(float time)
    {
        // Wait for the specified amount of time
        yield return new WaitForSeconds(time);

        // Check if the object is spawned on the server before despawning
        if (IsServer)
        {
            // Despawn the object
            NetworkObject.Despawn();
        }
    }
}
