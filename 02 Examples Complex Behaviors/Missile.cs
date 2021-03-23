using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 10f, angularSpeed = 30f, maxLifeTime = 3f, damage = 150f, explosionRadius = 20f;
    public Transform target;
    public ParticleSystem missileHitPS, missileStreakPS;
    public LayerMask hitableLayers;
    public bool hasProximityTrigger;
    public TrackingMode currentTrackingMode = TrackingMode.dumbFire;
    public enum TrackingMode { dumbFire, swerveTracking, spiralTracking, defaultTracking, increasingTracking, degradingTracking, silverBulletTracking, destoyerTorpedoTracking }
    float currentLifetime, currentSpeed, currentDistance, randomOffset, dt; int leftOrRightSpin;
    Collider[] hitColliders;
    void Update()
    {
        dt = Time.deltaTime;
        currentLifetime += dt;
        Tracking();

        if (currentLifetime > .1f && Physics.Raycast(transform.position, transform.forward, speed * dt, hitableLayers)) DeathSequence();
        else if (currentLifetime >= maxLifeTime) DeathSequence();

        if (currentTrackingMode == TrackingMode.silverBulletTracking)
        {
            currentSpeed = Mathf.SmoothStep(0, speed * 2f * (currentLifetime / maxLifeTime), (currentLifetime / maxLifeTime));
        }
        else if (currentTrackingMode == TrackingMode.destoyerTorpedoTracking)
        {
            currentSpeed = Mathf.SmoothStep(speed / 10, speed * (currentLifetime / maxLifeTime), (currentLifetime / maxLifeTime));
        }
        else currentSpeed = Mathf.SmoothStep(speed / 2f, speed, currentLifetime);
        transform.position += transform.forward * currentSpeed * dt;
    }

    private void Tracking()
    {
        if (target == null) return;

        currentDistance = Vector3.Distance(transform.position, target.position);

        if (currentLifetime > .1f && currentTrackingMode != TrackingMode.dumbFire && target != null)
        {
            if (currentTrackingMode == TrackingMode.defaultTracking) DefaultTracking();
            else if (currentTrackingMode == TrackingMode.swerveTracking) SwervingTracking();
            else if (currentTrackingMode == TrackingMode.spiralTracking) SpiralTracking();
            else if (currentTrackingMode == TrackingMode.increasingTracking) IncreasingTracking();
            else if (currentTrackingMode == TrackingMode.degradingTracking) DegradingTracking();
            else if (currentTrackingMode == TrackingMode.silverBulletTracking) SilverBulletTracking();
            else if (currentTrackingMode == TrackingMode.destoyerTorpedoTracking) DestroyerTorpedoTracking();
        }
    }

    private void DestroyerTorpedoTracking()
    {
        float modifiedAngularSpeed = angularSpeed - angularSpeed * (currentLifetime / maxLifeTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position, transform.up), modifiedAngularSpeed * dt);
        if (hasProximityTrigger && Vector3.Distance(transform.position, target.position) < explosionRadius) DeathSequence();
    }

    private void SpiralTracking()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position, transform.up), angularSpeed * dt);
        float swerveFactor = Mathf.Sin(currentLifetime * 10f + Time.time) * angularSpeed * 1.3f;
        float swerveFactor2 = Mathf.Cos(currentLifetime * 10f + Time.time) * angularSpeed * 1.3f * leftOrRightSpin;
        if (currentLifetime > .5f && 2 * currentDistance < speed)
        {
            float distanceBasedReduction = (2 * currentDistance) / speed;
            transform.Rotate(swerveFactor * dt * distanceBasedReduction, swerveFactor2 * dt * distanceBasedReduction, 0);
        }
        else transform.Rotate(swerveFactor * dt, (1 - currentLifetime / maxLifeTime) * swerveFactor2 * dt, 0);
        if (hasProximityTrigger && Vector3.Distance(transform.position, target.position) < explosionRadius) DeathSequence();
    }

    private void SwervingTracking()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position, transform.up), angularSpeed * dt);
        float swerveFactor = Mathf.Sin(currentLifetime * 15f + Time.time + randomOffset) * angularSpeed * 2f;
        if (currentLifetime > .5f && 2 * currentDistance < speed)
        {
            float distanceBasedReduction = (2 * currentDistance) / speed;
            transform.Rotate(swerveFactor * dt * distanceBasedReduction, 0, 0);
        }
        else transform.Rotate(swerveFactor * dt, 0, 0);

        if (hasProximityTrigger && Vector3.Distance(transform.position, target.position) < explosionRadius) DeathSequence();
    }

    private void DefaultTracking()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position, transform.up), angularSpeed * dt);
        if (hasProximityTrigger && Vector3.Distance(transform.position, target.position) < explosionRadius) DeathSequence();
    }

    private void DegradingTracking()
    {
        float modifiedAngularSpeed = angularSpeed - angularSpeed * (currentLifetime / maxLifeTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position, transform.up), modifiedAngularSpeed * dt);
        if (hasProximityTrigger && Vector3.Distance(transform.position, target.position) < explosionRadius) DeathSequence();
    }

    private void SilverBulletTracking()
    {
        if (currentLifetime * 2f <= maxLifeTime) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position, transform.up), (currentLifetime * 2f) / maxLifeTime);
        else transform.rotation = Quaternion.LookRotation(target.position - transform.position, transform.up);
        if (hasProximityTrigger && Vector3.Distance(transform.position, target.position) < explosionRadius) DeathSequence();
    }

    private void IncreasingTracking()
    {
        float modifiedAngularSpeed = angularSpeed + angularSpeed * (currentLifetime / maxLifeTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position, transform.up), modifiedAngularSpeed * dt);
        if (hasProximityTrigger && Vector3.Distance(transform.position, target.position) < explosionRadius) DeathSequence();
    }

    void DeathSequence()
    {
        hitColliders = Physics.OverlapSphere(transform.position, explosionRadius * 1.25f, hitableLayers);
        foreach (Collider col in hitColliders)
        {
            //Debug.Log(col.transform.root.gameObject.name);
            if (col.transform.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth)) enemyHealth.Damage(damage);
            else if (col.transform.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth)) playerHealth.Damage(damage);
        }

        Instantiate(missileHitPS, transform.position, transform.rotation);
        missileStreakPS.transform.parent = null;
        Destroy(gameObject);
    }
    private void Start()
    {
        if (currentTrackingMode == TrackingMode.spiralTracking)
        {
            transform.Rotate(0, 0, UnityEngine.Random.Range(0, 360));
            if (UnityEngine.Random.value > .5f) leftOrRightSpin = -1;
            else leftOrRightSpin = 1;
        }
        else if (currentTrackingMode == TrackingMode.swerveTracking) randomOffset = UnityEngine.Random.Range(0, Mathf.PI);
    }
}
