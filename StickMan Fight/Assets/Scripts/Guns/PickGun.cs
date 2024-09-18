using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PickGun : NetworkBehaviour
{
    [SerializeField] GameObject[] Guns;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsOwner)
        {
            if (collision.gameObject.CompareTag("Gun"))
            {
                Debug.Log("Gun found");
                Guns[0].gameObject.SetActive(true);
            }
        }
        
    }
}
