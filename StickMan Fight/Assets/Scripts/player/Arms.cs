using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Arms : MonoBehaviour
{
    [SerializeField] private InputActionReference aimDir;
    [SerializeField] private float shootingDelay = 0.5f; // Time between each shot in seconds
    [SerializeField] private PlayerShooting shooting; // Reference to your shooting script
    private Vector3 Playeraim;
    private int speed = 1000;
    private bool isShooting = false; // Prevents multiple coroutines from starting simultaneously

    public Rigidbody2D armRB;

    // Update is called once per frame
    void Update()
    {
        if (aimDir.action.enabled)
        {
            Vector2 aim = aimDir.action.ReadValue<Vector2>();
            Playeraim = new Vector3(aim.x, aim.y, 0);
        }
    }

    private void FixedUpdate()
    {
        // Left Arm Rotation
        if (armRB.tag == "LeftArm")
        {
            float Leftangle = Mathf.Atan2(-Playeraim.x, Playeraim.y) * Mathf.Rad2Deg;
            if (Leftangle != 0)
            {
                armRB.MoveRotation(Mathf.Lerp(armRB.rotation, Leftangle, speed));
                // Add shooting logic if needed for the left arm
            }
        }

        // Right Arm Rotation and Shooting
        if (armRB.tag == "RightArm")
        {
            float Rightangle = Mathf.Atan2(Playeraim.x, -Playeraim.y) * Mathf.Rad2Deg;
            if (Rightangle != 180)
            {
                armRB.MoveRotation(Mathf.Lerp(armRB.rotation, Rightangle, speed));

                // Start shooting coroutine if not already shooting
                if (!isShooting)
                {
                    StartCoroutine(ShootWithDelay());
                }
            }
        }
    }

    // Coroutine to shoot bullets with a delay
    private IEnumerator ShootWithDelay()
    {
        isShooting = true;
        shooting.shoot(); // First shot

        // Wait for the specified delay before the next shot
        yield return new WaitForSeconds(shootingDelay);

        shooting.shoot(); // Second shot, add more shooting logic as needed

        // After shooting, reset the shooting state
        isShooting = false;
    }
}
