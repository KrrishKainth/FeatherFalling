using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    Camera camera;
    Rigidbody2D rb;
    bool inJump;
    public int numJumps;
    float maxJumpPower = 20;
    float maxJumpChargeTime = 0.7f;
    float jumpChargeStart;
    bool jumpCharging;
    InputAction jumpAction;
    bool inBounce;
    float bouncePower = 2f;
    Vector2 bounceImpulse;
    float bounceStart;
    float bounceStickTime = 0.25f;
    float gravity;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        jumpAction = gameObject.GetComponent<PlayerInput>().actions["Jump"];
        inJump = false;
        inBounce = false;
        jumpCharging = false;
        numJumps = 0;
        camera = Camera.main;
        jumpChargeStart = 0;
        gravity = rb.gravityScale;
    }

    void FixedUpdate()
    {
        refreshJump();
        if (inBounce)
        {
            bounce();
        }
    }

    // Movement
    void OnJump()
    {
        if (numJumps > 0)
        {
            if ((int) jumpAction.phase == 3)  // Jump charge started
            {
                jumpChargeStart = Time.time;
                jumpCharging = true;
            }
            else if ((int) jumpAction.phase == 4 && jumpCharging) // Jump charge released
            {
                Vector3 mousePos3D = camera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos3D.x, mousePos3D.y);
                Vector2 playerPos2D = new Vector2(transform.position.x, transform.position.y);
                Vector2 jumpVector = playerPos2D - mousePos2D;

                // Direction filtering to set unit jump vector
                if (jumpVector.magnitude < 0.75)  // Click on player
                {
                    jumpVector = new Vector2(0, 1);
                }
                else
                {
                    // Get angle of raw jump vector
                    float jumpDir = Mathf.Atan(jumpVector.y / jumpVector.x);
                    if (mousePos2D.x > playerPos2D.x)
                        jumpDir += Mathf.PI;

                    // Round angle of raw jump vector to a multiple of 15 degrees
                    jumpDir = Mathf.Round(jumpDir / (Mathf.PI / 12)) * (Mathf.PI / 12);

                    // Set jump vector to unit vector in adjusted direction
                    jumpVector = new Vector2(Mathf.Cos(jumpDir), Mathf.Sin(jumpDir));
                }
                
                float jumpChargeTime = Time.time - jumpChargeStart;
                if (jumpChargeTime > maxJumpChargeTime)
                {
                    jumpChargeTime = maxJumpChargeTime;
                }

                // Set jump power between half of max power and max power, based on jump charge time
                float jumpPower = 0.5f * maxJumpPower * jumpChargeTime / maxJumpChargeTime + 0.5f * maxJumpPower;

                Vector2 jumpImpulse = jumpVector * jumpPower;
                rb.AddForce(jumpImpulse, ForceMode2D.Impulse);
                numJumps--;
                jumpCharging = false;
            }
        }
    }

    // Collision with platforms 
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Platform")
        {
            Vector2 collisionDir = other.GetContact(0).normal;
            if (collisionDir == Vector2.up)  // bottom collision -> landed on platform
            {
                inJump = false;
            }
            else  // sideways or top collision
            {
                if (inJump)
                {
                    // Player should bounce off wall
                    inBounce = true;
                    Vector2 playerVelocity = -1 * other.relativeVelocity;
                    Vector2 velocityIntoWall = (playerVelocity.x * collisionDir.x + 
                                                playerVelocity.y * collisionDir.y) * collisionDir;
                    bounceImpulse = playerVelocity - bouncePower * velocityIntoWall;
                    bounceStart = Time.time;
                    rb.velocity = new Vector2(0, 0);
                    rb.gravityScale = 0f;
                }
            }
        }
    }

    // If feet hitbox leave a platform, player is in air
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Platform")
        {
            inJump = true;
        }
    }

    void refreshJump()
    {
        // Refresh jump when stationary on a platform
        if (rb.velocity.magnitude <= 0.1 && !inJump)
        {
            numJumps = 1;
        }
    }

    void bounce()
    {
        // If bouncing and have stuck to the wall for long enough, bounce
        if (Time.time - bounceStart >= bounceStickTime)
        {
            inBounce = false;
            rb.AddForce(bounceImpulse, ForceMode2D.Impulse);
            rb.gravityScale = gravity;
        }
    }
}
