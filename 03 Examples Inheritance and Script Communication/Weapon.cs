using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    [HideInInspector] public bool isActive, hasMouseOverTarget;
    public void SetActive(bool b)
    {
        isActive = b;
    }
}
