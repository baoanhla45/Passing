using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private bool isfacingright = true;
    private int jumpbuffercounter;
    private float coyotetimecounter = 0;
    private bool candash=true;
    private bool dashed = false;
    private float gravity;
    private int airjumpcounter;
    [Header("Jump Setting")]
    [SerializeField] private int Maxairjump;
    [SerializeField] private float CoyoteTime;
    [SerializeField] private float JumpPower = 20f;
    [SerializeField] private int JumpBufferFrames;
    [Header("Speed Setting")]
    [SerializeField] private float Speed = 6f;
    [Header("Ground Check Setting")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private LayerMask GroundLayer;
    [Header("Dash Setting")]
    [SerializeField] private float DashTime;
    [SerializeField] private float DashCoolDown;
    [SerializeField] private float DashSpeed;
    Animator Anim;
    PlayerStateList pState;


    public static PlayerMovement Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        gravity = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        Flip();
        GetInput();
        if (pState.dashing)
        {
           return; 
        }
        UpdateJumpVar();
        Jump();
        StartDash();
        

    }

    void StartDash()
    {
        Debug.Log($"candash: {candash}, dashed: {dashed}");
        if (Input.GetButtonDown("Dash")&&candash&&!dashed)
        {
            Debug.Log("s");
            StartCoroutine(Dash());
            dashed = true;
        }
        if (IsGround())
        {
            dashed = false;
        }
    }
    IEnumerator Dash()
    {
        candash = false;
        pState.dashing = true;
        Anim.SetTrigger("Dashing");
        Debug.Log("Dash trigger activated");
        rb.gravityScale = 0;
            rb.velocity = new Vector2(transform.localScale.x * DashSpeed, 0);
            yield return new WaitForSeconds(DashTime);
            rb.gravityScale = gravity;
            pState.dashing = false;
            yield return new WaitForSeconds(DashCoolDown);
            candash = true;
    }
    void Jump()
    {
        if (!pState.jumping)
        {
            if (jumpbuffercounter > 0 && coyotetimecounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, JumpPower);
                pState.jumping = true;
                jumpbuffercounter = 0;
            }
            else if (IsGround() && airjumpcounter < Maxairjump && Input.GetButtonDown("Jump"))
            {
                pState.jumping = true;
                airjumpcounter++;
                rb.velocity = new Vector2(rb.velocity.x, JumpPower);
            }
        }
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            pState.jumping = false;
        }

        Anim.SetBool("Jumping", !IsGround());
    }
    void UpdateJumpVar()
    {
        if (IsGround())
        {
            pState.jumping = false;
            coyotetimecounter = CoyoteTime;
            jumpbuffercounter = 0;
        }
        else
        {
            coyotetimecounter -= Time.deltaTime;
        }
        if (Input.GetButtonDown("Jump"))
        {
            jumpbuffercounter = JumpBufferFrames;
        }
        else
        {
            jumpbuffercounter--;
        }
    }
    void GetInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        Anim.SetBool("Walking", IsGround() && rb.velocity.x != 0f);
        bool isWalking = IsGround() && MathF.Abs(horizontal) > 0.1f;
        Anim.SetBool("Walking", isWalking);
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * Speed, rb.velocity.y);
    }

    private bool IsGround()
    {
        return Physics2D.OverlapCircle(GroundCheck.position, 0.2f, GroundLayer);
    }
    private void Flip()
    {
        if (isfacingright && horizontal < 0f || !isfacingright && horizontal > 0f)
        {
            isfacingright = !isfacingright;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
