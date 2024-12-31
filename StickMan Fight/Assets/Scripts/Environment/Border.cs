using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Border : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            
            var playerHealth = collision.gameObject.GetComponentInParent<Health>();

            {
                collision.gameObject.GetComponent<TakeDamage>().InstanttKill();
            }
        }
    }
}
