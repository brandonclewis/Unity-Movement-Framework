using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Transactions;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    
    public float gravity = -9.81f; //constant acceleration downwards
    public float terminalVelocity = -53f; //maximum fall speed
    public float maxAcceleration = 8.5725f;
    public float airSpeedCap = 0.5715f;
    public float groundAccelMult = 10f;
    public float airAccelMult = 10f;
    public float brakingDecelStock = 1.905f;
    public float friction = 4f;
    public float brakingFrictionFactor = 1f;
    public float maxWalkSpeedCrouched = 1.2065f;
    public float sprintSpeed = 6.096f; //sprint speed cap w/o strafing
    public float walkSpeed = 2.8575f; //walk speed cap w/o strafing
    public float jumpVelocity = 3.048f; //initial upwards velocity added on jump

    float velocityVertical;
    private bool isSprinting;
    private bool isWalking = true;
    private bool isCrouching;
    private bool isGrounded;
    private bool groundTooSloped;
    
    CharacterController controller;
    private ControllerColliderHit ground;
    
    private Vector3 velocitySum;
    private Vector3 velH;
    private Vector3 velHProj;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        velocityVertical = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //Detect ground collision
        RaycastHit ground = GetGroundHit();
        
        //Add gravity acceleration and clamp fall speed to the terminal velocity
        velocityVertical += Time.deltaTime * gravity;
        velocityVertical = Mathf.Clamp(velocityVertical, terminalVelocity, float.MaxValue);

        //Sprint and crouch logic
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        isCrouching = Input.GetKey(KeyCode.LeftControl);

        //Jump code
        if (isGrounded && !groundTooSloped && Input.GetAxisRaw("Jump") != 0f)
        {
            
            velocityVertical = jumpVelocity;
            isGrounded = false;
        }

        velH = VelocityHorizontal();

        //Combine vertical and horizontal velocity
        if (!groundTooSloped && !ground.Equals(default(RaycastHit)))
        {
            velocityVertical -= gravity * Time.deltaTime;
            velHProj = Vector3.ProjectOnPlane(velH, ground.normal).normalized * velH.magnitude;
            velocitySum = velHProj + Vector3.up * velocityVertical;
        } else
            velocitySum = velH + Vector3.up * velocityVertical;
        
        //On ground sharper than the slope limit, project entire velocity onto surface normal
        if(groundTooSloped)
            velocitySum = Vector3.ProjectOnPlane(velocitySum, ground.normal);

        //Move and rotate the player
        controller.Move(velocitySum * Time.deltaTime);
        transform.Rotate(CameraController.sensitivity * Input.GetAxisRaw("Mouse X") * Vector3.up,Space.Self);
        
        //After movement reset vertical velocity if player is grounded
        if(isGrounded && !groundTooSloped)
            velocityVertical = 0;
        
        print(velH.magnitude + "," + velocityVertical + "," + controller.velocity.magnitude);
    }

    Vector3 VelocityHorizontal()
    {
        Vector3 velocity = new Vector3(velH.x, 0, velH.z);
        Vector3 newVel = velocity;
        Vector3 inputDirection = (transform.forward * Input.GetAxisRaw("Vertical") +
                                  transform.right * Input.GetAxisRaw("Horizontal")).normalized;

        if (isGrounded)
            newVel = VelocityBraked(newVel);
        
        Vector3 acceleration = inputDirection * maxAcceleration;
        acceleration = Vector3.ClampMagnitude(acceleration, GetMaxSpeed());
        Vector3 accelDir = acceleration.normalized;
        float veer = velocity.x * accelDir.x + velocity.z * accelDir.z;
        float addSpeed = (isGrounded ? acceleration : Vector3.ClampMagnitude(acceleration, airSpeedCap))
            .magnitude - veer;

        if (addSpeed > 0f)
        {
            float accelMult = isGrounded ? groundAccelMult : airAccelMult;
            acceleration *= accelMult * Time.deltaTime;
            acceleration = Vector3.ClampMagnitude(acceleration, addSpeed);
            newVel += acceleration;
        }
        
        return newVel;
    }

    Vector3 VelocityBraked(Vector3 vel)
    {
        float brakingDecel = brakingDecelStock;
        Vector3 newVel = vel;
        float speed = vel.magnitude;

        if (speed <= 0.1f)
            return newVel;

        float frictionFactor = Mathf.Max(0f, brakingFrictionFactor);
        friction = Mathf.Max(0f, friction * frictionFactor);
        brakingDecel = Mathf.Max(brakingDecel, speed);
        brakingDecel = Mathf.Max(0, brakingDecel);
        bool zeroFriction = Mathf.Approximately(0, friction);
        bool zeroBraking = brakingDecel == 0f;

        if (zeroFriction || zeroBraking)
            return newVel;
        
        Vector3 reverseAccel = friction * brakingDecel * vel.normalized;
        newVel -= reverseAccel * Time.deltaTime;

        if (Vector3.Dot(newVel, vel) <= 0f)
            return Vector3.zero;

        if (Mathf.Pow(newVel.magnitude, 2) <= 0.00001)
            return Vector3.zero;

        return newVel;
    }

    float GetMaxSpeed()
    {
        if (isWalking)
        {
            if (isSprinting)
            {
                if (isCrouching)
                    return maxWalkSpeedCrouched * 1.7f;
                return sprintSpeed;
            }

            if (isCrouching)
                return maxWalkSpeedCrouched;
            return walkSpeed;
        }

        return 0;
    }

    RaycastHit GetGroundHit()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, controller.radius + controller.skinWidth, Vector3.down, out hitInfo,
            controller.height / 2 - controller.radius))
        {
            isGrounded = true;
            if (Vector3.Angle(Vector3.up, hitInfo.normal) >= controller.slopeLimit)
                groundTooSloped = true;
            else
                groundTooSloped = false;
        }
        else
        {
            isGrounded = false;
            groundTooSloped = false;
            hitInfo = default(RaycastHit);
        }
        return hitInfo;
        
    }
}
