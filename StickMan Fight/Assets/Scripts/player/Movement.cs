using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : NetworkBehaviour
{
    private PlayerInput input;
    private Vector2 moveVector = Vector2.zero;
    

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

    private void OnEnable()
    {
       
        input.Enable();
        input.PlayerMovement.WalkMovement.performed += OnMovementPerformed;
        input.PlayerMovement.WalkMovement.canceled += OnMovementCancled;
        

    }

    private void OnDisable()
    {
        
        input.Disable();
        input.PlayerMovement.WalkMovement.performed -= OnMovementPerformed;
        input.PlayerMovement.WalkMovement.canceled -= OnMovementCancled;
        

    }

    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        moveVector = value.ReadValue<Vector2>();
    }
    private void OnMovementCancled(InputAction.CallbackContext value)
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
        if (IsOwner)
        {
            walkMovement();

            isOnGround = Physics2D.OverlapCircle(playerPosition.position, positionRadius, ground);

            if (!isOnGround) {
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
        if (IsOwner)
        {
            
            if (isOnGround == true)
            {
                RB.AddForce(Vector2.up * jumpForce);
            }
        }
    }
}
