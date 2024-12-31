using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class squareplayermovement : NetworkBehaviour
{
    private PlayerInput input;
    private Vector2 moveVector = Vector2.zero;
    private bool canMove = true;
    [SerializeField] Rigidbody2D rb;

    //public Animator anim;

    float speed = 1.5f;
    [SerializeField] float onAirSpeed = 1.0f;
    [SerializeField] float onGroundSpeed = 1.0f;
    [SerializeField] float jumpForce = 10;
    private bool isOnGround;
    public float positionRadius;
    public LayerMask ground;
    public Transform playerPosition;

    private void Awake()
    {
        input = new PlayerInput();
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

    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        Vector2 joystickInput = value.ReadValue<Vector2>();
        moveVector = new Vector2(joystickInput.x, 0); // Only use the horizontal axis
    }

    private void OnMovementCanceled(InputAction.CallbackContext value)
    {
        moveVector = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext value)
    {
        
        if (IsOwner && canMove && isOnGround)
        {
            //Debug.Log("jumping");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (IsOwner && canMove)
        {
            Move();

            isOnGround = Physics2D.OverlapCircle(playerPosition.position, positionRadius, ground);

            if (!isOnGround)
            {
                speed = onAirSpeed;
            }
            else
            {
                speed = onGroundSpeed;
            }
        }
    }

    /*private void Move()
    {
        if (moveVector.x != 0)
        {
            rb.velocity = new Vector2(moveVector.x * speed, rb.velocity.y); // Move left or right
            //anim.SetBool("isWalking", true);
            if (moveVector.x > 0)
            {
                //anim.Play("WalkRight");
            }
            else
            {
                //anim.Play("WalkLeft");
            }
        }
        else
        {
            //anim.SetBool("isWalking", false);
            //anim.Play("Idle");
        }
    }*/

    private void Move()
    {
        if (moveVector.x != 0)
        {
            rb.velocity = new Vector2(moveVector.x * speed, rb.velocity.y); // Apply movement
        }
        else
        {
            // Stop horizontal movement
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }


    public void freezePlayer(bool freeze)
    {
        if (!IsOwner) return;

        if (freeze)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = 1;
        }
    }

    public void SetMovement(bool state)
    {
        if (!IsOwner) return;
        canMove = state;
    }
}
