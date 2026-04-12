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
    public bool isDead;
    public bool isDark;

    // context
    public bool isEnteringRoom;
    public bool turnEventActive;
    public float turnDirection; // -1 left, +1 right

    public Camera cam;

    CharacterController controller;
    AudioSource walk;

    Vector3 inputDir;

    float velocityY;
    float rotX;
    float currentSpeed;
    float sanityTimer;

    float extraDrain = 0f;
    float extraDrainTimer = 0f;

    float lastYaw;
    float accumulatedTurn;

    bool isJumped;
    float targetVolume = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        walk = GetComponent<AudioSource>();

        lastYaw = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (isDead) return;

        MouseLook();
        DetectTurn();

        ReadMovementInput(out float x, out float z);
        Vector3 desiredMove = transform.right * x + transform.forward * z;

        if (controller.isGrounded)
            HandleGrounded(desiredMove, x, z);

        HandleJump();

        Vector3 move = isJumped ? inputDir * currentSpeed : desiredMove * currentSpeed;

        ApplyGravity(ref move);
        controller.Move(move * Time.deltaTime);

        CheckLit();
        HandleSanity();
        HandleExtraDrainTimer();
        HandleWalkAudio();
    }

    // ---------------- TURN DETECTION ----------------
    void DetectTurn()
    {
        float currentYaw = transform.eulerAngles.y;
        float delta = Mathf.DeltaAngle(lastYaw, currentYaw);

        accumulatedTurn += delta;

        // trigger earlier for better anticipation
        if (Mathf.Abs(accumulatedTurn) > 45f)
        {
            turnEventActive = true;
            turnDirection = Mathf.Sign(accumulatedTurn);

            Invoke(nameof(ResetTurn), 0.35f);
            accumulatedTurn = 0f;
        }

        lastYaw = currentYaw;
    }

    void ResetTurn()
    {
        turnEventActive = false;
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
                extraDrain = 0f;
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

    // ---------------- MOVEMENT ----------------
    void ReadMovementInput(out float x, out float z)
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
    }

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
            targetVolume = 0f;
        }
        else
        {
            noiseVal = 0;
            targetVolume = 0f;
        }
    }

    // temp system (replace later with step-based)
    void HandleWalkAudio()
    {
        if (walk == null) return;

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

    void ApplyGravity(ref Vector3 move)
    {
        if (controller.isGrounded && velocityY < 0)
            velocityY = -2f;

        velocityY -= gravity * Time.deltaTime;
        move.y = velocityY;
    }

    // ---------------- ROOM DETECTION ----------------
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            isEnteringRoom = true;
            Invoke(nameof(ResetRoom), 0.5f);
        }
    }

    void ResetRoom()
    {
        isEnteringRoom = false;
    }

    // ---------------- DEATH ----------------
    public void Death()
    {
        isDead = true;
        moveSpeed = runSpeed = stealthSpeed = 0f;
    }
}