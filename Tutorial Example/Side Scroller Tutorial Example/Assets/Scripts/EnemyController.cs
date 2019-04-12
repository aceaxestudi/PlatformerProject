using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float movementSpeed = 10.0f;
    public float xDistanceRight = 1.0f;
    public float xDistanceLeft = 1.0f;
    public float timeTillCanMove = 0.5f;

    private float timeLeft;

    private Rigidbody2D rb;
    private Animator anim;

    private int facingDirection = 1;

    private bool canMove = true;

    public Vector2 knockbackDirection;

    //private Transform waypointRight;
    //private Transform waypointLeft;

    private Vector3 waypointRight;
    private Vector3 waypointLeft;
    private Vector3 startPos;

   private  int currentWaypoint = 1;


    [SerializeField] private float health = 100.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        knockbackDirection.Normalize();
        waypointLeft = new Vector3(transform.position.x - xDistanceLeft, transform.position.y, transform.position.z);
        waypointRight = new Vector3(transform.position.x + xDistanceRight, transform.position.y, transform.position.z);
        startPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    private void Update()
    {
        CheckIfCanMove();
    }

    void FixedUpdate()
    {
        Move();
        CheckWaypoints();
    }

    private void Move()
    {
        if (canMove)
        {
            rb.velocity = new Vector2(facingDirection * movementSpeed, rb.velocity.y);
        }
    }

    private void CheckIfCanMove()
    {
        if(timeLeft <= 0)
        {
            canMove = true;
        }
        else
        {
            timeLeft -= Time.deltaTime;
        }
    }

    private void CheckWaypoints()
    {
        if(transform.position.x >= waypointRight.x && currentWaypoint == 1)
        {
            Flip();
            currentWaypoint = 2;
        }
        else if(transform.position.x <= waypointLeft.x && currentWaypoint == 2)
        {
            Flip();
            currentWaypoint = 1;
        }
    }

    private void KnockBack(float knockbackStrength, int leftOrRight)
    {
        Vector2 forceToAdd = new Vector2(knockbackDirection.x * knockbackStrength * leftOrRight, knockbackDirection.y * knockbackStrength);
        rb.AddForce(forceToAdd, ForceMode2D.Impulse);   
    }

    public void Damage(float amount, float knockbackStrength, int leftOrRight)
    {
        health -= amount;
        Debug.Log("OUCH!");
        anim.SetTrigger("damage");
        canMove = false;
        timeLeft = timeTillCanMove;
        KnockBack(knockbackStrength, leftOrRight);

        if (health <= 0)
        {
            GameObject.Destroy(gameObject, 0.50f);
        }
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(new Vector2(startPos.x - xDistanceLeft, startPos.y), 0.25f);
        Gizmos.DrawWireSphere(new Vector2(startPos.x + xDistanceRight, startPos.y), 0.25f);
    }
}
