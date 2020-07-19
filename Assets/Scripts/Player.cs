using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed = 10;
    public float gravity = -9.81f;
    public float terminalVelocity = -53f;
    
    float velocityY;

    CharacterController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        velocityY = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (velocityY >= terminalVelocity)
            velocityY += (Time.deltaTime * gravity);
        else
            velocityY = terminalVelocity;

        Vector3 velocity = transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal");
        velocity = velocity.normalized * speed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);
        if(controller.isGrounded)
            velocityY = 0;
        transform.Rotate(FollowCamera.sensitivity * Input.GetAxis("Mouse X") * Vector3.up,Space.Self);
        
        print(controller.velocity.magnitude);
    }
}
