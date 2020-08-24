using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject[] WeaponType = new GameObject[9];

    private KeyCode[] keycodes =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
    };
    
    private bool[] weaponInventory;
    private GameObject currentWeapon;
    public Transform weaponPosition;
    private GameObject parent;
    // Start is called before the first frame update
    void Start()
    {
        parent = GameObject.Find("First Person Camera");
        weaponInventory = new bool[WeaponType.Length];
        
        //Debug - give all weapons
        for(int x = 0;x<weaponInventory.Length;x++)
        {
            weaponInventory[x] = true;
        }
    }

    public void SwitchWeapon(GameObject weapon)
    {
        if (weaponInventory.Contains(weapon)) //Add check for same weapon later
        {
            Destroy(currentWeapon);
            currentWeapon = Instantiate<GameObject>(weapon, weaponPosition.position, weaponPosition.rotation, parent.transform);
        }
    }

    public void AcquireWeapon(GameObject weapon)
    {
        weaponInventory[Array.IndexOf(WeaponType,weapon)] = true;
        SwitchWeapon(weapon);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < keycodes.Length; i++)
        {
            if (Input.GetKeyDown(keycodes[i]))
            {
                SwitchWeapon(WeaponType[i]);
                break;
            }
        }
    }
}
