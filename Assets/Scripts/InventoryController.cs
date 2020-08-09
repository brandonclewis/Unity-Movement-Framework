using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public enum WeaponType
    {
        Pistol = 0,
        GrapplingHook = 1,
        LightningGun = 2,
        RocketLauncher = 3,
        GravityGun = 4,
        PhysicsGun = 5,
        MagmaGun = 6,
        MachineGun = 7,
        Laser = 8,
    }

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
    private WeaponType currentWeapon;

    // Start is called before the first frame update
    void Start()
    {
        weaponInventory = new bool[Enum.GetValues(typeof(WeaponType)).Length];
        
        // Debug - give all weapons
        for (int j = 0; j < weaponInventory.Length; j++)
        {
            weaponInventory[j] = true;
        }
    }

    public void SwitchWeapon(WeaponType weapon)
    {
        if (weaponInventory[(int)weapon])
        {
            currentWeapon = weapon;
        }
    }

    public void AcquireWeapon(WeaponType weapon)
    {
        weaponInventory[(int) weapon] = true;
        SwitchWeapon(weapon);
    }

    // Update is called once per frame
    void Update()
    {
        print(currentWeapon);
        for (int i = 0; i < keycodes.Length; i++)
        {
            if (Input.GetKeyDown(keycodes[i]))
            {
                SwitchWeapon((WeaponType)i);
                break;
            }
        }
    }
}
