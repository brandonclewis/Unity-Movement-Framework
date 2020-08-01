using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera thirdPerson;
    public Camera firstPerson;
    GameObject player;
    public static int sensitivity = 3;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        thirdPerson.enabled = false;
        firstPerson.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            thirdPerson.enabled = !thirdPerson.enabled;
            firstPerson.enabled = !firstPerson.enabled;
        }
        thirdPerson.transform.RotateAround(player.transform.position,player.transform.right,-1 * sensitivity * Input.GetAxisRaw("Mouse Y"));
        firstPerson.transform.RotateAround(firstPerson.transform.position,firstPerson.transform.right,-1 * sensitivity * Input.GetAxisRaw("Mouse Y"));
    }
}