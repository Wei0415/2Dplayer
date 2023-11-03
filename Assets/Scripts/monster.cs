using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform leftPoint, rightPoint;
    public float speed;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public Transform player;
    public float followRange = 5.0f;

    private bool isFacingLeft = true;
    private bool isGrounded;

    private float randomMoveTime = 2.0f;
    private float timeSinceLastRandomMove = 0.0f;
    private Vector2 randomMoveDirection = Vector2.zero;
    private Animator animator;
    private bool isFollowingPlayer = false;

    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;
    private bool isAttacking = false;
    private float timeSinceLastAttack = 0.0f;

    public int maxHealth = 100;
    public int currentHealth = 100;
    public int attackDamage = 10;
    private bool isDead  = false;
    private float lastAttackTime;



    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("Escape", isDead);
        currentHealth = maxHealth;
        isAttacking = false;
        ResetRandomMove();
        Move();

    }

    private void Update()
{
    isGrounded = IsGrounded();

    //是否在跟蹤範圍內
    if (Vector2.Distance(transform.position, player.position) <= followRange)
    {
        isFollowingPlayer = true;
    }
    else
    {
        isFollowingPlayer = false;
    }

    if (isFollowingPlayer)
    {
        FollowPlayer();
        
        if (Vector2.Distance(transform.position, player.position) <= attackRange && !isAttacking && !isDead)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                StartCoroutine(ExecuteAttack());
                lastAttackTime = Time.time;
            }
        }
    }
    else
    {
        RandomMove();
    }
    timeSinceLastAttack += Time.deltaTime;
}

    
    private bool IsGrounded() //地面檢測
    {
        float radius = 0.05f; 
        Vector2 checkPosition = new Vector2(transform.position.x, transform.position.y - 1.25f);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPosition, radius, groundLayer);
        return colliders.Length > 0;
    }

    private void Move() //移動
    {
    if (isGrounded)
    {
        animator.SetBool("Walk", true);
        float horizontalSpeed = isFacingLeft ? -speed : speed;
        rb.velocity = new Vector2(horizontalSpeed, rb.velocity.y);

        if ((isFacingLeft && transform.position.x <= leftPoint.position.x) && !isDead||
            (!isFacingLeft && transform.position.x >= rightPoint.position.x) && !isDead)
        {
            Flip();
        }
    }
    else
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
    }
    private void Flip() //角色反轉
    {
        isFacingLeft = !isFacingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void RandomMove() //隨機移動
    {
        animator.SetBool("Walk", true);
        timeSinceLastRandomMove += Time.deltaTime;
        if (timeSinceLastRandomMove >= randomMoveTime)
        {
            randomMoveDirection = new Vector2(Random.Range(-1f, 1f), 0).normalized;
            timeSinceLastRandomMove = 0f;
            randomMoveTime = Random.Range(2.0f, 5.0f);
        }
        rb.velocity = new Vector2(randomMoveDirection.x * speed, rb.velocity.y);
    }

    private void ResetRandomMove()
    {
        //初始化
        timeSinceLastRandomMove = 0.0f;
        randomMoveDirection = Vector2.zero;
        randomMoveTime = Random.Range(2.0f, 5.0f);
    }

    private void FollowPlayer() //跟隨玩家
    {
        float horizontalSpeed = player.position.x < transform.position.x ? -speed : speed;
        rb.velocity = new Vector2(horizontalSpeed, rb.velocity.y);

        if ((isFacingLeft && transform.position.x <= leftPoint.position.x) && !isDead ||
            (!isFacingLeft && transform.position.x >= rightPoint.position.x) && !isDead)
        {
            Flip();
        }
    }
    
    private IEnumerator ExecuteAttack() //攻擊
    {
       isAttacking = true;
       animator.SetBool("isAttacking", true);
       yield return new WaitForSeconds(2.5f); // 等待動畫2.5秒
       isAttacking = false;
       animator.SetBool("isAttacking", false);

       Vector2 attackDirection = new Vector2(1.0f, 0.0f);
       if (GetComponent<SpriteRenderer>().flipX)
       {
           attackDirection.x = 1.0f;
       }
       else
       {
           attackDirection.x = -1.0f;
       }

       Vector2 attackPosition = (Vector2)transform.position + attackDirection * 1.0f;
       float attackRadius = 1.0f;
       Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPosition, attackRadius);

       foreach (Collider2D hitCollider in hitColliders)
       {
           Player player = hitCollider.GetComponent<Player>();
           if (player != null)
           {
               player.TakeDamage(attackDamage);
           }
       }
    }
   
    public void TakeDamage(int damage)
    {
        if(isDead){
            return;
        }
       currentHealth -= damage;
       if (currentHealth <= 0)
       {
           isDead = true;
           animator.SetBool("Escape", true);
           rb.velocity = Vector2.zero;
           Destroy(gameObject, 5.0f);
       }
    }

}
