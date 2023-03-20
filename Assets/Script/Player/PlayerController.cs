using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    public Transform groundCheck;
    public Transform wallCheck;

    private float moveInputDir;
    public float moveSpeed = 10f;
    public float moveSpeedInit = 10f;
    public float jumpforce = 16f;
    public float wallSlideSpeed = 3f;//墙壁下落速度
    public float movementForceInAir = 50f;//设置空中跳跃转移附加力度
    public float airDragMultiplier = 0.95f;//设置跳跃时往返的阻力系数
    public float variableJumpHeight = 0.5f;//设置点按跳跃的跳跃系数
    [Header("跳墙")]
    public float wallHopForce;//蹬墙力度
    public float wallJumpForce;//跳墙力度
    public Vector2 wallHopDir;//确定从墙上掉落的方向
    public Vector2 wallJumpDir;
    private int facingDirection = 1;
    [Header("检测范围")]
    public float groundCheckRadius;//地面检测范围
    public float wallCheckDistance;//墙壁检测距离
    public LayerMask ground;
    [Header("跳跃改进")]
    private float jumpTimer;
    public float jumpTimerSet = 0.15f;//连跳间隔
    //在墙上延迟
    public float turnTimerSet = 0.1f;
    private float turnTimer;
    //连跳
    private float wallJumpTimer;//在墙上连跳计时器
    public float wallJumpTimerSet = 0.5f;
    private int lastWallJumpDirection;

    public int amountOfJump = 1;//控制跳跃次数
    public int amountOfJumpLeft;//控制剩余跳跃次数
    [Header("攀爬参数设置")]
    public Transform ledgeCheck;
    public float ledgeCheckDistance;//检测距离
    private bool isTouchingLedge;
    private bool canClimbLedge;
    private bool ledgeDetected;
    private Vector2 ledgePosBot;//取得上面射线Ledgecheck的位置
    private Vector2 ledgePos1;
    public Vector2 ledgePos2;//角色左右攀爬时最后到达的位置

    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;

    [Header("冲刺参数设置")]   
    public float dashSpeed;//冲刺速度
    public float distanceBetweenImage;//残影之间的间隔
    public float dashCoolDown;//冲刺冷却时间
    public float dashTime = 0.3f;//冲刺持续时间

    private bool isDashing;
    private float dashTimeLeft;//记录距上一次冲刺的时间
    private float lastImagePos;
    private float lastDash = -100;//记录上一次冲刺时间
    [Header("角色击退参数设置")]
    public float knockbackDuration;//击退持续时间
    public Vector2 knockbackSpeed;//击退速度

    private float knockbackStartTime;
    private bool knockback;
    [Header("判断条件")]
    public bool isFacingRight = true;
    public bool isTouchingWall;
    public bool isWallSliding;
    public bool isWalking;
    public bool canJump;
    public bool isGround;
    public bool isAttemptingToJump;
    public bool canNormalJump;
    public bool canWallJump;
    private bool checkJumpMultiplier;//判断是否为点跳
    public bool canMove;
    public bool canFlip;
    private bool hasWallJump;
    public bool allowTurn;
    private bool CanJump;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpLeft = amountOfJump;
        wallHopDir.Normalize();//归一化
        wallJumpDir.Normalize();
        allowTurn = true;
        moveSpeedInit = moveSpeed;
        CanJump = true;
    }
    void Update()
    {
        CheckInput();
        SwitchAnim();
        CheckIfWallSliding();
        if (CanJump)
        {        
            CheckIfCanJump();
            CheckJump();
        }
        CheckIfLedgeClimb();
        CheckDash();
        CheckKnockback();

    }
    private void FixedUpdate()
    {       
        CheckMoveDirection();
        ApplyMovement();
        CheckSurroundings();   
        CheckIsWalking();  
    }
    private void SwitchAnim()//转换动画函数
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGround", isGround);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }
    private void CheckMoveDirection()//改变朝向同时也能改变射线的朝向
    {
        if (allowTurn)
        {
            if (isFacingRight && moveInputDir > 0)
            {
                Flip();
            }
            else if(!isFacingRight&&moveInputDir < 0)
            {
                Flip();
            }
        }
    }
    public void DisableFlip()//在动画事件中调用
    {
        canFlip = false;
    }
    public void EnableFlip()
    {
        canFlip = true;
    }
    private void Flip()//改变朝向实现函数
    {
        if (!isWallSliding && canFlip && !knockback)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180f, 0f);
        }
    }
    private void CheckIfWallSliding()//检测是否在墙上滑动
    {
        if (isTouchingWall && moveInputDir == facingDirection && rb.velocity.y < 0 && !canClimbLedge)//如果键入的方向与角色朝向相同 
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void CheckIsWalking()//检测是否在移动
    {
        if (Mathf.Abs(rb.velocity.x) > 0.01f)
            isWalking = true;
        else
            isWalking = false;
    }
    private void CheckSurroundings()//发出射线检测周围环境
    {
        isGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, ground);//向下发送射线
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, ground);//向右发射射线
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, ledgeCheckDistance, ground);
        if(isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected= true;
            ledgePosBot = ledgeCheck.position;
        }
    }
    private void OnDrawGizmosSelected()//显示上面检测地面的范围
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
        Gizmos.DrawLine(ledgeCheck.position, new Vector3(ledgeCheck.position.x + ledgeCheckDistance, ledgeCheck.position.y, ledgeCheck.position.z));
    }
    private void CheckIfLedgeClimb()//检测是否能攀爬
    {
        if (ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;
            if (!isFacingRight)//设置左右攀爬位置的偏移，可手动调整变量以获得最好效果
            {//Mathf.Floor(float f)函数为返回小于等于f的最大整数
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, 
                                        Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2,
                                        Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {//Mathf.Ceil(float f)函数为返回大于等于f的最小整数
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1,
                        Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset2,
                                        Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            canMove = false;
            canFlip = false;
            rb.velocity = Vector2.zero;//让其攀爬时静止
            moveSpeed = 0f;//让其攀爬时不能再移动
            allowTurn = false;//让其攀爬时不能转向
            isTouchingWall = false;
            canWallJump = false;
            CanJump = false;
            canNormalJump = false;
            rb.gravityScale = 0f;
            transform.position=new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);
            anim.SetBool("canClimbLedge", canClimbLedge);
        }
    }
    public void FinishLedgeClimb()//用于插入到攀爬动画中，在动画结束后执行
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        allowTurn = true;
        canNormalJump = true;
        CanJump = true;
        ledgeDetected = false;
        rb.gravityScale = 8f;
        moveSpeed = moveSpeedInit;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }
    private void CheckInput()//检测输入，控制连跳，长跳，点跳
    {
        moveInputDir = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            if(isGround || (amountOfJumpLeft > 0 && isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }
        if (Input.GetButtonDown("Horizontal") && isTouchingWall)//角色贴墙时让其暂停移动与翻转
        {
            if(!isGround && moveInputDir != facingDirection)
            {
                canFlip = false;
                canMove = false;
                turnTimer = turnTimerSet;//增加一个计时器让玩家有反应时间
            }
        }
        if (turnTimer >= 0)//设置一个计时器让玩家在这段时间有反应间隔
        {
            turnTimer -= Time.deltaTime;
            if(turnTimer <= 0)
            {
                canFlip = true;
                canMove = true;
            }
        }
        if (checkJumpMultiplier && !Input.GetButton("Jump"))//检测是否为点按跳跃键，改进之前的检测方法
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeight);//点按时将跳跃高度降低
        }
        if (Input.GetButtonDown("Dash"))
        {
            if (Time.time >= (lastDash + dashCoolDown))
            {
                AttemptToDash();
            }
        }
    } 
    private void AttemptToDash()
    {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImagePos = transform.position.x;
    }
    private void CheckDash()
    {
        if (isDashing)
        {
            if(dashTimeLeft > 0)
            {
                canMove = false;
                canFlip = false;
                rb.velocity = new Vector2(dashSpeed * facingDirection, 0);
                dashTimeLeft -= Time.deltaTime;
                if(Mathf.Abs(transform.position.x - lastImagePos) > distanceBetweenImage)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImagePos = transform.position.x;
                }
            }
            if (dashTimeLeft <= 0 || isTouchingWall)
            {
                isDashing = false;
                canMove = true;
                canFlip = true;
            }

        }
    }
    private void CheckIfCanJump()//检测是否能进行跳跃
    {
        if (isGround && rb.velocity.y <= 0.01f)//在地面上并且不在进行跳跃
        {
            amountOfJumpLeft = amountOfJump;
        }
        if (isTouchingWall)//判断是否能跳墙
        {
            canWallJump = true;
        }
        if (amountOfJumpLeft <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }
    }
    //private void Jump()//跳跃函数
    //{
    //    if(canJump && !isWallSliding)
    //    {
    //        rb.velocity = new Vector2(rb.velocity.x, jumpforce);
    //        amountOfJumoLeft--;
    //    }
    //    else if (isWallSliding && moveInputDir == 0 && canJump)//朝向不移动时
    //    {
    //        isWallSliding = false;
    //        amountOfJumoLeft--;
    //        Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDir.x * -facingDirection, wallHopForce * wallHopDir.y);
    //        rb.AddForce(forceToAdd, ForceMode2D.Impulse);//模式设为冲击的力
    //    }
    //    else if ((isWallSliding || isTouchingWall) && moveInputDir!=0 && canJump)//朝向移动时
    //    {
    //        isWallSliding = false;
    //        amountOfJumoLeft--;
    //        Vector2 forceToAdd = new Vector2(wallHopForce * wallJumpDir.x * -moveInputDir, wallHopForce * wallJumpDir.y);
    //        rb.AddForce(forceToAdd, ForceMode2D.Impulse);//模式设为冲击的力
    //    }
    //}
    private void CheckJump()//判断是跳墙还是平常跳跃
    {
        if (jumpTimer > 0)
        {
            if (!isGround && isTouchingWall && moveInputDir != 0 && moveInputDir != facingDirection)
            {
                WallJump();//先判断执行跳墙
            }
            else if (isGround)
            {
                NormalJump();
            }
        }
        if(isAttemptingToJump)
        {
            jumpTimer -= Time.deltaTime;//执行跳跃时，将时间减少
        }
        if (wallJumpTimer > 0)//设置一段时间暂停其垂直方向移动
        {
            if(hasWallJump && moveInputDir == -lastWallJumpDirection)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                hasWallJump= false;
            }
            else if (wallJumpTimer <= 0)
            {
                hasWallJump = false;
            }
            else
            {
                wallJumpTimer-= Time.deltaTime;
            }
        }
    }
    private void NormalJump()//普通跳跃函数
    {
        if (canNormalJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpforce);
            amountOfJumpLeft--;
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }
    private void WallJump()//跳墙函数
    {
        if (canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);//保持水平方向的速度
            isWallSliding = false;
            amountOfJumpLeft = amountOfJump;
            amountOfJumpLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallJumpDir.x * -moveInputDir, wallHopForce * wallJumpDir.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);//模式设为冲击的力
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJump = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }
    private void ApplyMovement()//设置速度，控制其基础移动
    {
        if(!isGround && !isWallSliding && moveInputDir == 0 && !knockback)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }        
        else if(canMove && !knockback)
        {
            rb.velocity = new Vector2(moveInputDir * moveSpeed, rb.velocity.y);
        }
        //else if (!isGround && !isWallSliding && moveInputDir != 0)
        //{
        //    Vector2 addForce = new(movementForceInAir * moveInputDir, 0);//添加一个力
        //    rb.AddForce(addForce);

        //    if (Mathf.Abs(rb.velocity.x) > moveSpeed)//若添加的速度过大，将其设为原来的移动速度
        //    {
        //        rb.velocity = new Vector2(moveInputDir * moveSpeed, rb.velocity.y);
        //    }
        //}
        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
        //if (moveInputDir != 0)//设置其朝向
        //{
        //   rb.transform.localScale = new Vector3(moveInputDir, 1, 1);
        //}
    }
    public int GetFacingDirection()
    {
        return facingDirection;
    } 
    public bool GetDashStatus()//返回冲刺状态，用于冲刺时敌人无法伤害角色
    {
        return isDashing;
    }
    public void Knockback(int direction)
    {//设置角色击退
        knockback = true;
        knockbackStartTime = Time.time;
        rb.velocity = new Vector2(knockbackSpeed.x * direction, knockbackSpeed.y);
    }
    private void CheckKnockback()
    {
        if(Time.time >= (knockbackStartTime + knockbackDuration) && knockback)
        {
            knockback = false;
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }
    }


}
