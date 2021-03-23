using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInventory : MonoBehaviour
{
    public Queue<MissileWeapon> allMissileWeapons = new Queue<MissileWeapon>();
    public Queue<GunWeapon> allGunWeapons = new Queue<GunWeapon>();
    public MissileWeapon currentMissileWeapon;
    public GunWeapon currentGunWeapon;
    private void Awake()
    {
        MissileWeapon[] allFoundMissileWeapons = GetComponentsInChildren<MissileWeapon>();
        foreach (MissileWeapon missileWeapon in allFoundMissileWeapons) { allMissileWeapons.Enqueue(missileWeapon); missileWeapon.SetActive(false); }
        currentMissileWeapon = allMissileWeapons.Peek();
        currentMissileWeapon.SetActive(true);
        GunWeapon[] allFoundGunWeapons = GetComponentsInChildren<GunWeapon>();
        foreach (GunWeapon gunWeapon in allFoundGunWeapons) { allGunWeapons.Enqueue(gunWeapon); gunWeapon.SetActive(false); }
        currentGunWeapon = allGunWeapons.Peek();
        currentGunWeapon.SetActive(true);
    }
    private void Update()
    {
        if (Mouse.current.scroll.ReadValue().y < 0) SwitchToNextMissileWeapon();
        if (Mouse.current.scroll.ReadValue().y > 0) SwitchToNextGunWeapon();
    }

    private void SwitchToNextGunWeapon()
    {
        allGunWeapons.Peek().SetActive(false);
        allGunWeapons.Enqueue(allGunWeapons.Dequeue());
        currentGunWeapon = allGunWeapons.Peek();
        currentGunWeapon.SetActive(true);
    }

    private void SwitchToNextMissileWeapon()
    {
        allMissileWeapons.Peek().SetActive(false);
        allMissileWeapons.Enqueue(allMissileWeapons.Dequeue());
        currentMissileWeapon = allMissileWeapons.Peek();
        currentMissileWeapon.SetActive(true);
    }
}
