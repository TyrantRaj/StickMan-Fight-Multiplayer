using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : NetworkBehaviour
{
    private PlayerInput input;
    private Vector2 moveVector = Vector2.zero;
    private bool canMove = true;
    [SerializeField] Rigidbody2D[] rigidbodies;
    public GameObject leftLeg;
    public GameObject rightLeg;
    Rigidbody2D leftLegRB;
    Rigidbody2D rightLegRB;
    public Rigidbody2D RB;

    public Animator anim;

    float speed = 1.5f;
    [SerializeField] float OnairSpeed = 1.0f;
    [SerializeField] float OngroundSpeed = 1.0f;
    [SerializeField] float stepwait = 0.5f;
    [SerializeField] float jumpForce = 10;
    private bool isOnGround;
    public float positionRadius;
    public LayerMask ground;
    public Transform playerPosition;

    private void Awake()
    {
        input = new PlayerInput();
    }

    /*private void OnEnable()
    {
        input.Enable();
        input.PlayerMovement.WalkMovement.performed += OnMovementPerformed;
        input.PlayerMovement.WalkMovement.canceled += OnMovementCanceled;
    }

    private void OnDisable()
    {
        input.Disable();
        input.PlayerMovement.WalkMovement.performed -= OnMovementPerformed;
        input.PlayerMovement.WalkMovement.canceled -= OnMovementCanceled;
    }

    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        moveVector = value.ReadValue<Vector2>();
    }


    private void OnMovementCanceled(InputAction.CallbackContext value)
    {
        moveVector = Vector2.zero;
    }*/

    private void OnEnable()
    {
        input.Enable();
        input.PlayerMovement.WalkMovement.performed += OnMovementPerformed;
        input.PlayerMovement.WalkMovement.canceled += OnMovementCanceled;
    }

    private void OnDisable()
    {
        input.Disable();
        input.PlayerMovement.WalkMovement.performed -= OnMovementPerformed;
        input.PlayerMovement.WalkMovement.canceled -= OnMovementCanceled;
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



    void Start()
    {
        leftLegRB = leftLeg.GetComponent<Rigidbody2D>();
        rightLegRB = rightLeg.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (IsOwner && canMove)
        {
            walkMovement();

            isOnGround = Physics2D.OverlapCircle(playerPosition.position, positionRadius, ground);

            if (!isOnGround)
            {
                speed = OnairSpeed;
            }
            else
            {
                speed = OngroundSpeed;
            }
        }
    }

    private void walkMovement()
    {
        if (moveVector.x != 0)
        {
            if (moveVector.x > 0)
            {
                anim.Play("WalkRight");
                StartCoroutine(MoveRight(stepwait));
            }
            else
            {
                anim.Play("WalkLeft");
                StartCoroutine(MoveLeft(stepwait));
            }
        }
        else
        {
            anim.Play("Idle");
        }
    }

    IEnumerator MoveRight(float seconds)
    {
        leftLegRB.AddForce(Vector2.right * Time.deltaTime * (speed * 1000));
        yield return new WaitForSeconds(seconds);
        leftLegRB.AddForce(Vector2.right * Time.deltaTime * (speed * 1000));
    }

    IEnumerator MoveLeft(float seconds)
    {
        leftLegRB.AddForce(Vector2.left * Time.deltaTime * (speed * 1000));
        yield return new WaitForSeconds(seconds);
        leftLegRB.AddForce(Vector2.left * Time.deltaTime * (speed * 1000));
    }

    private void OnJump()
    {
        if (IsOwner && canMove && isOnGround)
        {
            RB.AddForce(Vector2.up * jumpForce);
        }
    }

    public void freezePlayer(bool freeze)
    {
        if (!IsOwner) return;

        if (freeze)
        {
            foreach (Rigidbody2D rb in rigidbodies)
            {
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        else
        {
            foreach (Rigidbody2D rb in rigidbodies)
            {
                rb.gravityScale = 1;
                rb.constraints = RigidbodyConstraints2D.None;
            }
        }
    }

    public void SetMovement(bool state)
    {
        if (!IsOwner) return;
        canMove = state;
    }
}
