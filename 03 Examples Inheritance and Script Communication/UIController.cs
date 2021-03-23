using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Image mouseReticle, boreSight, lockChargeBar;
    public Camera cam;
    public Transform shipMesh;
    public Canvas canvas;
    public Text secondaryWeaponName, missileAmmoCounter, playerHealthCounter, primaryWeaponName, gunAmmoCounter;
    Mouse ms;
    WeaponInventory weaponInventory;
    MissileWeapon cmw; // "currentMissileWeapon"
    GunWeapon cgw; // "currentGunWeapon"
    PlayerHealth playerHealth;
    Vector3 screenPos, boreSightPos;
    private void Awake()
    {
        ms = Mouse.current;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        weaponInventory = FindObjectOfType<WeaponInventory>();
        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    void LateUpdate()
    {
        screenPos = ms.position.ReadValue();
        mouseReticle.transform.position = screenPos;

        HealthFeedBack();
        GunWeaponFeedBack();
        MissileLauncherLockFeedBack();
    }

    private void GunWeaponFeedBack()
    {
        cgw = weaponInventory.currentGunWeapon;
        if (cgw.reticleOverTarget) boreSightPos = cam.WorldToScreenPoint(cgw.hit.point);
        else boreSightPos = cam.WorldToScreenPoint(cgw.gunPositions[cgw.currentGunIndex].position + cgw.gunPositions[cgw.currentGunIndex].forward * 900f);
        boreSight.transform.position = boreSightPos;

        if (!cgw.reticleOverTarget)
        {
            boreSight.color = Color.green;
            boreSight.transform.localEulerAngles -= new Vector3(0, 0, Mathf.Lerp(boreSight.transform.localEulerAngles.z, 0, .9f));
        }
        else
        {
            boreSight.color = Color.red;
            boreSight.transform.localEulerAngles += new Vector3(0, 0, 180f * Time.deltaTime);
            if (boreSight.transform.localEulerAngles.z > 90f) boreSight.transform.localEulerAngles -= new Vector3(0, 0, 90); ;
        }
        primaryWeaponName.text = cgw.name;
        gunAmmoCounter.text = cgw.currentAmmo + " | " + cgw.maxAmmo;
    }

    private void HealthFeedBack()
    {
        playerHealthCounter.text = playerHealth.currentHealth + " | " + playerHealth.maxHealth;
    }

    private void MissileLauncherLockFeedBack()
    {
        cmw = weaponInventory.currentMissileWeapon;
        if (cmw is PlayerMissileLauncher)
        {
            if (cmw.hasLockedOn) { mouseReticle.color = Color.red; lockChargeBar.color = Color.red; }
            else if (cmw.hasMouseOverTarget) { mouseReticle.color = Color.yellow; lockChargeBar.color = Color.yellow; }
            else { mouseReticle.color = Color.green; lockChargeBar.color = Color.green; }

            if (cmw.currentAmmo < 1) lockChargeBar.transform.localScale = new Vector3(lockChargeBar.transform.localScale.x, 0, 1);
            else lockChargeBar.transform.localScale = new Vector3(lockChargeBar.transform.localScale.x,
                Mathf.Max(0, cmw.currentLockTime) / cmw.requiredLockTime, 1);
        }
        else if (cmw is PlayerBurstMissileLauncher)
        {
            if (cmw.isLaunching) mouseReticle.color = Color.red;
            else if (cmw.hasMouseOverTarget) mouseReticle.color = Color.red;
            else if (cmw.hasLockedOn) mouseReticle.color = Color.yellow;
            else mouseReticle.color = Color.green;

            if (cmw.currentAmmo < 1 || !cmw.canMultiTrackSingleTargets) lockChargeBar.transform.localScale = new Vector3(lockChargeBar.transform.localScale.x, 0, 1);
            else
            {
                lockChargeBar.transform.localScale = new Vector3(lockChargeBar.transform.localScale.x, cmw.currentLockTime / cmw.requiredLockTime, 1);
                lockChargeBar.color = Color.yellow;
            }
        }
        else if (cmw is PlayerSwarmMissileLauncher)
        {
            if (cmw.isLaunching || cmw.hasLockedOn) mouseReticle.color = Color.yellow;
            else if (cmw.hasMouseOverTarget) mouseReticle.color = Color.red;
            else mouseReticle.color = Color.green;
            lockChargeBar.transform.localScale = new Vector3(lockChargeBar.transform.localScale.x, 0, 1);
        }
        secondaryWeaponName.text = cmw.name;
        missileAmmoCounter.text = cmw.currentAmmo + " | " + cmw.maxAmmo;
    }
}