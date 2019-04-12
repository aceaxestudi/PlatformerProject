using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private int amountOfJumpsLeft; // 7. The amount of times the character is still able to jump
    private int facingDirection = 1; // 14. The direction the character is facing

    private float movementInputDirection; // 1. Float to hold the direction the play wishes to move.

    private bool isFacingRight = true; // 2. Holds wether or not the character is facing right
    private bool isWalking = false; // 4. Wether or not the character actually move when attempting to walk
    private bool isGrounded; // 5. Wether or not the character is currently touching ground
    private bool canJump = true; // 6. Wether or not the character can currently jump
    private bool isTouchingWall; // 9. Wether or not the character is currently touching a wall
    private bool isWallSliding; // 10. Wether or not the character is currently wallSliding
    private bool isAttacking;

    private Rigidbody2D rb; // 1. Holds reference to the Rigidbody attached to the character. Used to move the character.
    private Animator anim; // 4. Holds refernece to Animator component so we can change the animations

    public int amountOfJumps = 1; // 7. The total amount of times the character can jump

    public float movementSpeed = 5.0f; // 1. Holds speed at which the character moves.
    public float jumpForce = 10.0f; // 3. The force at which the character jumps
    public float groundCheckRadius; // 5. The radius at which we are checking for ground
    public float wallCheckDistance; // 9. The distnace we look for if the character is touching a wall
    public float wallSlideSpeed = 3.0f; // 10. The speed at which the character slides down the wall
    public float airMovementForce = 2.0f; // 12. The force applied to the character when moving in the air
    public float airDragMultiplier = 0.5f; // 12. The rate at which the characters velocity decays in the air
    public float variableJumpHightMultiplier = 0.75f; // 13. The rate at which jump hight decays
    public float wallJumpForce; // 14.
    public float wallHopForce; // 14.


    //Slash Attack
    public float slashAttackRadius = 1.0f;
    public float slashAttackDamage = 25.0f;
    public float slashAttackKnockBack = 10.0f;

    public Vector2 wallJumpDirection; // 14. How the force is split up when wall jumping
    public Vector2 wallHopDirection; // 14. How the force is split up when hopping off wall

    public LayerMask whatIsGround; // 5. A layer mask that specifies what is ground
    public LayerMask whatIsDamageable;

    public Transform groundCheckTransform; // 5. Holds the transform of the position where we will be checking for ground
    public Transform wallCheckTransform; // 9. Hold transform of position where we will check for wall
    public Transform slashAttackCheckTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 1. Sets the reference to the Rigidbody.
        anim = GetComponent<Animator>(); // 4. Sets the animator reference
        amountOfJumpsLeft = amountOfJumps; // 7. Initializes it
        wallJumpDirection.Normalize(); // 14. Makes vector length 1
        wallHopDirection.Normalize(); // 14.Ditto
    }

    void Update()
    {
        CheckInput(); // 1. Function to check all user input related to the character.
        CheckMovementDirection(); // 2.
        UpdateAnimations(); // 4. Used to update all animations based on character condition
        CheckIfCanJump(); // 6. Check if the player can currently jump or not
        CheckWallSliding(); // 10. Function that determines if the character is currently wall sliding or not
    }

    private void FixedUpdate() // 1.
    {
        CheckSurroundings(); // 5.  Function where we will check if we are touching ground
        ApplyMovement(); // 1. Applies movement to the character. 
    }

    private void CheckInput() // 1. Function that check's all user input for character every update.
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal"); // 1. Gets the raw input (-1, 0, 1) from the player in the horizontal direction (A, D) Keys.

        if (Input.GetButtonDown("Jump")) // 3. Checks if player is attempting to jump
        {
            Jump();
        }

        if (Input.GetButtonUp("Jump")) // 13. Cut y vel when jump button is released
        {
            if(rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHightMultiplier);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            isAttacking = true;
        }

    }

    private void CheckWallSliding() // 10. Check if the character should be wall sliding
    {
        if(isTouchingWall && !isGrounded && rb.velocity.y < 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckIfCanJump() // 6. Check if the player can currently jump or not
    {
        if(isGrounded && rb.velocity.y <= 0 || isWallSliding)
        {
            amountOfJumpsLeft = amountOfJumps; // 7. Resets how many times the character can jump when touching ground
        }

        if(amountOfJumpsLeft <= 0) // 7. Makes it so character can only jump if jumps left
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }
       
    }

    void ChangeAttackState()
    {
        isAttacking = false;
    }

    private void CheckSurroundings() // 5. Checks for ground (and eventually walls)
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, whatIsGround); // 5. Checks for ground

        isTouchingWall = Physics2D.Raycast(wallCheckTransform.position, wallCheckTransform.right, wallCheckDistance, whatIsGround); // 9. Checks for wall using RayCast
    }

    private void ApplyMovement() // 1.
    {
        if (isGrounded) // 10. Only move when is grounded
        {
            rb.velocity = new Vector2(movementInputDirection * movementSpeed, rb.velocity.y); // 1. Applies movement when player pushes A or D
        }
        else if(!isGrounded && !isWallSliding && movementInputDirection != 0) // 12. Apply force when in the air to move
        {
            Vector2 forceToAdd = new Vector2(airMovementForce * movementInputDirection, 0);
            rb.AddForce(forceToAdd);

            // 12. Clamp x velocity
            if(Mathf.Abs(rb.velocity.x) > movementSpeed)
            {
                rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
            }

        }
        else if(!isGrounded && !isWallSliding && movementInputDirection == 0) // 12. Apply drag when no input is given
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }


        if (isWallSliding) // 10. Limit down velocity when wall sliding
        {
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    private void SlashAttack()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(slashAttackCheckTransform.position, slashAttackRadius, whatIsDamageable);

        for(int i = 0; i < hit.Length; i++)
        {
            EnemyController script = hit[i].GetComponent<EnemyController>();
            script.Damage(slashAttackDamage, slashAttackKnockBack, facingDirection);
            //script.KnockBack(slashAttackKnockBack, facingDirection);
        }


    }

    private void CheckMovementDirection() // 2.
    {
        if (isFacingRight && movementInputDirection < -0.1f) // 2. If character is facing right but trying to move left, flip
        {
            Flip();
        }
        else if (!isFacingRight && movementInputDirection > 0.1f) // 2. vice versa
        {
            Flip();
        }

        if(Mathf.Abs(rb.velocity.x) > 0.01f) // 4. Determines if cahracter actually moves if attempting
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void UpdateAnimations() // 4.
    {
        anim.SetBool("isWalking", isWalking); // 4. Sets isWalking parameter in animator
        anim.SetBool("isGrounded", isGrounded); // 8. Updates is grounded parameter
        anim.SetFloat("yVelocity", rb.velocity.y); // 8. Updates Y velocity in animator
        anim.SetBool("isWallSliding", isWallSliding); // 11. Update isWallSliding parameter
        anim.SetBool("isAttacking", isAttacking);
    }

    private void Flip() // 2. Function used to flip the direction the character sprite is facing
    {
        if (!isWallSliding) // 10. Only flip if not wall sliding
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
            facingDirection = -facingDirection; // 14. Updates facing direction for math
        }
    }

    private void Jump() // 3.
    {
        if (canJump && !isWallSliding) // 6. Only jump if you can, 14. Add !isWallSliding
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 3. Set's upwards velocity when jumping
            amountOfJumpsLeft--; // 7. One less jump left
        }
        else if (isWallSliding && movementInputDirection == 0 && canJump) // 14. Hop off wall
        {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
        else if ((isWallSliding || isTouchingWall) && movementInputDirection != 0 && canJump && movementInputDirection != facingDirection)
        {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmos() // 5. Makes it so we can see our checks in the scene view
    {
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius); // 5. Shows the ground check circle
        Gizmos.DrawWireSphere(slashAttackCheckTransform.position, slashAttackRadius); 
        Gizmos.DrawLine(wallCheckTransform.position, new Vector3(wallCheckTransform.position.x + wallCheckDistance, wallCheckTransform.position.y, wallCheckTransform.position.z)); // 9. Shows wall check in scene view
    }
}
