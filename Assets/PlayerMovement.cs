using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    float speed;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float sideMoveMultipliter;
    public Transform groundCheck;
    float groundRadius;
    public float standingGroundRadius = 0.14f;
    public float crouchingGroundRadius;
    public LayerMask ground;
    CharacterController controller;
    public Transform orientation;
    public float jumpCooldown = 1.0f;
    float jumpCooldownTimer = 0.0f;
    public float jumpForce = 5.0f;
    bool canJump = false;
    float yVel;
    public float gravity = 2.84f;
    float startHeight;
    public float crouchSlideHeight;
    Transform groundCheckPos;
    public Transform slideCrouchGroundCheckPos;
    public float speedBoostSlide;
    public float speedSlideAdd;
    bool isSliding = false;
    public float maxSlideSpeed;
    public float slidingFriction;
    public float jumpSlideBoost;
    public float overSpeedLimitFriction;
    public float overSpeedLimitFrictionCrouched;
    public float minimumSlideSpeed;
    bool requestingUnCrouch = false;
    public float underObjectCrouchedCheckLength;
    bool underObjectCrouched = false;
    public Transform underObjectCheck;
    public float underObjectCheckLength;
    bool underObject = false;
    string moveMode = "walking";

    bool isGrounded;
    // Start is called before the first frame update
    void Start()
    {
        speed = walkSpeed;
        controller = GetComponent<CharacterController>();
        groundCheckPos = groundCheck;
        startHeight = controller.height;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundRadius, ground);
        if(moveMode == "crouching")
        {
            groundRadius = crouchingGroundRadius;
            underObjectCrouched = Physics.Raycast(this.transform.position, Vector3.up, controller.height + underObjectCrouchedCheckLength, ground);
        }
        else
        {
            groundRadius = standingGroundRadius;
            underObjectCrouched = false;
        }
        underObject = Physics.Raycast(underObjectCheck.position, Vector3.up, underObjectCheckLength, ground);
        Physics.Raycast(groundCheck.position, Vector3.down, out var hitInfo, groundRadius);

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        CrouchHandler();
        SprintHandler();

        // Moving the character
        if (!isSliding)
        {
            controller.Move(speed * Time.deltaTime * vertical * orientation.forward + horizontal * sideMoveMultipliter * speed * Time.deltaTime * orientation.right + Time.deltaTime * yVel * Vector3.up);
        }
        if (moveMode == "sprinting" && speed >= sprintSpeed && isGrounded)
        {
            speed -= overSpeedLimitFriction * Time.deltaTime;
            speed = Mathf.Clamp(speed, sprintSpeed, speed);
        }
        else if (moveMode == "walking" && speed >= crouchSpeed && isGrounded)
        {
            speed -= overSpeedLimitFriction * Time.deltaTime;
            speed = Mathf.Clamp(speed, walkSpeed, speed);
        }
        else if (moveMode == "crouching" && speed >= crouchSpeed && !isSliding && isGrounded)
        {
            speed -= overSpeedLimitFrictionCrouched * Time.deltaTime;
            speed = Mathf.Clamp(speed, crouchSpeed, speed);
        }

        if(moveMode == "crouching" && yVel > 0 && underObjectCrouched)
        {
            yVel = 0;
        }
        else if(moveMode == "walking" && yVel > 0 && underObject || moveMode == "sprinting" && yVel > 0 && underObject)
        {
            yVel = 0;
        }

        JumpHandler();
    }

    void SprintHandler()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && moveMode != "crouching")
        {
            moveMode = "sprinting";
            if (isGrounded)
            {
                speed = sprintSpeed;
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && moveMode == "sprinting")
        {
            moveMode = "walking";
            if (isGrounded)
            {
                speed = walkSpeed;
            }
        }
    }

    void CrouchHandler()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            requestingUnCrouch = false;
            if (!Input.GetKey(KeyCode.LeftShift) || Input.GetAxis("Vertical") == 0 || controller.velocity.sqrMagnitude < minimumSlideSpeed)
            {
                controller.height = crouchSlideHeight;
                groundCheck = slideCrouchGroundCheckPos;
                moveMode = "crouching";
                speed = crouchSpeed;
            }
            else
            {
                moveMode = "crouching";
                controller.height = crouchSlideHeight;
                groundCheck = slideCrouchGroundCheckPos;
                controller.Move(orientation.forward * (speedBoostSlide * Time.deltaTime));
                isSliding = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSliding = false;
        }
        if (isSliding == true)
        {
            if (controller.velocity.y < 0)
            {
                controller.Move(orientation.forward * speed * Time.deltaTime + Vector3.up * yVel * Time.deltaTime);
                speed += speedSlideAdd;
                speed = Mathf.Clamp(speed, Mathf.NegativeInfinity, maxSlideSpeed);
            }
            else
            {
                controller.Move(orientation.forward * speed * Time.deltaTime + orientation.right * speed * sideMoveMultipliter * Input.GetAxis("Horizontal") * Time.deltaTime + Vector3.up * yVel * Time.deltaTime);
                speed -= slidingFriction * Time.deltaTime;
                speed = Mathf.Clamp(speed, 0, Mathf.Infinity);
                if (speed <= 0)
                {
                    isSliding = false;
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        requestingUnCrouch = false;
                        moveMode = "crouching";
                        speed = crouchSpeed;
                    }
                    else
                    {
                        requestingUnCrouch = true;
                    }
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            requestingUnCrouch = true;
            isSliding = false;
        }
        if (requestingUnCrouch)
        {
            if (!underObjectCrouched && controller.velocity.y == 0)
            {
                isSliding = false;
                requestingUnCrouch = false;
                groundCheck = groundCheckPos;
                controller.height = startHeight;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    moveMode = "sprinting";
                    if (isGrounded)
                    {
                        speed = sprintSpeed;
                    }
                }
                else
                {
                    moveMode = "walking";
                    if (isGrounded)
                    {
                        speed = walkSpeed;
                    }
                }
            }
        }
    }

    void JumpHandler()
    {
        if (isGrounded && jumpCooldownTimer >= jumpCooldown)
        {
            yVel = 0;
            canJump = true;
        }
        else
        {
            canJump = false;
        }
        if(jumpCooldownTimer <= jumpCooldown)
        {
            jumpCooldownTimer += Time.deltaTime;
            if(jumpCooldownTimer > jumpCooldown)
            {
                canJump = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            jumpCooldownTimer = 0.0f;
            yVel = jumpForce;
            if (isSliding && !underObjectCrouched)
            {
                speed += (controller.velocity.sqrMagnitude / 100) * jumpSlideBoost * Time.deltaTime;
                speed = Mathf.Clamp(speed, 0.0f, maxSlideSpeed + jumpSlideBoost);
                isSliding = false;
                groundCheck = groundCheckPos;
                controller.height = startHeight;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    moveMode = "sprinting";
                }
                else
                {
                    moveMode = "walking";
                }
            }
        }
        else
        {
            if (!isGrounded)
            {
                yVel -= gravity * Time.deltaTime;
            }
        }
    }
}