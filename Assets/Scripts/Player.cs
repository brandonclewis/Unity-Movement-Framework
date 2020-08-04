using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Transactions;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    public float gravity = -9.81f; // Constant acceleration downwards
    public float terminalVelocity = -53f; // Maximum fall speed
    public float maxAcceleration = 8.5725f; // Maximum acceleration
    public float airSpeedCap = 0.5715f; // Maximum value for add speed
    public float groundAccelMult = 10f; // Grounded acceleration multiplier
    public float airAccelMult = 10f; // Aerial acceleration multiplier
    public float brakingDecelStock = 1.905f; // Default value for braking
    public float friction = 4f; // Ground and aerial friction
    public float brakingFrictionFactor = 1f; // Friction multiplier
    public float maxWalkSpeedCrouched = 1.2065f; // Crouch walk speed cap
    public float sprintSpeed = 6.096f; // Sprint speed cap w/o strafing
    public float walkSpeed = 2.8575f; // Walk speed cap w/o strafing
    public float jumpVelocity = 3.048f; // Initial upwards velocity added on jump
    public float animationSmoothTime = 0.1f; // Smooth time for changing animator variables
    
    private bool isSprinting;
    private bool isWalking = true;
    private bool isCrouching = false;
    private bool isGrounded;
    private bool groundTooSloped;
    
    private Animator animator;
    private CharacterController controller;
    private RaycastHit ground;

    private float velocityVertical;
    private Vector3 velocityHorizontal;
    private Vector3 velHProj;
    private Vector3 velocitySum;
    private Vector3 localVelocity;
    private Vector3 localVelocityChange;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Quit code
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();

        // Detect ground collision
        ground = GetGroundHit();
        
        // Add gravity acceleration and clamp fall speed to the terminal velocity
        velocityVertical += Time.deltaTime * gravity;
        velocityVertical = Mathf.Clamp(velocityVertical, terminalVelocity, float.MaxValue);

        // Sprint and crouch logic
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        //isCrouching = Input.GetKey(KeyCode.LeftControl);

        // Jump code
        if (isGrounded && !groundTooSloped && Input.GetAxisRaw("Jump") != 0f)
        {
            velocityVertical = jumpVelocity;
            isGrounded = false;
        }
        
        // Get horizontal velocity
        velocityHorizontal = CalculateVelocityHorizontal();

        // Combine vertical and horizontal velocity
        if (!groundTooSloped && !ground.Equals(default(RaycastHit)))
        {
            velocityVertical -= gravity * Time.deltaTime;
            velHProj = Vector3.ProjectOnPlane(velocityHorizontal, ground.normal).normalized * velocityHorizontal.magnitude;
            velocitySum = velHProj + Vector3.up * velocityVertical;
        } else
            velocitySum = velocityHorizontal + Vector3.up * velocityVertical;
        
        // On ground sharper than the slope limit, project entire velocity onto surface normal
        if(groundTooSloped)
            velocitySum = Vector3.ProjectOnPlane(velocitySum, ground.normal);
        
        // Smooth animation turn time and round to two digits
        localVelocity = Vector3.SmoothDamp(localVelocity, transform.InverseTransformDirection(velocityHorizontal), ref localVelocityChange, animationSmoothTime);
        animator.SetFloat("moveForward", Mathf.Round(localVelocity.z * 100f) / 100f);
        animator.SetFloat("moveSideways", Mathf.Round(localVelocity.x * 100f) / 100f);

        // Move and rotate the player
        controller.Move(velocitySum * Time.deltaTime);
        transform.Rotate(CameraController.sensitivity * Input.GetAxisRaw("Mouse X") * Vector3.up,Space.Self);
        
        // After movement reset vertical velocity if player is grounded
        if(isGrounded && !groundTooSloped)
            velocityVertical = 0;
    }

    // Returns the horizontal velocity based on player input
    Vector3 CalculateVelocityHorizontal()
    {
        // Take in player input and current velocity
        Vector3 currentVelocity = new Vector3(velocityHorizontal.x, 0, velocityHorizontal.z);
        Vector3 newVelocity = currentVelocity;
        Vector3 inputDirection = (transform.forward * Input.GetAxisRaw("Vertical") +
                                  transform.right * Input.GetAxisRaw("Horizontal")).normalized;
        
        // Apply friction
        if (isGrounded)
            newVelocity = VelocityBraked(newVelocity);
        
        // Apply friction and find direction of acceleration
        Vector3 acceleration = inputDirection * maxAcceleration;
        acceleration = Vector3.ClampMagnitude(acceleration, GetMaxSpeed());
        Vector3 accelDir = acceleration.normalized;
        
        // Calculate veer and the speed added
        float veer = currentVelocity.x * accelDir.x + currentVelocity.z * accelDir.z;
        float addSpeed = (isGrounded ? acceleration : Vector3.ClampMagnitude(acceleration, airSpeedCap))
            .magnitude - veer;

        // Add new speed to velocity
        if (addSpeed > 0f)
        {
            float accelMult = isGrounded ? groundAccelMult : airAccelMult;
            acceleration *= accelMult * Time.deltaTime;
            acceleration = Vector3.ClampMagnitude(acceleration, addSpeed);
            newVelocity += acceleration;
        }
        
        // Return the final velocity 
        return newVelocity;
    }

    // Returns a given velocity with friction applied
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

    // Returns the maximum allowed speed based on player state
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

    // Returns the ground based on spherecast
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
