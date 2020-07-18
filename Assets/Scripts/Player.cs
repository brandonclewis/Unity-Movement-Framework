using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")).normalized;
        transform.Translate(inputDirection * speed);
        transform.Rotate(FollowCamera.sensitivity * Input.GetAxis("Mouse X") * Vector3.up,Space.Self);
    }
}
