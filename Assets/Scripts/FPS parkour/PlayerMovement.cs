using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    float playerHeight = 2f;

    //[Header("Sounds")]
    //public AudioClip walkSound;
    //public AudioClip sprintSound;
    //public AudioClip jumpSoundж
    public AudioSource audioSource;


    [SerializeField] Transform orientation;
    [SerializeField] private Camera cam;
    public Animator camAnimator;

    public Climbing climbingScript;
    public WallRun wallRunScript;
    public LedgeGrabbing grabbingScript;

    [Header("Movement")]
    public float moveSpeed;
    public float climbSpeed;
    public float gravitation = 2f;
    public float fallAcceleration = 1f;
    public float airMultiplier = 0.4f;
    float movementMultiplier = 10f;
    public float crouchYscale = 0.5f;
    public float startYScale;
    public float sprintfov;
    public float walkfov;
    bool isCrouching = false;

    [Header("Sprinting")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 6f;
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float acceleration = 10f;

    [Header("Jumping")]
    public float jumpForce = 5f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    float horizontalMovement;
    float verticalMovement;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;
    public bool isGrounded { get; private set; }

    public Vector3 moveDirection;
    public float maxSlopeAngle;
    public float jumpCooldown = 0.5f;
    bool exitingSlope;
    bool readyToJump;

    public bool freeze;
    public bool unlimited;
    public bool climbing;

    public bool restricted;

    Rigidbody rb;

    RaycastHit slopeHit;

    public MovementState state;

    public enum MovementState
    {
        freeze,
        unlimited,
        climbing,
        walk,
        sprint,
        jump,
        idle,
        air
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        startYScale = transform.localScale.y;
        readyToJump = true;
    }


    void CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, groundDistance))
        {
            // Получаем слой, на который падает луч
            int layerIndex = hit.collider.gameObject.layer;
            string layerName = LayerMask.LayerToName(layerIndex);

            Debug.Log("Bestanden Layer: " + layerName);
        }
        else
        {
            Debug.Log("No ground found.");
        }
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        MyInput();
        ControlDrag();
        Crouching();
        CheckGround();
        WhatIsState();
        StateHandler();


        if (!isGrounded)
        {
            fallAcceleration += Time.deltaTime * gravitation;
        }
        if (isGrounded)
        {
            fallAcceleration = 0f;

            if (wallRunScript.isWallRunning || climbingScript.climbing)
            {
                fallAcceleration = 0f;

            }

        }

        Debug.Log(isGrounded);
    }

        void MyInput()
        {
            horizontalMovement = Input.GetAxisRaw("Horizontal");
            verticalMovement = Input.GetAxisRaw("Vertical");

            moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
            if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
            {
                //PlaySound(sounds[0]);

                readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }

        void Jump()
        {

            exitingSlope = true;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);


        }
        void ResetJump()
        {
            readyToJump = true;

            exitingSlope = false;
        }


        void ControlSpeed()
        {

            if (isGrounded)
            {

                if (OnSlope() && !exitingSlope)
                {
                    if (Input.GetKey(sprintKey))
                    {
                        moveSpeed = sprintSpeed;
                    }
                    else
                    {
                        moveSpeed = walkSpeed;
                    }
                }

                else
                {

                    if (Input.GetKey(sprintKey))
                    {
                        moveSpeed = sprintSpeed;
                    }

                    else if (isCrouching)
                    {
                        moveSpeed = crouchSpeed;
                    }

                    else
                    {
                        moveSpeed = walkSpeed;
                    }
                }
            }

            else
            {
                moveSpeed = sprintSpeed;
            }

            rb.useGravity = !OnSlope();
        }

        void ControlDrag()
        {
            if (isGrounded)
            {
                rb.linearDamping = groundDrag;
            }
            else
            {
                rb.linearDamping = airDrag;
            }
        }

        void FixedUpdate()
        {
            WhatIsState();
            ControlSpeed();
            MovePlayer();
        }

        private void MovePlayer()
        {
            if (restricted) return;

            if (climbingScript.exitingWall)
            {
                return;
            }

            if (isGrounded && !OnSlope())
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
            }
            else if (OnSlope() && !exitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection() * moveSpeed * movementMultiplier, ForceMode.Acceleration);
                if (rb.linearVelocity.y > 0)
                    rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
            }
            else if (!isGrounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier + Vector3.down * fallAcceleration, ForceMode.Acceleration);
            }
        }

        private void Crouching()
        {
            if (Input.GetKeyDown(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYscale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                isCrouching = true;
            }

            else if (Input.GetKeyUp(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                isCrouching = false;
            }

        }

    private bool wasInAir = false;

    private void WhatIsState()
    {
        // Обновляем состояние клавиш движения
        bool allMoveKeyIsUp = !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D);
        bool anyMoveKeyIsPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isGrounded)
        {
            if (wasInAir)
            {
                wasInAir = false;
                // После приземления проверяем скорость
                if (rb.linearVelocity.magnitude <= 0.1f)
                {
                    state = MovementState.idle;
                }
                else
                {
                    state = MovementState.walk;
                }
            }
            else
            {
                // Если все клавиши движения отпущены или скорость почти нулевая
                if (allMoveKeyIsUp || rb.linearVelocity.magnitude <= 0.1f)
                {
                    state = MovementState.idle;
                }
                // Если нажата клавиша бега и скорость выше порога
                else if (Input.GetKey(sprintKey) && rb.linearVelocity.magnitude > 2f)
                {
                    state = MovementState.sprint;
                }
                // Если нажаты клавиши движения, но не бег
                else if (anyMoveKeyIsPressed)
                {
                    state = MovementState.walk;
                }
            }
        }
        else
        {
            // В воздухе
            state = MovementState.air;
            wasInAir = true;
        }

        // Обработка прыжка
        if (Input.GetKeyDown(jumpKey) && readyToJump && isGrounded && !climbing)
        {
            state = MovementState.jump;
        }

        // Дополнительные проверки (стенка, лазание)
        if (wallRunScript.IsWallRunning()) fallAcceleration = 0;
        if (climbingScript.climbing) fallAcceleration = 0;
    }

    private void StateHandler()
    {
        float currentTime = Time.time;

        switch (state)
        {
            case MovementState.sprint:
                camAnimator.SetBool("isWalk", false);
                camAnimator.SetBool("isSprint", true);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintfov, 5f * Time.deltaTime);
                break;

            case MovementState.walk:
                camAnimator.SetBool("isWalk", true);
                camAnimator.SetBool("isSprint", false);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, walkfov, 5f * Time.deltaTime);
                break;

            case MovementState.jump:
                camAnimator.SetBool("isWalk", false);
                camAnimator.SetBool("isSprint", false);
                if (Mathf.Abs(rb.linearVelocity.z) > 0f)
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintfov, 5f * Time.deltaTime);
                break;

            case MovementState.idle:
                camAnimator.SetBool("isWalk", false);
                camAnimator.SetBool("isSprint", false);
                break;

            case MovementState.air:
                camAnimator.SetBool("isWalk", false);
                camAnimator.SetBool("isSprint", false);
                break;
        }
    }

}

