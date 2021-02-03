using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cuby
{
    public class MovementManager : MonoBehaviour
    {
        AudioSource jumpAudioSource;

        const string LEFT = "left";
        const string RIGHT = "right";

        [SerializeField] private int walkSpeed = 7;
         [SerializeField] private int runSpeed = 12;

        [SerializeField] float groundCheckDistance = 0.1f;

        [SerializeField] float wallCheckDistance = 0.2f;
        internal string facingDirection;

        [SerializeField] private bool wallJumpEnabled;

        [SerializeField] private bool doubleJumpEnabled;

        [SerializeField] private float initialJumpVelocity = 15;

        [SerializeField] private float fallMultiplier = 1;

        [SerializeField] private float jumpMultiplier = 2f;

        [SerializeField] private float maxJumpTime = 0.22f;

        [SerializeField] private float minJumpTime = 0.09f;

        [SerializeField] string groundLayerName = null;

        [SerializeField] Transform groundCheckL = null;

        [SerializeField] Transform groundCheckM = null;

        [SerializeField] Transform groundCheckR = null;

        [SerializeField] Transform ceilingCheckL = null;

        [SerializeField] Transform ceilingCheckR = null;

        [SerializeField] private bool showRayCastLines = true;

        private InputManager inputManager;
        private Rigidbody2D rb2d;
        private Transform t;
        private bool wasJumpPressed;
        private bool isTryingToJump;
        private float totalJumpTime;
        private int currentSpeed;
        private int groundMask;
        private int jumpCount = 0;
        private bool isGrounded;
        private bool isTouchingWall;
        private bool jumpedDuringSprint;
        private float sprintJumpDirection;

        private void Awake()
        {
            if(GetComponent<AudioSource>() != null)
            {
                jumpAudioSource = GetComponent<AudioSource>();
            }

            //assign the rigidbody2D
            inputManager = GetComponent<InputManager>();
            rb2d = GetComponent<Rigidbody2D>();
            t = transform;
        }

        private void Start()
        {
            groundMask = 1 << LayerMask.NameToLayer(groundLayerName);
            
            inputManager.onJumpPressed = OnJumpPressed;
            inputManager.onJumpReleased = OnJumpReleased;

            DetermineFacingDirection();
        }

        private void Update()
        {
            if (showRayCastLines) ShowRayCastLines();
        }

        private void FixedUpdate()
        {
            ProcessRayCastChecks();
            UpdateHorizontalMovement();
            UpdateVerticalMovement();
        }

        void DetermineFacingDirection()
        {
            if (t.localScale.x < 0)
            {
                facingDirection = LEFT;
            }
            else
            {
                facingDirection = RIGHT;
            }
        }

        private void ShowRayCastLines()
        {
            //set a colour for all raycast visualizations
            Color c = Color.green;
            float gDist = groundCheckDistance;
            float wDist = wallCheckDistance;

            //visualize ground check rays
            Debug.DrawLine(groundCheckL.position, groundCheckL.position + Vector3.down * gDist, c);
            Debug.DrawLine(groundCheckM.position, groundCheckM.position + Vector3.down * gDist, c);
            Debug.DrawLine(groundCheckR.position, groundCheckR.position + Vector3.down * gDist, c);

            //visualise ceiling hit ray
            Debug.DrawLine(ceilingCheckL.position, ceilingCheckL.position + Vector3.up * gDist, c);
            Debug.DrawLine(ceilingCheckR.position, ceilingCheckR.position + Vector3.up * gDist, c);

            //visualize wall check rays
            if (facingDirection == Direction.RIGHT)
            {
                Debug.DrawLine(groundCheckR.position, groundCheckR.position + Vector3.right * wDist, c);
                Debug.DrawLine(groundCheckL.position, groundCheckL.position + Vector3.left * wDist, c);
            }
            else
            {
                Debug.DrawLine(groundCheckR.position, groundCheckR.position + Vector3.left * wDist, c);
                Debug.DrawLine(groundCheckL.position, groundCheckL.position + Vector3.right * wDist, c);
            }
        }

        private void ProcessRayCastChecks()
        {
            float gDist = groundCheckDistance;
            float wDist = wallCheckDistance;

            //shoots raycasts down from feet to look for ground
            RaycastHit2D groundHitL = Physics2D.Raycast(groundCheckL.position, Vector3.down, gDist, groundMask);
            RaycastHit2D groundHitM = Physics2D.Raycast(groundCheckM.position, Vector3.down, gDist, groundMask);
            RaycastHit2D groundHitR = Physics2D.Raycast(groundCheckR.position, Vector3.down, gDist, groundMask);

            //shoots a ray upwards to look for a ceiling
            RaycastHit2D ceilingHitL = Physics2D.Raycast(ceilingCheckL.position, Vector3.up, gDist, groundMask);
            RaycastHit2D ceilingHitR = Physics2D.Raycast(ceilingCheckR.position, Vector3.up, gDist, groundMask);

            //shoots a ray Right looking for a wall (used for wall jumping)
            RaycastHit2D wallHitR;
            RaycastHit2D wallHitL;

            if (facingDirection == Direction.RIGHT)
            {
                wallHitR = Physics2D.Raycast(groundCheckR.position, Vector3.right, wDist, groundMask);
                wallHitL = Physics2D.Raycast(groundCheckR.position, Vector3.left, wDist, groundMask);
            }
            else
            {
                wallHitR = Physics2D.Raycast(groundCheckR.position, Vector3.left, wDist, groundMask);
                wallHitL = Physics2D.Raycast(groundCheckR.position, Vector3.right, wDist, groundMask);
            }

            if (ceilingHitL.collider != null || ceilingHitR.collider != null)
            {
                wasJumpPressed = false;
                isTryingToJump = false;
            }

            if (groundHitL.collider != null || 
                groundHitM.collider != null || 
                groundHitR.collider != null)
            {
                if (!isGrounded)
                {
                    jumpedDuringSprint = false;
                }
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }

            //check if the player is touching a wall (used for wall jump)
            if (wallJumpEnabled && (wallHitR.collider != null || wallHitL.collider != null))
            {
                isTouchingWall = true;
            }
            else
            {
                isTouchingWall = false;
            }
        }

        internal void OnJumpPressed()
        {
            if ((isGrounded || (doubleJumpEnabled && jumpCount < 2)) || isTouchingWall)
            {
                PlayJumpSound();

                if (inputManager.isSprintPressed)
                {
                    jumpedDuringSprint = true; //flag the sprint jump for later use
                    sprintJumpDirection = inputManager.xAxis; //store the direction for later reference
                }

                wasJumpPressed = true;
                isTryingToJump = true;

                if (isGrounded)
                {
                    jumpCount = 0;
                }

                jumpCount++;
            }
        }

        private void PlayJumpSound()
        {
            if(jumpAudioSource != null)
            {
                jumpAudioSource.Play();
            }
        }

        internal void OnJumpReleased()
        {
            wasJumpPressed = false;
        }

        private void UpdateVerticalMovement()
        {
            if (wasJumpPressed)
            {
                wasJumpPressed = false;
                totalJumpTime = 0;
            }

            //heart of the variable jump 
            if (isTryingToJump)
            {
                totalJumpTime += Time.deltaTime;

                if (inputManager.isJumpPressed)
                {
                    if (totalJumpTime <= maxJumpTime)
                    {
                        rb2d.velocity = new Vector2(rb2d.velocity.x, initialJumpVelocity);
                    }
                    else
                    {
                        isTryingToJump = false;
                    }
                }
                else
                {
                    if (totalJumpTime < minJumpTime)
                    {
                        rb2d.velocity = new Vector2(rb2d.velocity.x, initialJumpVelocity);
                    }
                    else
                    {
                        isTryingToJump = false;
                    }
                }
            }

            //create a temp gravity value for convenience
            Vector2 vGravityY = Vector2.up * Physics2D.gravity.y;

            //check if the players jump is in the rising or falling phase and calulate physics
            if (rb2d.velocity.y < 0)
            {
                rb2d.velocity += vGravityY * fallMultiplier * Time.deltaTime;
            }
            else if (rb2d.velocity.y > 0 && isTryingToJump)
            {
                //determine how far though the jump we are as a decimal percentage 
                float t = totalJumpTime / maxJumpTime * 1;
                float tempJumpM = jumpMultiplier;

                //smooth out the peak of the jump, just like in super mario
                if (t > 0.5f)
                {
                    tempJumpM = jumpMultiplier * (1 - t);
                }

                //assign the final calculation to the rigidbody2D
                rb2d.velocity += vGravityY * tempJumpM * Time.deltaTime;
            }
        }

        private void UpdateHorizontalMovement()
        {
            Vector2 vel = rb2d.velocity;

            //check if currently walking or in the air
            if (isGrounded)
            {
                if (inputManager.isSprintPressed)
                {
                    currentSpeed = runSpeed;
                }
                else
                {
                    currentSpeed = walkSpeed;
                }
            }
            else
            {
                //handle sprint jumps and movement
                if (jumpedDuringSprint)
                {
                    if ((int)inputManager.xAxis == (int)sprintJumpDirection)
                    {
                        currentSpeed = runSpeed;
                    }
                    else
                    {
                        jumpedDuringSprint = false;
                        currentSpeed = walkSpeed;
                    }
                }
            }

            //check of left, right or nothing is pressed and set the velocity and facing position
            if (inputManager.isLeftPressed)
            {
                vel.x = -currentSpeed;
                t.localScale = new Vector2(-1, 1);
                facingDirection = LEFT;
            }
            else if (inputManager.isRightPressed)
            {
                vel.x = currentSpeed;
                t.localScale = new Vector2(1, 1);
                facingDirection = RIGHT;
            }
            else
            {
                vel.x = 0;
            }

            rb2d.velocity = vel;
        }
    }
}