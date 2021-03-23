using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MissileWeapon : Weapon
{
    public Missile.TrackingMode currentTrackingMode = Missile.TrackingMode.defaultTracking;
    public bool canDumbFire, hasProximityTrigger, canMultiTrackSingleTargets;
    public LayerMask missileTargetLM;
    public Transform missilePrefab, presenterMissilePrefab;
    public int maxAmmo = 50, maxConcurrentMissiles = 5;
    public float requiredLockTime = 2.5f, sustainedLockTimeWhenOffTarget = 2, delayBetweenLaunches = .1f;

    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool hasLockedOn, isLaunching, isPresenting;
    [HideInInspector] public float currentLockTime;
    [HideInInspector] public Transform lockTarget, currentPresenter;
    [HideInInspector] public Mouse ms;
    [HideInInspector] public Camera cam;
    [HideInInspector] public RaycastHit hit;
    [HideInInspector] public Missile currentMissile;
}
