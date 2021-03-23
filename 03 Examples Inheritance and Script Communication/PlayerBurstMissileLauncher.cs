using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBurstMissileLauncher : MissileWeapon
{
    Queue<Transform> lockedTargets = new Queue<Transform>();
    List<Transform> presenterMissiles = new List<Transform>();
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
            if (ms.rightButton.isPressed && !isLaunching) LockTargets();
            else if (ms.rightButton.wasReleasedThisFrame) StartCoroutine(LaunchBurstMissiles());
            else if (!isLaunching) CheckForTargetUnderMouse();
            hasLockedOn = lockedTargets.Count > 0 && !isLaunching;
            if (canMultiTrackSingleTargets) currentLockTime = Mathf.Clamp(currentLockTime - Time.deltaTime, 0, requiredLockTime);
            else currentLockTime = 0;
        }
    }

    private void CheckForTargetUnderMouse()
    {
        hasMouseOverTarget = Physics.Raycast(cam.ScreenPointToRay(ms.position.ReadValue()), out hit, 1000f, missileTargetLM)
            || Physics.SphereCast(cam.transform.position, 3f, cam.ScreenPointToRay(ms.position.ReadValue()).direction, out hit, 1000f, missileTargetLM);
    }

    private void LockTargets()
    {
        if (currentAmmo < 1 || (lockedTargets.Count >= maxConcurrentMissiles && canMultiTrackSingleTargets)) return;
        if (Physics.Raycast(cam.ScreenPointToRay(ms.position.ReadValue()), out hit, 1000f, missileTargetLM))
        {
            hasMouseOverTarget = true;
            if (!lockedTargets.Contains(hit.collider.transform) && currentLockTime == 0) 
            {
                currentLockTime = requiredLockTime;
                lockedTargets.Enqueue(hit.collider.transform); 
            }
            else if (canMultiTrackSingleTargets && currentLockTime == 0)
            {
                currentLockTime = requiredLockTime;
                lockedTargets.Enqueue(hit.collider.transform);
            }

            if (lockedTargets.Count > maxConcurrentMissiles) lockedTargets.Dequeue();
        }
        else if (Physics.SphereCast(cam.transform.position, 3f, cam.ScreenPointToRay(ms.position.ReadValue()).direction, out hit, 1000f, missileTargetLM))
        {
            hasMouseOverTarget = true;
            if (!lockedTargets.Contains(hit.collider.transform) && currentLockTime == 0) 
            { 
                currentLockTime = requiredLockTime;
                lockedTargets.Enqueue(hit.collider.transform); 
            }
            else if (canMultiTrackSingleTargets && currentLockTime == 0)
            {
                currentLockTime = requiredLockTime;
                lockedTargets.Enqueue(hit.collider.transform);
            }

            if (lockedTargets.Count > maxConcurrentMissiles) lockedTargets.Dequeue();
        }
        else hasMouseOverTarget = false;

        if (lockedTargets.Count > presenterMissiles.Count) AddPresenterMissile();
    }
    void AddPresenterMissile()
    {
        currentAmmo--;
        float spacialOffset = .4f, maxAngleOffset = 60;
        currentPresenter = Instantiate(presenterMissilePrefab, transform.position, transform.rotation, transform);
        if (lockedTargets.Count == 1)
        {
            //currentPresenter.localRotation = ;
        }
        else if (lockedTargets.Count % 2 == 1)
        {
            currentPresenter.localPosition += new Vector3(lockedTargets.Count - 1, 0, 0) * spacialOffset;
            currentPresenter.Rotate(0, (maxAngleOffset / maxConcurrentMissiles) * lockedTargets.Count - 1, 0, Space.Self);
        }
        else if (lockedTargets.Count % 2 != 1)
        {
            currentPresenter.localPosition -= new Vector3(lockedTargets.Count, 0, 0) * spacialOffset;
            currentPresenter.Rotate(0, (-maxAngleOffset / maxConcurrentMissiles) * lockedTargets.Count - 1, 0, Space.Self);
        }
        currentPresenter.Rotate(-35f, 0, 0);
        presenterMissiles.Add(currentPresenter);
    }
    private IEnumerator LaunchBurstMissiles()
    {
        isLaunching = true;
        presenterMissiles.Reverse();
        while (lockedTargets.Count > 0)
        {
            currentPresenter = presenterMissiles[0];
            LaunchSwarmMissile(lockedTargets.Dequeue(), currentPresenter.position, currentPresenter.rotation);
            presenterMissiles.RemoveAt(0);
            Destroy(currentPresenter.gameObject);
            yield return new WaitForSeconds(delayBetweenLaunches);
        }
        isLaunching = false;
    }

    private void LaunchSwarmMissile(Transform target, Vector3 launchPosition, Quaternion launchRotation)
    {
        currentMissile = Instantiate(missilePrefab, launchPosition, launchRotation).GetComponent<Missile>();
        currentMissile.target = target;
        currentMissile.currentTrackingMode = currentTrackingMode;
        currentMissile.damage = damage;
    }
}
