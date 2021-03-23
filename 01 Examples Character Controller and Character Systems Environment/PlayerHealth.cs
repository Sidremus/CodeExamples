using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public ParticleSystem playerHitPS, streakPS;
    public float currentHealth;
    bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }
    public void Damage(float dmg, Vector3 hitLocation)
    {
        currentHealth -= dmg;
        Instantiate(playerHitPS, hitLocation, Quaternion.identity);
        if (currentHealth <= 0 && !isDead) DeathSequence();
        Debug.Log(maxHealth - currentHealth);
    }
    public void Damage(float dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0 && !isDead) DeathSequence();
    }
    void DeathSequence()
    {
        isDead = true;
        if (!streakPS.isPlaying) streakPS.Play();
        Debug.Log("player death");
        //Mission.MissionFailure();
    }
}
