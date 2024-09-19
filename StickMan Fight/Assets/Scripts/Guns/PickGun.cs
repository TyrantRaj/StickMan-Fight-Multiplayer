using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PickGun : NetworkBehaviour
{
    [SerializeField] GameObject[] Guns;
    [SerializeField] PlayerShooting shooting;

    // Use a NetworkVariable to track gun activation
    private NetworkVariable<bool> isGunActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Start()
    {
        foreach (GameObject gun in Guns)
        {
            gun.SetActive(false);
        }

        // Subscribe to the NetworkVariable's value change event
        isGunActive.OnValueChanged += OnGunActivationChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe when the object is destroyed
        isGunActive.OnValueChanged -= OnGunActivationChanged;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsOwner)
        {
            if (collision.gameObject.CompareTag("Gun"))
            {
                Debug.Log("Gun found");

                /*pistol = 0
                AK = 1*/

                Guns[collision.gameObject.GetComponent<GunIndex>().GunNumber].SetActive(true);
                shooting.hasGun = true;
                // Set the NetworkVariable to true to trigger activation across all clients
                isGunActive.Value = true;

                // Pass the NetworkObject reference to the ServerRpc for despawning
                var networkObject = collision.gameObject.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    despawngunServerRpc(networkObject);
                }
            }
        }
    }

    // ServerRpc to despawn the gun object
    [ServerRpc]
    private void despawngunServerRpc(NetworkObjectReference gunReference)
    {
        // Despawn the object only on the server
        if (gunReference.TryGet(out NetworkObject gunNetworkObject))
        {
            gunNetworkObject.Despawn(); // Despawn the object
        }
    }

    // This function will be called when the NetworkVariable changes
    private void OnGunActivationChanged(bool oldValue, bool newValue)
    {
        // When the NetworkVariable value changes to true, activate the gun on all clients
        if (newValue)
        {
            Guns[0].SetActive(true);
        }
    }
}
