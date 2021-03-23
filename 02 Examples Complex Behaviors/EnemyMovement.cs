using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform target;
    public float moveForce = 10f, rotForce = 10f, turnDistance = 150f, timeOfPursuitTillEvasion = 5f, maxTimeOfPursuitEvasion = 5f;
    bool isTurning, isDead, isEvadingCollision, isEvadingPursuit, longPursuit, longTurn;
    float pursuitTimer, longPursuitTimer, distance, turnTimer;
    Rigidbody rb; Collider thisShipCollider;
    Vector3 direction, rotAmount, averagePositionOfCollidersInEvadeCone, deathSpin;
    List<Collider> collidersInEvadeCone = new List<Collider>();

    private void Awake()
    {
        if (target == null) target = FindObjectOfType<ShipMovement>().transform;
        rb = GetComponent<Rigidbody>();
        thisShipCollider = GetComponentInChildren<MeshCollider>();
        deathSpin = new Vector3(
            UnityEngine.Random.Range(-rotForce * .3f, rotForce * .3f),
            UnityEngine.Random.Range(-rotForce * .3f, rotForce * .3f),
            UnityEngine.Random.Range(-rotForce * 2f, rotForce * 2f));

    }
    private void FixedUpdate()
    {
        direction = (target.position - rb.position).normalized;
        distance = Vector3.Distance(transform.position, target.position);

        if (!isTurning && !isEvadingCollision && Vector3.Angle(rb.velocity, target.forward) < 90 && distance < 2 * turnDistance)
            Mathf.Clamp(pursuitTimer += Time.fixedDeltaTime, 0f, timeOfPursuitTillEvasion + maxTimeOfPursuitEvasion);
        else pursuitTimer = Mathf.Clamp(pursuitTimer -= Time.fixedDeltaTime, 0f, timeOfPursuitTillEvasion + maxTimeOfPursuitEvasion);

        if (isEvadingPursuit) longPursuitTimer += Time.fixedDeltaTime;
        if (longPursuitTimer > maxTimeOfPursuitEvasion) { longPursuit = !longPursuit; longPursuitTimer = 0f; }

        if (isTurning) isTurning = Vector3.Angle(transform.forward, direction) > 1f;
        else isTurning = distance > turnDistance;

        if (isTurning) { turnTimer += Time.fixedDeltaTime; if (turnTimer > 8f) { longTurn = !longTurn; turnTimer = 0f; } }
        else { turnTimer = 0f; longTurn = false; }

        isEvadingCollision = collidersInEvadeCone.Count > 0;
        isEvadingPursuit = pursuitTimer >= timeOfPursuitTillEvasion;

        if (!isDead) //steering
        {
            if (isEvadingCollision)
            {
                isTurning = false;

                averagePositionOfCollidersInEvadeCone = Vector3.zero;
                foreach (Collider col in collidersInEvadeCone)
                {
                    if (col == null) collidersInEvadeCone.Remove(col);
                    else averagePositionOfCollidersInEvadeCone += col.transform.position;
                }
                averagePositionOfCollidersInEvadeCone /= collidersInEvadeCone.Count;
                direction = (averagePositionOfCollidersInEvadeCone - rb.position).normalized;
                rotAmount = Vector3.Cross(transform.forward, direction);

                if (Vector3.Angle(transform.forward, direction) < 3f) rotAmount = Vector3.Cross(transform.right, direction); //correction in case we're headed straight for the target
                else rotAmount = Vector3.Cross(transform.forward, direction);

                rb.AddTorque(-rotAmount * rotForce * 3f);

                rb.AddForce(transform.forward * moveForce * .75f);
            }
            else if (isEvadingPursuit)
            {
                isTurning = false;
                if (!longPursuit)
                {
                    rotAmount = Vector3.Cross(transform.up, direction);
                    rb.AddTorque(rotAmount * rotForce * UnityEngine.Random.Range(1, 2));
                }
                else
                {
                    rotAmount = Vector3.Cross(transform.up, direction);
                    rb.AddTorque(rotAmount * rotForce * UnityEngine.Random.Range(-2, -1));
                }
                rb.AddForce(transform.forward * moveForce * 1.25f);
            }
            else if (isTurning)
            {
                if (longTurn) rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(direction, direction), 2f * rotForce * Time.deltaTime));
                else rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(direction, direction), rotForce * Time.deltaTime));
                rb.AddForce(transform.forward * moveForce * 1.25f);
            }
            else rb.AddForce(transform.forward * moveForce);
        }

        else { rb.AddForce(transform.forward * moveForce); rb.AddTorque(deathSpin * 10); }
    }

    public void DeathSequence()
    {
        isDead = true;
        rb.detectCollisions = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != thisShipCollider && !other.CompareTag("EnemyBullets")) collidersInEvadeCone.Add(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other != thisShipCollider && !other.CompareTag("EnemyBullets")) collidersInEvadeCone.Remove(other);
    }
}
