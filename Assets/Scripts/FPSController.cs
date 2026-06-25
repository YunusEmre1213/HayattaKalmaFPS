using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5.5f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Momentum & Hava Kontrolü")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float airControlMultiplier = 0.6f;
    private Vector3 currentMoveVelocity;

    [Header("Kayma (Slide) Ayarlarý")]
    [SerializeField] private float initialSlideSpeed = 12f;
    [SerializeField] private float slideFriction = 7f;
    private bool isSliding;
    private Vector3 slideDirection;
    private float currentSlideSpeed;

    [Header("Eðilme Ayarlarý")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Kamera & FOV Ayarlarý")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 85f;
    [SerializeField] private float standingCameraHeight = 1.6f;
    [SerializeField] private float crouchingCameraHeight = 0.8f;
    [SerializeField] private float slidingCameraHeight = 0.4f; 

    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 75f;
    [SerializeField] private float fovTransitionSpeed = 10f;

    [Header("Kamera Sarsýntýsý (Head Bobbing)")]
    [SerializeField] private float walkBobFrequency = 10f;
    [SerializeField] private float walkBobAmplitude = 0.05f;
    [SerializeField] private float runBobFrequency = 14f;
    [SerializeField] private float runBobAmplitude = 0.1f;
    private float bobTimer;

    [Header("Düþüþ Sarsýntýsý (Landing Impact)")]
    [SerializeField] private float maxLandingImpact = -0.4f;
    [SerializeField] private float impactRecoverySpeed = 7f;
    private float currentLandingOffset;
    private bool wasGrounded;
    private float lastFallSpeed;

    [Header("Animasyon")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private PlayerInputActions inputActions;
    private Camera cam;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isWalking;
    private float cameraPitch = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
        cam = playerCamera.GetComponent<Camera>();

        inputActions.Player.Jump.performed += context => Jump();

        inputActions.Player.Crouch.performed += context =>
        {
            isCrouching = true;

            
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();

           
            if (isGrounded && moveInput.magnitude > 0.1f && !isWalking && !isSliding)
            {
                isSliding = true;

              
                Vector3 inputDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
                slideDirection = inputDirection;

                currentSlideSpeed = initialSlideSpeed;
            }
        };

        inputActions.Player.Crouch.canceled += context =>
        {
            isCrouching = false;
            isSliding = false; 
        };

        inputActions.Player.Walk.performed += context => isWalking = true;
        inputActions.Player.Walk.canceled += context => isWalking = false;
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CheckGrounded();
        HandleMouseLook();
        HandleMovement();
        ApplyGravity();
        HandleCameraAndCrouch();
        HandleFOV();
        UpdateAnimations();
    }

    private void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            if (!wasGrounded)
            {
                ApplyLandingImpact();
            }
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            lastFallSpeed = velocity.y;
        }
    }

    private void ApplyLandingImpact()
    {
        float impactIntensity = Mathf.Clamp01(-lastFallSpeed / 15f);
        currentLandingOffset = Mathf.Lerp(0, maxLandingImpact, impactIntensity);
    }

    private void HandleMouseLook()
    {
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
    }

    private void HandleMovement()
    {
        if (isSliding)
        {
            currentSlideSpeed -= slideFriction * Time.deltaTime;

            if (currentSlideSpeed <= crouchSpeed)
            {
                isSliding = false;
            }

            currentMoveVelocity = slideDirection * currentSlideSpeed;
        }
        else
        {
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();

            float targetSpeed = 0f;
            if (moveInput.magnitude > 0)
            {
                targetSpeed = isCrouching ? crouchSpeed : (isWalking ? walkSpeed : runSpeed);
            }

            Vector3 targetDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
            Vector3 targetVelocity = targetDirection * targetSpeed;

            float accRate = (moveInput.magnitude > 0) ? acceleration : deceleration;
            currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, targetVelocity, Time.deltaTime * accRate);
        }

        Vector3 finalVelocity = currentMoveVelocity;
        if (!isGrounded)
        {
            finalVelocity *= airControlMultiplier;
        }

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (isGrounded)
        {
            isSliding = false;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCameraAndCrouch()
    {
        bool shouldCrouch = isCrouching || isSliding;

        float targetHeight = shouldCrouch ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        float targetCenter = shouldCrouch ? crouchHeight / 2f : standingHeight / 2f;
        controller.center = Vector3.Lerp(controller.center, new Vector3(0, targetCenter, 0), Time.deltaTime * crouchTransitionSpeed);

        
        float targetCameraHeight = standingCameraHeight;
        if (isSliding)
        {
            targetCameraHeight = slidingCameraHeight; 
        }
        else if (isCrouching)
        {
        }

        float bobOffset = 0f;

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        float speedMagnitude = moveInput.magnitude;

        if (isGrounded && speedMagnitude > 0.1f && !shouldCrouch)
        {
            float frequency = isWalking ? walkBobFrequency : runBobFrequency;
            float amplitude = isWalking ? walkBobAmplitude : runBobAmplitude;

            bobTimer += Time.deltaTime * frequency;
            bobOffset = Mathf.Sin(bobTimer) * amplitude;
        }
        else
        {
            bobTimer = 0f;
        }

        currentLandingOffset = Mathf.Lerp(currentLandingOffset, 0f, Time.deltaTime * impactRecoverySpeed);

        Vector3 newCameraPos = new Vector3(playerCamera.localPosition.x, targetCameraHeight + bobOffset + currentLandingOffset, playerCamera.localPosition.z);
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, newCameraPos, Time.deltaTime * crouchTransitionSpeed);
    }

    private void HandleFOV()
    {
        float targetFOV = normalFOV;
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f;

        if (isSliding)
        {
            targetFOV = sprintFOV + 5f;
        }
        else if (isGrounded && isMoving && !isWalking && !isCrouching)
        {
            targetFOV = sprintFOV;
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float animSpeed = 0f;
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        if (moveInput.magnitude > 0.1f)
        {
            if (isSliding) animSpeed = 1f;
            else if (isCrouching) animSpeed = 0.5f;
            else if (isWalking) animSpeed = 0.5f;
            else animSpeed = 1f;
        }

        animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsSliding", isSliding);

        if (!isGrounded)
        {
            animator.SetFloat("Speed", 0);
        }
    }
}