using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public static int sensitivity;
    public int s = 3;
    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        sensitivity = s;
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(player.transform.position,player.transform.right,-1 * sensitivity * Input.GetAxis("Mouse Y"));
    }
}
