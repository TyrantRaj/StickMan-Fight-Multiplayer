using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PickGun : NetworkBehaviour
{
    [SerializeField] GameObject[] Guns;          // List of gun game objects
    [SerializeField] PlayerShooting shooting;    // Reference to player shooting script
    private int GunNumber;                       // The index of the currently picked gun

    // Use a NetworkVariable<int> to track the currently equipped gun across all clients
    private NetworkVariable<int> equippedGunNumber = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Start()
    {
        // Disable all guns at the start
        foreach (GameObject gun in Guns)
        {
            gun.SetActive(false);
        }

        // Subscribe to the NetworkVariable's value change event to sync the equipped gun
        equippedGunNumber.OnValueChanged += OnGunEquippedChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe when the object is destroyed
        equippedGunNumber.OnValueChanged -= OnGunEquippedChanged;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsOwner && collision.gameObject.CompareTag("Gun"))
        {
            //Debug.Log("Gun found");

            // Get the gun number from the GunIndex component of the collided object
            GunNumber = collision.gameObject.GetComponent<GunIndex>().GunNumber;

            // Activate the gun locally for the player
            EquipGun(GunNumber);

            // Set the NetworkVariable to sync the gun across all clients
            equippedGunNumber.Value = GunNumber;

            // Pass the NetworkObject reference to the ServerRpc for despawning the gun in the world
            var networkObject = collision.gameObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                despawngunServerRpc(networkObject);
            }
        }
    }

    // Equip the gun locally and update the shooting script
    private void EquipGun(int gunNumber)
    {
        // Deactivate all guns first
        foreach (GameObject gun in Guns)
        {
            gun.SetActive(false);
        }

        // Activate the specific gun by its index
        Guns[gunNumber].SetActive(true);
        shooting.currentGun = gunNumber;
        shooting.hasGun = true;
    }

    // ServerRpc to despawn the gun object on the server
    [ServerRpc]
    private void despawngunServerRpc(NetworkObjectReference gunReference)
    {
        if (gunReference.TryGet(out NetworkObject gunNetworkObject))
        {
            gunNetworkObject.Despawn(); // Despawn the gun object on the server
        }
    }

    // This function will be called when the NetworkVariable value changes to sync the equipped gun across clients
    private void OnGunEquippedChanged(int oldGunNumber, int newGunNumber)
    {
        // Equip the gun on all clients based on the new value
        if (newGunNumber >= 0 && newGunNumber < Guns.Length)
        {
            EquipGun(newGunNumber);
        }
    }
}
