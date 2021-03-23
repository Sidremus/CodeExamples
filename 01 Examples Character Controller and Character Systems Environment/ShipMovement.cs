using UnityEngine;
using UnityEngine.InputSystem;

public class ShipMovement : MonoBehaviour
{
    public Camera screenCam;

    public Vector3 thrust = new Vector3(75f, 75f, 250.0f);
    public float throttleGainOverTime = 0.5f;
    public Vector3 torque = new Vector3(50f, 50f, 30f);
    [Range(0, 1)] public float yawToRoll = 0;

    [HideInInspector] public float pitch, yaw, roll, strafe, lift, throttle;
    [HideInInspector] public bool isBoosting, isDrifting;
    Keyboard kb; Mouse ms;
    Rigidbody rb;
    float forceMultiplier = 100.0f;
    Vector3 appliedThrust, appliedTorque, driftVel;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        kb = Keyboard.current;
        ms = Mouse.current;
    }

    private void Update()
    {
        ReadControls();
    }

    private void ReadControls()
    {
        //Strafe
        if (kb.aKey.isPressed && !kb.dKey.isPressed) strafe = -1;
        else if (!kb.aKey.isPressed && kb.dKey.isPressed) strafe = 1;
        else strafe = 0;

        //Lift
        if (kb.spaceKey.isPressed && !kb.cKey.isPressed) lift = 1;
        else if (!kb.spaceKey.isPressed && kb.cKey.isPressed) lift = -1;
        else lift = 0;

        //Roll
        if (kb.qKey.isPressed && !kb.eKey.isPressed) roll = 1;
        else if (!kb.qKey.isPressed && kb.eKey.isPressed) roll = -1;
        else roll = -yaw * yawToRoll;

        //Throttle
        float target = throttle;
        if (kb.wKey.isPressed && !kb.sKey.isPressed && !isDrifting) { target = 1.0f; }
        else if (!kb.wKey.isPressed && kb.sKey.isPressed && !isDrifting) { target = 0.0f; }
        throttle = Mathf.MoveTowards(throttle, target, Time.deltaTime * throttleGainOverTime);

        //Mouse Pitch & Yaw
        Vector2 mousePos = (screenCam.ScreenToViewportPoint(ms.position.ReadValue()) - new Vector3(.5f, .5f, 0)) * 2f;
        pitch = -Mathf.Clamp(mousePos.y, -1.0f, 1.0f);
        yaw = Mathf.Clamp(mousePos.x, -1.0f, 1.0f);

        //Drifting
        if (kb.leftCtrlKey.wasPressedThisFrame) { driftVel = rb.velocity; isDrifting = true; }
        else if (kb.leftCtrlKey.wasReleasedThisFrame) { isDrifting = false; }
    }

    void FixedUpdate()
    {
        ApplyControls();
    }

    private void ApplyControls()
    {
        appliedThrust = new Vector3(strafe * thrust.x, lift * thrust.y, throttle * thrust.z);
        appliedTorque = new Vector3(pitch * torque.x, yaw * torque.y, roll * torque.z);

        if (!isDrifting) rb.AddRelativeForce(appliedThrust * forceMultiplier, ForceMode.Force);
        else rb.velocity = driftVel;
        rb.AddRelativeTorque(appliedTorque * forceMultiplier, ForceMode.Force);
    }
}
