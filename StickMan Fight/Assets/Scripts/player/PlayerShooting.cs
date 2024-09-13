using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    void Update()
    {
        // Only allow the local player to shoot
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            ShootServerRpc(firePoint.position, firePoint.right);
        }
    }

    // Request the server to spawn a bullet
    [ServerRpc]
    private void ShootServerRpc(Vector3 position, Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;

        // Spawn the bullet across the network
        bullet.GetComponent<NetworkObject>().Spawn();
    }
}
