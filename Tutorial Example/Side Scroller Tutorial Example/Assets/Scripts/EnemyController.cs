using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator anim;

    public Vector2 knockbackDirection;

    [SerializeField] private float health = 100.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        knockbackDirection.Normalize();
    }

    void Update()
    {
        
    }

    public void KnockBack(float knockbackStrength, int leftOrRight)
    {
        Vector2 forceToAdd = new Vector2(knockbackDirection.x * knockbackStrength * leftOrRight, knockbackDirection.y * knockbackStrength);
        rb.AddForce(forceToAdd, ForceMode2D.Impulse);   
    }

    public void Damage(float amount)
    {
        health -= amount;
        Debug.Log("OUCH!");
        anim.SetTrigger("damage");

        if(health <= 0)
        {
            GameObject.Destroy(gameObject, 0.50f);
        }
    }
}
