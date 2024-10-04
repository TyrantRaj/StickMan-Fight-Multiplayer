using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform[] firePoint; // Fire points (from different guns)
    public float bulletSpeed = 25f;
    public float recoilForce = 5f;  // Recoil force applied to the arm
    public float recoilAngle = 5f;  // Max angle deviation for recoil

    [HideInInspector]
    public bool hasGun = false;
    public int currentGun;

    [SerializeField] private Rigidbody2D rb;
    public Transform arm; // The player's arm or gun Transform


    public void shoot()
    {
        // Only allow the local player to shoot
        if (IsOwner)
        {
            if (hasGun)
            {
               

                // Apply recoil rotation to the arm/gun
                ApplyRecoil();

                // Call the ServerRpc to spawn and assign the bullet with adjusted direction
                Vector3 shootDirection = firePoint[currentGun].right;
                ShootServerRpc(firePoint[currentGun].position, shootDirection);
            }
        }
        else
        {
            Debug.Log("isOwner failed");
        }
    }

    // Apply recoil rotation to the arm in addition to force
    private void ApplyRecoil()
    {
        // Apply slight random angle change to simulate recoil
        float randomRecoilAngle = Random.Range(-recoilAngle, recoilAngle); // Random recoil angle within set range
        arm.Rotate(0, 0, randomRecoilAngle); // Apply recoil rotation to the arm

        // Get the direction of the shot (opposite direction of bullet)
        Vector2 recoilDirection = -firePoint[currentGun].right;

        // Apply force to the player's Rigidbody2D in the opposite direction of the shot
        rb.AddForce(recoilDirection * recoilForce, ForceMode2D.Impulse);
    }

    [ServerRpc]
    public void ShootServerRpc(Vector3 position, Vector3 direction)
    {

        // Instantiate the bullet
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);

        // Adjust bullet velocity based on the current firePoint direction
        bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;

        // Spawn bullet on the network
        var bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn();

        // Assign bullet to the shooter
        AssignBulletIDClientRpc(bulletNetworkObject.NetworkObjectId, OwnerClientId);
    }

    [ClientRpc]
    private void AssignBulletIDClientRpc(ulong bulletNetworkObjectId, ulong ownerClientId)
    {
        // Find bullet and assign ownership
        NetworkObject bulletNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[bulletNetworkObjectId];
        if (bulletNetworkObject != null)
        {
            bulletNetworkObject.GetComponent<Bullet>().bulletID = ownerClientId;
        }
    }
}
