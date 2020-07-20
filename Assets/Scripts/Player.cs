using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{

    public float maxSpeed = 6f;
    public float gravity = -9.81f;
    public float terminalVelocity = -53f;
    public float maxAcceleration = 8.5725f;
    public float airSpeedCap = 0.5715f;
    public float groundAccelMult = 10f;
    public float airAccelMult = 10f;
    public float brakingDecel = 1.905f;
    public float friction = 1f;
    public float brakingFrictionFactor = 1f;
    
    float velocityVertical;

    CharacterController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        velocityVertical = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (velocityVertical >= terminalVelocity)
            velocityVertical += (Time.deltaTime * gravity);
        else
            velocityVertical = terminalVelocity;

        //Vector3 velocity = transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal");
        //velocity = velocity.normalized * maxSpeed + Vector3.up * velocityVertical;
        Vector3 velocity = VelocityHorizontal() + Vector3.up * velocityVertical;

        controller.Move(velocity * Time.deltaTime);
        if(controller.isGrounded)
            velocityVertical = 0;
        transform.Rotate(FollowCamera.sensitivity * Input.GetAxis("Mouse X") * Vector3.up,Space.Self);
        
        print(controller.velocity.magnitude);
    }

    Vector3 VelocityHorizontal()
    {
        Vector3 velocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        Vector3 newVel = velocity;
        Vector3 inputDirection = (transform.forward * Input.GetAxisRaw("Vertical") +
                                  transform.right * Input.GetAxisRaw("Horizontal")).normalized;

        if (controller.isGrounded)
        {
            float speed = velocity.magnitude;

            float frictionFactor = Mathf.Max(0f, brakingFrictionFactor);
            friction = Mathf.Max(0f, friction * frictionFactor);
            brakingDecel = Mathf.Max(brakingDecel, speed);
            brakingDecel = Mathf.Max(0, brakingDecel);
            
            bool zeroFriction = Mathf.Approximately(0, friction);
            bool zeroBraking = brakingDecel == 0f;

            if (!(zeroFriction || zeroBraking))
            {
                Vector3 reverseAccel = friction * brakingDecel * velocity.normalized;
                newVel -= reverseAccel * Time.deltaTime;
                if (Vector3.Dot(newVel, velocity) <= 0f)
                {
                    newVel = Vector3.zero;
                }
            }
        }
        
        
        Vector3 acceleration = inputDirection * maxAcceleration;
        acceleration = Vector3.ClampMagnitude(acceleration, maxSpeed);
        Vector3 accelDir = acceleration.normalized;
        float veer = velocity.x * accelDir.x + velocity.z * accelDir.z;
        float addSpeed = (controller.isGrounded ? acceleration : Vector3.ClampMagnitude(acceleration, airSpeedCap))
            .magnitude - veer;

        if (addSpeed > 0f)
        {
            float accelMult = controller.isGrounded ? groundAccelMult : airAccelMult;
            acceleration *= accelMult * Time.deltaTime;
            acceleration = Vector3.ClampMagnitude(acceleration, addSpeed);
            newVel += acceleration;
        }

        return newVel;
    }
}
