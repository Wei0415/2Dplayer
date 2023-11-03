using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float damping = 0.1f;
    public float stationaryJumpForce = 12f;
    public float jumpForce = 10f;
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int attackDamage = 10;


    private Vector2 moveInput;
    private Vector2 smoothMovement;
    private Vector2 smoothVelocity;
    private Rigidbody2D rb;
    private Animator animator;


    private bool isGrounded;
    public bool isDead = false;
    private bool isJumping = false;
    private bool isAttacking = false;
    
    public LayerMask groundLayer;
    public Transform groundCheck;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isDead", isDead);
        currentHealth = maxHealth;
    }

    private void Update()
    {
        isGrounded = IsGrounded();

        if (isGrounded)
        {
            isJumping = false;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame) //檢測左鍵
        { 
            if (!isAttacking && !isDead)
            {
               StartCoroutine(ExecuteAttack());
            }
        }   
    }

    private void FixedUpdate()
    {
        SetMoveInput();
        SetLookDirection();
        HandleJumpInput();
    }

    private bool IsGrounded() //地面檢測
    {
        float radius = 0.05f; 
        Vector2 checkPosition = new Vector2(transform.position.x, transform.position.y - 1.25f);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPosition, radius, groundLayer);
        return colliders.Length > 0;
    }

    private void SetMoveInput()//走路與奔跑
    {
        float currentSpeed = speed;

        if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed && !isDead) //按下Shift奔跑
        {
            currentSpeed = speed * 1.75f; 
        }
        if(!isDead){
        moveInput = new Vector2(Keyboard.current.dKey.isPressed ? 1 : Keyboard.current.aKey.isPressed ? -1 : 0, 0);
        smoothMovement = Vector2.SmoothDamp(smoothMovement, moveInput, ref smoothVelocity, damping);
        rb.velocity = new Vector2(moveInput.x * currentSpeed, rb.velocity.y);
        }
    }

    private void SetLookDirection() //走路與奔跑動畫
    {
        if (moveInput == Vector2.zero && !isDead)
        {
           animator.SetBool("Walk", false);
        }
        else
        {
            if (Keyboard.current.leftShiftKey.isPressed && !isDead || Keyboard.current.rightShiftKey.isPressed && !isDead)
            {
                animator.SetBool("Run", true); 
                animator.SetBool("Walk", false); 
            }
            else if(!isDead)
            {
                animator.SetBool("Run", false); 
                animator.SetBool("Walk", true);

            }
            GetComponent<SpriteRenderer>().flipX = moveInput.x < 0;
        }
    }

    private void HandleJumpInput() //跳躍
    {
        if (isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame && !isDead)
        {
            Jump(stationaryJumpForce); //原地
        }
        else if (isGrounded && Keyboard.current.spaceKey.isPressed && !isJumping && !isDead)
        {
            Jump(jumpForce); //移動中
        }
    }

    private void Jump(float jumpForce)
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isJumping = true;
    }

    private IEnumerator ExecuteAttack() //攻擊
    {
       isAttacking = true;
       animator.SetBool("isAttacking", true);
       yield return new WaitForSeconds(1f); // 等待動畫1秒
       isAttacking = false;
       animator.SetBool("isAttacking", false);

       Vector2 attackDirection = new Vector2(1.0f, 0.0f);
       if (!GetComponent<SpriteRenderer>().flipX)
       {
        attackDirection.x = 1.0f;
       }
       else
       {
        attackDirection.x = -1.0f;
       }

       Vector2 attackPosition = (Vector2)transform.position + attackDirection * 1.0f; 
       float attackRadius = 2.0f;
       Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPosition, attackRadius);

       foreach (Collider2D hitCollider in hitColliders)
       {
           Monster monster = hitCollider.GetComponent<Monster>();
           if (monster != null)
           {
               monster.TakeDamage(attackDamage);
           }
       }
    }

    public void TakeDamage(int damage)//受到傷害
    {
        if(isDead){
            return;
        }
        currentHealth -= damage;
        
        if (currentHealth <= 0) //是否死亡
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Walk", false);
            animator.SetBool("Dead", true);
            isDead = true;
        } 
        return;
    }

}