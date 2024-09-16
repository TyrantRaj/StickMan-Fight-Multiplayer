using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 25f;

    void Update()
    {
        //shoot();
    }

    public void shoot()
    {
        // Only allow the local player to shoot
        if (!IsOwner) return;

        // Call the ServerRpc to spawn and assign the bullet
        ShootServerRpc(firePoint.position, firePoint.right);
    }

    // Request the server to spawn a bullet
    [ServerRpc]
    public void ShootServerRpc(Vector3 position, Vector3 direction)
    {
        // Instantiate the bullet on the server
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;

        // Spawn the bullet across the network
        var bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn();

        // Send the bullet's NetworkObjectId and OwnerClientId to all clients
        AssignBulletIDClientRpc(bulletNetworkObject.NetworkObjectId, OwnerClientId);
    }

    // This ClientRpc sends the bullet's NetworkObjectId and the owner's ClientId to all clients
    [ClientRpc]
    private void AssignBulletIDClientRpc(ulong bulletNetworkObjectId, ulong ownerClientId)
    {
        // Find the bullet by its NetworkObjectId
        NetworkObject bulletNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[bulletNetworkObjectId];

        // Assign the bullet ID (which is the ClientId of the player who shot it)
        if (bulletNetworkObject != null)
        {
            bulletNetworkObject.GetComponent<Bullet>().bulletID = ownerClientId;
        }
    }
}
