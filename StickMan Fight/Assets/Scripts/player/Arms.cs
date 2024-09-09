using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.InputSystem;
using UnityEditor.Experimental;
using Unity.Burst.Intrinsics;
public class Arms : MonoBehaviour
{
    [SerializeField] private InputActionReference aimDir;
    Vector3 playerpos;
    int speed = 1000;
    public Camera cam;
    public Rigidbody2D armRB;
    public Joystick joystick;
    Vector3 Playeraim;


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
        if (armRB.tag == "LeftArm")
        {
            float Leftangle = Mathf.Atan2(-Playeraim.x, Playeraim.y) * Mathf.Rad2Deg;
            if (Leftangle != 0)
            {
                armRB.MoveRotation(Mathf.Lerp(armRB.rotation, Leftangle, speed));
            }
        }

        if (armRB.tag == "RightArm")
        {
            float Rightangle = Mathf.Atan2(Playeraim.x, -Playeraim.y) * Mathf.Rad2Deg;
            if (Rightangle != 180)
            {
                armRB.MoveRotation(Mathf.Lerp(armRB.rotation, Rightangle, speed));
            }
        }
    }
}
