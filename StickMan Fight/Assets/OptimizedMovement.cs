using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class OptimizedRagdollMovement : NetworkBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D leftLegRB;
    [SerializeField] private Rigidbody2D rightLegRB;
    [SerializeField] private Rigidbody2D bodyRB;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float stepWait = 0.5f;

    private PlayerInput input;
    private Vector2 moveInput;
    private Vector3 networkPosition;
    private bool isOnGround;

    private void Awake()
    {
        input = new PlayerInput(); // Auto-generated Input Action class
    }

    private void OnEnable()
    {
        input.Enable();
        input.PlayerMovement.WalkMovement.performed += OnMovementPerformed;
        input.PlayerMovement.WalkMovement.canceled += OnMovementCanceled;
        input.PlayerMovement.Jump.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.PlayerMovement.WalkMovement.performed -= OnMovementPerformed;
        input.PlayerMovement.WalkMovement.canceled -= OnMovementCanceled;
        input.PlayerMovement.Jump.performed -= OnJumpPerformed;
    }

    private void Update()
    {
        if (IsOwner) // Only the owner controls movement
        {
            HandleLocalMovement();
            UpdateGroundedStatus();
            SendPositionToServerRpc(playerTransform.position);
        }
        else // Other clients smoothly interpolate position
        {
            playerTransform.position = Vector3.Lerp(playerTransform.position, networkPosition, Time.deltaTime * moveSpeed);
        }
    }

    private void HandleLocalMovement()
    {
        if (moveInput.x != 0)
        {
            // Determine the direction and play the corresponding walking animation
            if (moveInput.x > 0)
            {
                animator.Play("WalkRight");
                // Call MoveLegs coroutine to move the leg in the right direction
                StartCoroutine(MoveLegs(Vector2.right, stepWait));
            }
            else
            {
                animator.Play("WalkLeft");
                // Call MoveLegs coroutine to move the leg in the left direction
                StartCoroutine(MoveLegs(Vector2.left, stepWait));
            }
        }
        else
        {
            // If there's no movement, play idle animation
            animator.Play("Idle");
        }
    }

    IEnumerator MoveLegs(Vector2 direction, float seconds)
    {
        // Apply force to the leg's Rigidbody2D to simulate walking
        leftLegRB.AddForce(direction * Time.deltaTime * (moveSpeed * 1000)); // Move left leg
        rightLegRB.AddForce(direction * Time.deltaTime * (moveSpeed * 1000)); // Move right leg

        yield return new WaitForSeconds(seconds); // Wait for a short duration before the next step

        // Optionally, apply additional force for continuous walking effect
        leftLegRB.AddForce(direction * Time.deltaTime * (moveSpeed * 1000)); // Move left leg
        rightLegRB.AddForce(direction * Time.deltaTime * (moveSpeed * 1000)); // Move right leg
    }


    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (IsOwner && isOnGround)
        {
            bodyRB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.Play("Jump");
        }
    }

    private void UpdateGroundedStatus()
    {
        isOnGround = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    [ServerRpc]
    private void SendPositionToServerRpc(Vector3 position)
    {
        BroadcastPositionToClientRpc(position);
    }

    [ClientRpc]
    private void BroadcastPositionToClientRpc(Vector3 position)
    {
        if (IsOwner) return;
        networkPosition = position;
    }
}
