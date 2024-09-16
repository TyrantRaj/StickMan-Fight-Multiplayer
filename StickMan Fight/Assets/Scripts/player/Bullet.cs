using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    bool isBulletHit = false;
    public ulong bulletID = 5; // This is the ID of the player who fired the bullet
    [SerializeField] private float despawnTime = 5f; // Time before despawning the object (in seconds)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBulletHit) return; // If the bullet has already hit something, exit

        // Check if the collision is with a body part
        if (collision.gameObject.CompareTag("bodypart") ||
            collision.gameObject.CompareTag("LeftArm") ||
            collision.gameObject.CompareTag("RightArm"))
        {
            // Check if the player the bullet hit is not the one who fired it
            var playerHealth = collision.gameObject.GetComponentInParent<Health>();
            if (playerHealth != null && playerHealth.OwnerClientId != bulletID)
            {
                Debug.Log(playerHealth.OwnerClientId.ToString());
                // Apply damage only if it's a different player
                collision.gameObject.GetComponent<TakeDamage>().TakeDamageAction();
                isBulletHit = true; // Mark the bullet as hit
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
