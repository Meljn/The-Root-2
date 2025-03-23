using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera camera;


    public PlayerMovement pm;
    public WallRun wl;
    public Transform orientation;
    public Transform cam;
    public Rigidbody rb;

    [Header("Ledge Grabbing")]
    public float moveToLedgeSpeed;
    public float maxLedgeGrabDistance;
    public KeyCode grabKey;

    public float minTimeOnLedge;
    private float timeOnLedge;

    public bool holding;

    [Header("Ledge Jumping")]
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpFov = 130f;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpwardForce;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;

    private Transform lastLedge;
    private Transform currLedge;

    private RaycastHit ledgeHit;

    [Header("Exiting")]
    public bool exitingLedge;
    public float exitLedgeTime;
    private float exitLedgeTimer;

    public bool ledgeDetected;

    private void Update()
    {
        LedgeDetection();
        SubStateMachine();
    }

    private void SubStateMachine()
    {
        
        if (holding)
        {
            FreezeRigidbodyOnLedge();

            timeOnLedge += Time.deltaTime;

            if (timeOnLedge > minTimeOnLedge && Input.GetKeyDown(grabKey)) ExitLedgeHold();

            if (Input.GetKeyDown(jumpKey)) LedgeJump();
        }

        
        else if (exitingLedge)
        {
            if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
            else exitingLedge = false;
        }
    }

    private void LedgeDetection()
    {
        ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);

        if (!ledgeDetected) return;

        float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);

        if (ledgeHit.transform == lastLedge) return;

        if (distanceToLedge < maxLedgeGrabDistance && !holding && !pm.isGrounded && Input.GetKeyDown(grabKey)) EnterLedgeHold();
    }

    private void LedgeJump()
    {
        ExitLedgeHold();

        Invoke(nameof(DelayedJumpForce), 0.05f);
    }

    private void DelayedJumpForce()
    {
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    private void EnterLedgeHold()
    {
        holding = true;

        //pm.unlimited = true;
        //pm.restricted = true;

        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
    }

    private void FreezeRigidbodyOnLedge()
    {
        rb.useGravity = false;
        pm.fallAcceleration = 0f;
        pm.enabled = false;
        wl.enabled = false;
       

        Vector3 directionToLedge = currLedge.position - transform.position;
        float distanceToLedge = Vector3.Distance(transform.position, currLedge.position);
        
        if (distanceToLedge > 1f)
        {
            rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);
            
        }

        
        else
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (!pm.freeze) pm.freeze = true;
            if (pm.unlimited) pm.unlimited = false;
        }

        
        if (distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();

        
    }

    private void ExitLedgeHold()
    {
        pm.enabled = true;
        wl.enabled = true;
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        holding = false;
        timeOnLedge = 0f;

        pm.restricted = false;
        pm.freeze = false;

        rb.useGravity = true;

        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }

}
