using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float lookSpeed = 2f;
    public float jumpForce = 1f;
    public float gravity = -10f;
    public float fallMultiplier = 10f;

    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public float crouchTransitionSpeed = 5f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private float xRotation = 0f;
    public Transform playerCamera;

    private bool isCrouching = false;
    private bool isInteracting = false;

    private PlayerInteraction playerInteraction;

    public void SetInteractionState(bool interacting)
    {
        isInteracting = interacting;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInteraction = GetComponent<PlayerInteraction>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (playerInteraction != null && !playerInteraction.isInteracting)  // Use public isInteracting
        {
            HandleMovement();
            HandleCameraLook();
        }
        HandleCrouch();
    }

    private void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float currentSpeed = isCrouching ? crouchSpeed : Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        if (velocity.y < 0)
        {
            velocity.y += gravity * (fallMultiplier - 1) * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCameraLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.Rotate(Vector3.up * mouseX);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)) isCrouching = true;
        if (Input.GetKeyUp(KeyCode.LeftControl)) isCrouching = false;

        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        playerCamera.localPosition = new Vector3(0f, targetHeight / 2f, 0f);
    }
}
