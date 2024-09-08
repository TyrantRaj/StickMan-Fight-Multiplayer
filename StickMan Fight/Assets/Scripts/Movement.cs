using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private PlayerInput input;
    private Vector2 moveVector = Vector2.zero;

    public GameObject leftLeg;
    public GameObject rightLeg;
    Rigidbody2D leftLegRB;
    Rigidbody2D rightLegRB;
    public Rigidbody2D RB;

    public Animator anim;

    [SerializeField] float speed = 1.5f;
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

    // Start is called before the first frame update
    void Start()
    {
        leftLegRB = leftLeg.GetComponent<Rigidbody2D>();
        rightLegRB = rightLeg.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
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

        /*isOnGround = Physics2D.OverlapCircle(playerPosition.position, positionRadius, ground);
        if (isOnGround && Input.GetKeyDown(KeyCode.Space))
        {
            RB.AddForce(Vector2.up * jumpForce);
        }*/
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
        
        isOnGround = Physics2D.OverlapCircle(playerPosition.position, positionRadius, ground);
        if (isOnGround == true)
        {
            RB.AddForce(Vector2.up * jumpForce);
        }
        
    }
}
