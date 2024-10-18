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
    [SerializeField] float startDelay = 0f;
    private Vector3 startPosition;
    private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        networkComponent = GetComponent<NetworkObject>();
        startPosition = points[startingPoint].position;
        position.Value = startPosition; // Initialize position
        transform.position = position.Value; // Set initial position
        enabled = false;

        if (IsServer)
        {
            StartCoroutine(DelayMovement());
        }
        else if (IsClient) {
            StartCoroutine(DelayMovement());
        }
    }

    private IEnumerator DelayMovement()
    {
        yield return new WaitForSeconds(startDelay);
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

            // Move towards the next point
            Vector3 newPosition = Vector2.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
            transform.position = newPosition; // Update the transform position

            // Update the synchronized position
            position.Value = newPosition; // This will automatically sync with clients
        }
        else if (IsClient)
        {
            // Update the transform position if the position is changed by the server
            transform.position = position.Value;
        }
    }
}
