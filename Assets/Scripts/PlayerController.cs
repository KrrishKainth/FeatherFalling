using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    Camera camera;
    public CameraController cameraController;
    public Rigidbody2D rb;
    bool inJump;
    public int numJumps;
    float maxJumpPower = 12.5f;
    float maxJumpChargeTime = 0.7f;
    float jumpChargeStart;
    bool jumpCharging;
    InputAction jumpAction;
    bool inBounce;
    float bouncePower = 2f;
    Vector2 bounceImpulse;
    float bounceStart;
    float bounceStickTime = 0.217f;  // match squish animation duration
    float gravity;
    List<GameObject> fruits;
    float inGameTimer;
    bool inMenu;
    Animator animator;
    float scale;

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
        fruits = new List<GameObject>();
        inGameTimer = 0;
        inMenu = false;
        animator = gameObject.GetComponent<Animator>();
        scale = transform.localScale.x;
    }

    void Update()
    {
        refreshJump();
        if (inBounce)
        {
            bounce();
        }
        
        inGameTimer += Time.deltaTime;

        // Animations
        animator.SetBool("onGround", !inJump);
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetFloat("xSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("jumpCharging", jumpCharging);
        animator.SetBool("inBounce", inBounce);

        if (!jumpCharging && !inBounce)
        {
            // If x velocity is 0, use the last direction of travel
            if (rb.velocity.x > 0)
            {
                transform.localScale = new Vector2(scale, scale);
            }
            else if (rb.velocity.x < 0)
            {
                transform.localScale = new Vector2(-1 * scale, scale);
            }
        }
        else if (jumpCharging) // If jump charging, face direction aimed at by mous
        {
            if (camera.ScreenToWorldPoint(Input.mousePosition).x <= transform.position.x)
            {
                transform.localScale = new Vector2(scale, scale);
            }
            else
            {
                transform.localScale = new Vector2(-1 * scale, scale);
            }
        }
    }

    // Movement
    void OnJump()
    {
        if (numJumps > 0 && !inMenu)
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

    void OnTriggerEnter2D(Collider2D other)
    {
        // Collision with fruit
        if (other.gameObject.tag == "Fruit")
        {
            fruits.Add(other.gameObject);
            other.gameObject.SetActive(false);
            numJumps++;
        }

        // On a platform
        if (other.gameObject.tag == "Platform")
        {
            inJump = false;
        }
    }

    // void OnTriggerStay2D(Collider2D other)
    // {
    //     // On a platform
    //     if (other.gameObject.tag == "Platform")
    //     {
    //         inJump = false;
    //     }
    // }

    // If feet hitbox leave a platform, player is in air
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Platform")
        {
            inJump = true;
        }
    }

    // Collision with platforms 
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Platform")
        {
            Vector2 collisionDir = other.GetContact(0).normal;

            // Collision with wall or ceiling while in air -> bounce
            if (inJump && 
                ((collisionDir == Vector2.left) || 
                (collisionDir == Vector2.right) || 
                (collisionDir == Vector2.down)))
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

                if (collisionDir == Vector2.down)
                {
                    animator.SetBool("sideBounce", false);
                }
                else
                {
                    animator.SetBool("sideBounce", true);
                }
            }
            else if (collisionDir == Vector2.up)
            {
                // Shake camera if you hit the ground too hard
                if (Mathf.Abs(other.relativeVelocity.y) > 15)
                {
                    cameraController.cameraShake();
                }
            }
        }
    }

    void refreshJump()
    {
        // Refresh jump and fruits when stationary on a platform
        if (rb.velocity.magnitude <= 0.1 && !inJump)
        {
            numJumps = 1;

            foreach (GameObject f in fruits)
            {
                f.SetActive(true);
            }
            fruits.Clear();
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
