using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMissileLauncher : MissileWeapon
{
    private void Awake()
    {
        ms = Mouse.current;
        cam = Camera.main;
        currentAmmo = maxAmmo;
        if (requiredLockTime == 0) requiredLockTime = 1f;//prevents division by 0 in the UI handler
    }
    private void Update()
    {
        if (isActive)
        {
            if (currentAmmo > 0)
            {
                LockTarget();
                if (ms.rightButton.wasPressedThisFrame) LaunchMissile(lockTarget);
            }
            else { hasMouseOverTarget = hasLockedOn = false; }
        }
    }

    private void LockTarget()
    {
        if (Physics.Raycast(cam.ScreenPointToRay(ms.position.ReadValue()), out hit, 1000f, missileTargetLM))
        {
            hasMouseOverTarget = true;
            if (lockTarget == null) lockTarget = hit.collider.transform;
            else if (hit.collider.transform == lockTarget)
            {
                if (!hasLockedOn)
                {
                    currentLockTime = Mathf.Clamp(currentLockTime + Time.deltaTime, 0, requiredLockTime);
                    hasLockedOn = currentLockTime >= requiredLockTime;
                }
                else currentLockTime = Mathf.Clamp(currentLockTime + Time.deltaTime, 0, requiredLockTime);
            }
            else
            {
                lockTarget = hit.collider.transform;
                currentLockTime = 0;
                hasLockedOn = false;
            }
        }
        else if (Physics.SphereCast(cam.transform.position, 3f, cam.ScreenPointToRay(ms.position.ReadValue()).direction, out hit, 1000f, missileTargetLM))
        {
            hasMouseOverTarget = true;
            if (lockTarget == null) lockTarget = hit.collider.transform;
            else if (hit.collider.transform == lockTarget)
            {
                if (!hasLockedOn)
                {
                    currentLockTime = Mathf.Clamp(currentLockTime + Time.deltaTime, 0, requiredLockTime);
                    hasLockedOn = currentLockTime >= requiredLockTime;
                }
                else currentLockTime = Mathf.Clamp(currentLockTime + Time.deltaTime, 0, requiredLockTime);
            }
            else
            {
                lockTarget = hit.collider.transform;
                currentLockTime = 0;
                hasLockedOn = false;
            }
        }
        else if (hasLockedOn) { currentLockTime -= Time.deltaTime; hasLockedOn = currentLockTime + sustainedLockTimeWhenOffTarget >= requiredLockTime; hasMouseOverTarget = false; }
        else { currentLockTime = Mathf.Clamp(currentLockTime - Time.deltaTime, 0, requiredLockTime); hasMouseOverTarget = false; }
    }

    private void LaunchMissile(Transform target)
    {
        if (hasLockedOn)
        {
            currentMissile = Instantiate(missilePrefab, transform.position, transform.rotation).GetComponent<Missile>();
            currentMissile.target = target;
            currentMissile.hasProximityTrigger = hasProximityTrigger;
            currentMissile.currentTrackingMode = currentTrackingMode;
            currentMissile.damage = damage;
            currentAmmo--;
        }
        else if (canDumbFire)
        {
            currentMissile = Instantiate(missilePrefab, transform.position, transform.rotation).GetComponent<Missile>();
            currentMissile.currentTrackingMode = Missile.TrackingMode.dumbFire;
            currentMissile.damage = damage;
            currentAmmo--;
        }
    }
}
