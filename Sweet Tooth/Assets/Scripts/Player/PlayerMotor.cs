using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    [Header("Scripts")]
    private CharacterController controller;
    public PlayerStats playerStats;

    [Header("Objects")]
    public new Camera camera;
    public GameObject tool;

    [Header("Booleans")]
    private bool isGrounded;
    public bool lerpCrouch;
    public bool crouching;
    public bool sprinting;
    private bool sprintKeyDown;

    [Header("Speed")]    
    public Vector3 playerVelocity; // current velocity of the player
    public float speed; // current actual speed
    public float targetSpeed; // target speed
    public float sprintSpeedFactor; // sprinting speed factor
    public float crouchSpeedFactor; // crouching speed factor

    [Header("FOV")]
    private float fovVelocity;
    public float sprintFOVFactor; // sprinting FOV factor
    private float currentFOV; // current FOV
    public float walkFOV; // walking FOV
    public float fovDampTime; // time to reach target FOV

    [Header("Height")]
    public float standHeight; // standing height
    public float crouchHeight; // crouch height
    public float walkJumpHeight; // target jump height
    public float crouchJumpHeightFactor;
    public float jumpHeight; // jump height
    public float targetJumpHeight;
    public float sprintJumpHeightFactor;
    public float crouchHeightSmoothTime;
    public Vector3 controllerTop;


    


    [Header("Other")]
    public float gravity; // gravity acceleration
    private Vector3 targetScale = Vector3.one; // target player scale
    public float acceleration; // acceleration towards target speed, for smoothing/lerping
    public Transform[] children;

    // called before the first frame update
    void Start()
    { 
        // get controller component
        controller = GetComponent<CharacterController>();

        children = new Transform[transform.childCount];
        int i = 0;  
        foreach (Transform T in transform)
        {
            children[i++] = T;
        }
    }

    // called once per frame
    void Update()
    {
        // changes speed and jump height based on state
        targetSpeed = playerStats.walkSpeed;
        targetJumpHeight = walkJumpHeight;
        if (crouching)
        {
            targetSpeed = playerStats.walkSpeed * crouchSpeedFactor;
            targetJumpHeight = walkJumpHeight * crouchJumpHeightFactor;
        }
        if (sprinting)
        {
            targetSpeed = playerStats.walkSpeed * sprintSpeedFactor;
            targetJumpHeight = walkJumpHeight * sprintJumpHeightFactor;
        }
        speed = targetSpeed;
        // speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.deltaTime);
        jumpHeight = targetJumpHeight;
        isGrounded = controller.isGrounded;
        if (lerpCrouch)
        {
            // adjust camera height based on crouch state
            float targetHeight = crouching ? crouchHeight-1.4f : standHeight-1.4f; // sets the target height
            
            // lerps camera height
            Vector3 cameraPosition = camera.transform.localPosition;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetHeight, Time.deltaTime * crouchHeightSmoothTime);
            camera.transform.localPosition = cameraPosition;

            // sets target height and lerps controller height and center
            targetHeight = crouching ? crouchHeight : standHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchHeightSmoothTime);
            controller.center = Vector3.down * (standHeight - controller.height) / 2.0f;

            if (Mathf.Abs(controller.height - targetHeight) < 0.001f)
            {
                controller.height = targetHeight;
                lerpCrouch = false;
            }
        }

        GameData.playerLocation = transform.position; // saves player location to GameData
    }

    void OnDrawGizmos()
    {
        if (controller != null)
        {
            // Draw a red wire sphere at the top of the character controller
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(controller.transform.position + controllerTop, controller.radius * 0.9f);
        }
    }

    // recieve inputs from InputManager.cs and apply them to character controller
    public void ProcessMove(Vector2 input)
    {
        // move in the direction specified by wasd
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);

        // check for objects above player and zero out vertical velocity if necessary
        controllerTop = new Vector3(0, controller.height - 1.3f, 0); // offset so the sphere doesn't stick out much
        Collider[] colliders = Physics.OverlapSphere(controller.transform.position + controllerTop, controller.radius * 0.9f);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                playerVelocity.y = Mathf.Min(0, playerVelocity.y);
                break; // exit the loop if collision is detected
            }
        }

        // apply gravity to the player
        playerVelocity.y += gravity * Time.deltaTime;
        if(isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -0.5f; // slight gravity when on ground to stick to ground
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Jump()
    {
        if(controller.isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3f * gravity);
        }
    }


    public void Crouch(bool key)
    {  
        // stop sprinting if crouch
        if (sprinting)
        {
            Sprint(false);
            sprintKeyDown = true;
        }
        crouching = !crouching;
        if(!crouching)
        {

            if (sprintKeyDown)
            {
                Sprint(true);
            }
        }
        lerpCrouch = true;
    }

    public void Sprint(bool key)
    {
        sprintKeyDown = key;
        // dont sprint if crouching
        if (!crouching && key)
        {
            sprinting = true;
        }
        else
        {
            sprinting = false;
        }
        // smoothly change camera FOV
        currentFOV = Camera.main.fieldOfView;
    }
    
    void LateUpdate()
    {
        float targetFOV = walkFOV;
        if (sprinting)
        {
            targetFOV = walkFOV * sprintFOVFactor;
        }
        // smoothly interpolate FOV towards target FOV
        currentFOV = Mathf.SmoothDamp(currentFOV, targetFOV, ref fovVelocity, fovDampTime);
        Camera.main.fieldOfView = currentFOV;
    }
}