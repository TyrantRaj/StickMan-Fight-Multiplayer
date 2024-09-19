using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class GunRotation : NetworkBehaviour
{
    [SerializeField] private InputActionReference aimDir;  // Input action for aiming
    [SerializeField] private SpriteRenderer gunSprite;  // Reference to the gun sprite renderer
    
    /*private void OnEnable()
    {
        if (aimDir != null)
        {
            aimDir.action.Enable();
        }
        else
        {
            Debug.LogError("aimDir is not assigned.");
        }
    }

    private void OnDisable()
    {
        if (aimDir != null)
        {
            aimDir.action.Disable();
        }
    }*/

    private void Update()
    {
        if (IsOwner)
        {
            //Debug.Log("working");
            Vector2 aim = aimDir.action.ReadValue<Vector2>();
            if (aim != Vector2.zero && gunSprite != null)
            {
                bool flipLeft = aim.x < 0;
                
                ChangeSpriteOnServerServerRpc(flipLeft);
            }
        }
    }

    [ServerRpc]
    private void ChangeSpriteOnServerServerRpc(bool flipLeft)
    {
        ChangeSpriteOnClientsClientRpc(flipLeft);
    }

    [ClientRpc]
    private void ChangeSpriteOnClientsClientRpc(bool flipLeft)
    {
        if (gunSprite != null)
        {
            gunSprite.flipY = flipLeft;
        }
    }
}
