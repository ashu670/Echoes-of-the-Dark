using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController))]
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
    public float sanityTickInterval = 1f;
    public float sanity = 100f;
    public float drainRate = 2f;
    public int lightHit = 0;
    public bool isDead = false;
    public bool isDark;

    public Camera cam;

    float velocityY;
    float rotX;
    float currentSpeed;
    float sanityTimer;
    bool isJumped;


    CharacterController controller;
    Vector3 inputDir;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!isDead)
        {
            MouseLook();

            // Read inputs and prepare desired move vector
            ReadMovementInput(out float x, out float z);
            Vector3 desiredMove = transform.right * x + transform.forward * z;

            // Grounded handling (speed, input dir, noise)
            if (controller.isGrounded)
            {
                HandleGrounded(desiredMove, x, z);
            }

            HandleJump();

            // Choose movement vector depending on jump state (preserve original behavior)
            Vector3 move = isJumped ? inputDir * currentSpeed : desiredMove * currentSpeed;

            // Apply gravity and move controller
            ApplyGravity(ref move);
            controller.Move(move * Time.deltaTime);

            CheckLit();
            HandleSanity();
        }
    }

    void CheckLit()
    {
        isDark = lightHit == 0;
    }

    void HandleSanity()
    {
        sanityTimer -= Time.deltaTime;

        if( sanityTimer <= 0)
        {
            sanityTimer = sanityTickInterval;
            if (isDark)
            {
                sanity -= drainRate;
            }
            else
            {
                sanity += drainRate * 0.5f;
            }
            sanity = Mathf.Clamp(sanity, 0f, 100f);
        }
    }
    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, -90f, 90f);
        cam.transform.localRotation = Quaternion.Euler(rotX, 0f, 0f);
    }

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
        {
            currentSpeed = stealthSpeed;
        }

        inputDir = moveVec.normalized;

        if (currentSpeed == moveSpeed && (x != 0 || z != 0))
        {
            noiseVal = Mathf.Lerp(noiseVal, 0.5f, Time.deltaTime * 2f);
        }
        else if (currentSpeed == runSpeed && (x != 0 || z != 0))
        {
            noiseVal = Mathf.Lerp(noiseVal, 1f, Time.deltaTime * 2f);
        }
        else
        {
            noiseVal = 0;
        }
    }

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
        {
            velocityY = -2f; // Small negative value to keep the player grounded
        }
        velocityY -= gravity * Time.deltaTime;
        move.y = velocityY;
    }

    public void Death()
    {
        isDead = true;
        moveSpeed = runSpeed = stealthSpeed = 0f;
    }
}
