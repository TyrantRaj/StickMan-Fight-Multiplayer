using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BoxObstacleMovement : NetworkBehaviour
{
    private float speed = 4f;
    private int startingPoint = 0;
    public Transform[] points;
    private NetworkObject networkComponent;
    private int i;

    private Vector3 startPosition;
    private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        networkComponent = GetComponent<NetworkObject>();
        startPosition = points[startingPoint].position;
        enabled = false;

        if (IsServer)
        {
            position.Value = startPosition;
            StartCoroutine(DelayMovement());
        }
    }

    private IEnumerator DelayMovement()
    {
        yield return new WaitForSeconds(4f);
        enabled = true;
    }

    private void Update()
    {
        if (IsServer && networkComponent.IsSpawned)
        {
            // Move to the next point when close enough to the current target
            if (Vector2.Distance(transform.position, points[i].position) < 0.02f)
            {
                i++;
                if (i == points.Length)
                {
                    i = 0;
                }
            }

            // Move towards the next point and update the synced position
            Vector3 newPosition = Vector2.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
            transform.position = newPosition; // Update the transform position

            // Call the RPC to update clients
            UpdatePositionClientRpc(newPosition);
        }
    }

    [ClientRpc]
    private void UpdatePositionClientRpc(Vector3 newPosition)
    {
        // This function will be called on all clients
        transform.position = newPosition; // Update the transform position
    }
}
