using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSystems : MonoBehaviour
{
    public MeshRenderer[] engineSurfaceMeshes;
    public Transform shipMesh;
    public float maxShipMeshRotation = 15f;
    public ParticleSystem warpVFX;
    public Camera followCam;
    public Vector3 camPosNear, camPosFar;
    ShipMovement ship;
    
    private void FixedUpdate()
    {
        OrientShipMesh();
        EngineVFX();
        AdjustCamPos();
    }
    private void AdjustCamPos()
    {
        if(!ship.isDrifting) followCam.transform.localPosition = Vector3.Slerp(camPosNear, camPosFar, ship.throttle);
    }
    private void OrientShipMesh()
    {
        shipMesh.localRotation = Quaternion.Euler(ship.pitch * maxShipMeshRotation, ship.yaw * maxShipMeshRotation, -ship.yaw * maxShipMeshRotation);
    }
    private void EngineVFX()
    {
        foreach (Renderer rend in engineSurfaceMeshes)
        {
            if (ship.isBoosting) rend.material.SetFloat("_EmitLerp", 5f);
            else if (ship.isDrifting) rend.material.SetFloat("_EmitLerp", 0f);
            else rend.material.SetFloat("_EmitLerp", ship.throttle);
        }
    }
    private void Awake()
    {
        ship = FindObjectOfType<ShipMovement>();
    }
}
