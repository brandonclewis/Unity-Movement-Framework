using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public static int sensitivity = 2;
    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(player.transform.position,player.transform.right,-1 * sensitivity * Input.GetAxis("Mouse Y"));
    }
}
