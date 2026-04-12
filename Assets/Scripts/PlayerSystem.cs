using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerSystem : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float runSpeed = 5f;
    public float stealthSpeed = 1f;
    public float sensitivity = 100f;
    public float gravity = 9.81f;
    public float jumpHeight = 1.5f;

    [Range(0f, 1f)]
    public float noiseVal;

    public float sanity = 100f;
    public float sanityTickInterval = 1f;
    public float drainRate = 2f;

    public int lightHit = 0;
    public bool isDead = false;
    public bool isDark;

    public Camera cam;

    float velocityY;
    float rotX;
    float currentSpeed;
    float sanityTimer;

    float extraDrain = 0f;
    float extraDrainTimer = 0f;

    bool isJumped;

    CharacterController controller;
    Vector3 inputDir;

    public AudioSource walk;
    float targetVolume = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        walk = GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (isDead) return;

        MouseLook();

        ReadMovementInput(out float x, out float z);
        Vector3 desiredMove = transform.right * x + transform.forward * z;

        if (controller.isGrounded)
        {
            HandleGrounded(desiredMove, x, z);
        }

        HandleJump();

        Vector3 move = isJumped ? inputDir * currentSpeed : desiredMove * currentSpeed;

        ApplyGravity(ref move);
        controller.Move(move * Time.deltaTime);

        CheckLit();
        HandleSanity();
        HandleExtraDrainTimer();

        HandleWalkAudio(); // 🔥 AUDIO FIX
    }

    // ---------------- LIGHT ----------------
    void CheckLit()
    {
        isDark = lightHit == 0;
        lightHit = Mathf.Max(0, lightHit);
    }

    // ---------------- SANITY ----------------
    void HandleSanity()
    {
        sanityTimer -= Time.deltaTime;

        if (sanityTimer <= 0f)
        {
            sanityTimer = sanityTickInterval;

            float totalDrain = Mathf.Clamp(drainRate + extraDrain, 0f, 2f);

            if (isDark)
                sanity -= totalDrain;
            else
                sanity += drainRate * 0.5f;

            sanity = Mathf.Clamp(sanity, 0f, 100f);
        }
    }

    void HandleExtraDrainTimer()
    {
        if (extraDrainTimer > 0f)
        {
            extraDrainTimer -= Time.deltaTime;

            if (extraDrainTimer <= 0f)
            {
                extraDrain = 0f;
            }
        }
    }

    public void ApplySanityEffect(float amount, float duration)
    {
        if (amount > extraDrain)
        {
            extraDrain = Mathf.Clamp(amount, 0f, 1.5f);
            extraDrainTimer = duration;
        }
    }

    // ---------------- CAMERA ----------------
    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(rotX, 0f, 0f);
    }

    // ---------------- INPUT ----------------
    void ReadMovementInput(out float x, out float z)
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
    }

    // ---------------- MOVEMENT ----------------
    void HandleGrounded(Vector3 moveVec, float x, float z)
    {
        isJumped = false;

        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        if (Input.GetKey(KeyCode.LeftControl))
            currentSpeed = stealthSpeed;

        inputDir = moveVec.normalized;

        bool isMoving = (x != 0 || z != 0);

        if (currentSpeed == moveSpeed && isMoving)
        {
            noiseVal = Mathf.Lerp(noiseVal, 0.5f, Time.deltaTime * 2f);
            targetVolume = 1f;
        }
        else if (currentSpeed == runSpeed && isMoving)
        {
            noiseVal = Mathf.Lerp(noiseVal, 1f, Time.deltaTime * 2f);
            targetVolume = 0f; // later we will add run audio
        }
        else
        {
            noiseVal = 0;
            targetVolume = 0f;
        }
    }

    // ---------------- AUDIO (TEMP SYSTEM) ----------------
    void HandleWalkAudio()
    {
        if (walk == null) return;

        // TEMP AUDIO SYSTEM:
        // We DO NOT play/stop repeatedly.
        // Instead, we keep audio always playing and fade volume.
        // This avoids restart glitches when player taps movement keys.
        // This will be replaced later with proper step-based system.

        if (!walk.isPlaying)
            walk.Play();

        walk.volume = Mathf.Lerp(walk.volume, targetVolume, Time.deltaTime * 5f);
    }

    // ---------------- JUMP ----------------
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocityY = Mathf.Sqrt(jumpHeight * 2f * gravity);
            isJumped = true;
        }
    }

    // ---------------- GRAVITY ----------------
    void ApplyGravity(ref Vector3 move)
    {
        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = -2f;
        }

        velocityY -= gravity * Time.deltaTime;
        move.y = velocityY;
    }

    // ---------------- DEATH ----------------
    public void Death()
    {
        isDead = true;
        moveSpeed = runSpeed = stealthSpeed = 0f;
    }
}