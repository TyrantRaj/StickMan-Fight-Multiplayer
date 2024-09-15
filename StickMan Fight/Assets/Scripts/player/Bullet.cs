using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class Bullet : NetworkBehaviour
{
    bool isBulletHit = false;
    [SerializeField] private float despawnTime = 5f; // Time before despawning the object (in seconds)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if((collision.gameObject.CompareTag("bodypart") || collision.gameObject.CompareTag("LeftArm") || collision.gameObject.CompareTag("RightArm")) && isBulletHit == false)
        {
            collision.gameObject.GetComponent<TakeDamage>().TakeDamageAction();
            isBulletHit = true;
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
