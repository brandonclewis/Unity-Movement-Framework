using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingGun : MonoBehaviour, IWeapon
{
    private Camera firstPerson;
    public Transform muzzle;
    
    // Start is called before the first frame update
    void Start()
    {
        firstPerson = GameObject.Find("First Person Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    private Ray viewRay;
    private Ray gunRay;
    private RaycastHit viewHit;
    private RaycastHit gunHit;
    public float maxGrappleDistance = 30f;
    public Vector3 grapplePoint;
    
    public void Fire()
    {
        viewRay = firstPerson.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(viewRay, out viewHit))
        {
            gunRay = new Ray(muzzle.position, viewHit.point - muzzle.position);
            if (Physics.Raycast(gunRay, out gunHit, maxGrappleDistance))
            {
                grapplePoint = gunHit.point;
            }
            else
            {
                grapplePoint = Vector3.zero;
            }
        }
        else
        {
            grapplePoint = Vector3.zero;
        }
    }

    public void AltFire()
    {
        
    }

    public void Reload()
    {
        
    }
    
    //Debug
    private void OnDrawGizmos()
    {
        if (!grapplePoint.Equals(Vector3.zero))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(muzzle.position,grapplePoint);
        }
    }
}
