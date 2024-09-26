using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DestroyEnvironment : NetworkBehaviour
{
    [ServerRpc]
    private void ReduceBoxHealthServerRpc(NetworkObjectReference boxReference)
    {
        Debug.Log("inside serverrpc");
        if (boxReference.TryGet(out NetworkObject boxNetworkObject))
        {
            var box = boxNetworkObject.GetComponent<Box>();
            Debug.Log("inside serverrpc");
            if (box != null)
            {
                if (box.boxHealth < 0)
                {
                    boxNetworkObject.Despawn();
                }
                else
                {
                    box.boxHealth -= 10;
                    Debug.Log("Health reduced");
                }
            }
            else
            {
                Debug.Log("not found");
            }
        }
    }
}
