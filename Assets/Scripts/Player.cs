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
    public float brakingDecel = 1.905f;
    public float friction = 1f;
    public float brakingFrictionFactor = 1f;
    public float maxWalkSpeedCrouched = 1.2065f;
    public float sprintSpeed = 6.096f; //sprint speed cap w/o strafing
    public float walkSpeed = 2.8575f; //walk speed cap w/o strafing
    public float jumpVelocity = 3.048f; //initial upwards velocity added on jump

    float velocityVertical;
    private bool isSprinting = false;
    private bool isWalking = true;
    private bool isCrouching = false;

    private ControllerColliderHit ground;

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
        velocityVertical += Time.deltaTime * gravity;
        velocityVertical = Mathf.Clamp(velocityVertical, terminalVelocity, float.MaxValue);

        isSprinting = Input.GetKey(KeyCode.LeftShift);
        isCrouching = Input.GetKey(KeyCode.LeftControl);
        
        if (controller.isGrounded && ground == null && Input.GetAxisRaw("Jump") != 0f)
            velocityVertical += jumpVelocity;

        Vector3 velocitySum = VelocityHorizontal() + Vector3.up * velocityVertical;

        if(ground != null)
            velocitySum = Vector3.ProjectOnPlane(velocitySum, ground.normal).normalized * velocitySum.magnitude;

        controller.Move(velocitySum * Time.deltaTime);
        if(controller.isGrounded && ground == null)
            velocityVertical = 0;
        transform.Rotate(CameraController.sensitivity * Input.GetAxisRaw("Mouse X") * Vector3.up,Space.Self);
        
        print(controller.velocity.magnitude);
    }
    Vector3 VelocityHorizontal()
    {
        Vector3 velocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        Vector3 newVel = velocity;
        Vector3 inputDirection = (transform.forward * Input.GetAxisRaw("Vertical") +
                                  transform.right * Input.GetAxisRaw("Horizontal")).normalized;

        if (controller.isGrounded)
            newVel = VelocityBraked(newVel);
        
        Vector3 acceleration = inputDirection * maxAcceleration;
        acceleration = Vector3.ClampMagnitude(acceleration, GetMaxSpeed());
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

    Vector3 VelocityBraked(Vector3 vel)
    {
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
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Vector3.Angle(Vector3.up, hit.normal) >= controller.slopeLimit)
            ground = hit;
        else
            ground = null;
        
    }
}
