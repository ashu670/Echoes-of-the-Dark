using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float runSpeed = 5f;
    public float stealthSpeed = 1f;
    public float sensitivity = 100f;
    public float gravity = 9.81f;
    public float jumpHeight = 1.5f;
    [Range(0f, 1f)]
    public float noiseVal;

    public Camera cam;

    float velocityY;
    float rotX;
    float currentSpeed;
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
        // Mouse look
        MouseLook();

        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        if (controller.isGrounded)
        {
            isJumped = false;
            currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
            if(Input.GetKey(KeyCode.LeftControl))
            {
                currentSpeed = stealthSpeed;
            }
            inputDir = move.normalized;

            if(currentSpeed == moveSpeed && (x != 0 || z != 0))
            {
                noiseVal = Mathf.Lerp(noiseVal, 0.5f, Time.deltaTime * 2f);
            }
            else if(currentSpeed == runSpeed && (x != 0 || z != 0))
            {
                noiseVal = Mathf.Lerp(noiseVal, 1f, Time.deltaTime * 2f);
            }
            else
            {
                noiseVal = 0;
            }
        }
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocityY = Mathf.Sqrt(jumpHeight * 2f * gravity);
            isJumped = true;
        }

        move = isJumped ? inputDir * currentSpeed : move * currentSpeed;

        // Apply gravity
        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = -2f; // Small negative value to keep the player grounded
        }
        velocityY -= gravity * Time.deltaTime;
        move.y = velocityY;

        controller.Move(move * Time.deltaTime);
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
}
