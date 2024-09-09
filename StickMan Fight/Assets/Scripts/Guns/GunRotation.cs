using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class GunRotation : NetworkBehaviour
{
    [SerializeField] private InputActionReference aimDir;
    [SerializeField] private GameObject GunSprite;

    void FixedUpdate()
    {
        if (IsOwner)
        {
            Vector2 aim = aimDir.action.ReadValue<Vector2>();

            if (aim != Vector2.zero)
            {
                if ((aim.x > 0 && aim.y > 0) || (aim.x > 0 && aim.y < 0))
                {
                    ChangeSpriteOnServerServerRpc(true); // Flip sprite to the left
                }
                else
                {
                    ChangeSpriteOnServerServerRpc(false); // Flip sprite to the right
                }
            }
        }
    }

    // Send the sprite flip request from the client to the server
    [ServerRpc]
    private void ChangeSpriteOnServerServerRpc(bool flipLeft)
    {
        ChangeSpriteOnClientsClientRpc(flipLeft);
    }

    // Change the sprite on all clients
    [ClientRpc]
    private void ChangeSpriteOnClientsClientRpc(bool flipLeft)
    {
        if (flipLeft)
        {
            GunSprite.gameObject.GetComponent<SpriteRenderer>().flipY = false; // Left
        }
        else
        {
            GunSprite.gameObject.GetComponent<SpriteRenderer>().flipY = true; // Right
        }
    }
}
