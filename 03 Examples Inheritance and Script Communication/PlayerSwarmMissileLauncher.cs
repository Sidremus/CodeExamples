using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwarmMissileLauncher : MissileWeapon
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
            else if (ms.rightButton.wasReleasedThisFrame) StartCoroutine(LaunchSwarmMissiles());
            else if (!isLaunching) CheckForTargetUnderMouse();
            hasLockedOn = lockedTargets.Count > 0;
        }
    }

    private void CheckForTargetUnderMouse()
    {
        if (currentAmmo > 0)
            hasMouseOverTarget = Physics.Raycast(cam.ScreenPointToRay(ms.position.ReadValue()), out hit, 1000f, missileTargetLM)
            || Physics.SphereCast(cam.transform.position, 3f, cam.ScreenPointToRay(ms.position.ReadValue()).direction, out hit, 1000f, missileTargetLM);
        else hasMouseOverTarget = hasLockedOn = false;
    }

    private void LockTargets()
    {
        if (currentAmmo < 1) return;
        if (!isPresenting) for (int i = 1; i <= maxConcurrentMissiles && currentAmmo -i + 1 > 0; i++) { AddPresenterMissile(i); }

        isPresenting = true;

        if (Physics.Raycast(cam.ScreenPointToRay(ms.position.ReadValue()), out hit, 1000f, missileTargetLM))
        {
            hasMouseOverTarget = true;
            if (!lockedTargets.Contains(hit.collider.transform) && lockedTargets.Count < maxConcurrentMissiles) { lockedTargets.Enqueue(hit.collider.transform); }
        }
        else if (Physics.SphereCast(cam.transform.position, 3f, cam.ScreenPointToRay(ms.position.ReadValue()).direction, out hit, 1000f, missileTargetLM))
        {
            hasMouseOverTarget = true;
            if (!lockedTargets.Contains(hit.collider.transform) && lockedTargets.Count < maxConcurrentMissiles) { lockedTargets.Enqueue(hit.collider.transform); }
        }
        else hasMouseOverTarget = false;

        //if(lockedTargets.Count > presenterMissiles.Count) AddPresenterMissile();
    }
    void AddPresenterMissile(int i)
    {
        float spacialOffset = .4f, maxAngleOffset = 60;
        currentPresenter = Instantiate(presenterMissilePrefab, transform.position, transform.rotation, transform);
        if (i == 1)
        {
            //currentPresenter.localRotation = ;
        }
        else if (i % 2 == 1)
        {
            currentPresenter.localPosition += new Vector3(i - 1, 0, 0) * spacialOffset;
            currentPresenter.Rotate(0, (maxAngleOffset / maxConcurrentMissiles) * i -1, 0, Space.Self);
        }
        else if (i % 2 != 1)
        {
            currentPresenter.localPosition -= new Vector3(i, 0, 0) * spacialOffset;
            currentPresenter.Rotate(0, (-maxAngleOffset / maxConcurrentMissiles) * i -1, 0, Space.Self);
        }
        currentPresenter.Rotate(-35f, 0, 0);
        presenterMissiles.Add(currentPresenter);
    }
    private IEnumerator LaunchSwarmMissiles()
    {
        isLaunching = true;
        if(lockedTargets.Count == 0)
        {
            foreach (Transform presenter in presenterMissiles) { Destroy(presenter.gameObject); }
            presenterMissiles.Clear();
        }
        else
        {
            //presenterMissiles.Reverse();
            while (presenterMissiles.Count > 0)
            {
                currentPresenter = presenterMissiles[0];
                LaunchSwarmMissile(lockedTargets.Peek(), currentPresenter.position, currentPresenter.rotation);
                lockedTargets.Enqueue(lockedTargets.Dequeue());
                presenterMissiles.RemoveAt(0);
                Destroy(currentPresenter.gameObject);
                yield return new WaitForSeconds(delayBetweenLaunches);
            }
            lockedTargets.Clear();
        }
        isLaunching = false;
        isPresenting = false;
    }

    private void LaunchSwarmMissile(Transform target, Vector3 launchPosition, Quaternion launchRotation)
    {
        currentMissile = Instantiate(missilePrefab, launchPosition, launchRotation).GetComponent<Missile>();
        currentMissile.target = target;
        currentMissile.currentTrackingMode = currentTrackingMode;
        currentMissile.damage = damage;
        currentAmmo--;
    }
}
