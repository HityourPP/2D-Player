using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    private Animator anim;
    public bool combatEnabled;
    private bool gotInput;
    private bool isAttacking;
    private bool isFirstAttack;

    private float lastInputTime = Mathf.NegativeInfinity;//Mathf.NegativeInfinity为负无穷大

    //可攻击物体参数设置
    public float attack1Radius;//攻击范围
    public float attack1Damage;//攻击伤害
    public Transform attack1HitBoxPos;
    public LayerMask whatIsDamageable;
    public float inputTimer;
    private float[] attackDetails = new float[2];

    private PlayerController playerController;
    private PlayerStats playerStats;

    private void Start()
    {
        anim= GetComponent<Animator>();
        anim.SetBool("canAttack", combatEnabled);
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
    }
    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }
    private void CheckCombatInput()
    {
        if (Input.GetMouseButtonDown(0))//获取鼠标左键输入
        {
            if (combatEnabled)
            {//进行攻击
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }
    private void CheckAttacks()
    {
        if (gotInput)
        {//进行动画attack1
            if (!isAttacking)
            {
                gotInput = false;
                isAttacking = true;
                isFirstAttack = !isFirstAttack;
                anim.SetBool("attack1", true);
                anim.SetBool("firstAttack", isFirstAttack);
                anim.SetBool("isAttacking", isAttacking);
            }
        }
        if(Time.time >= (lastInputTime + inputTimer))
        {//如果超时，则等待新的输入
            gotInput = false;
        }
    }
    public void CheckAttackHitBox()//检测可攻击的物体，并将其损坏
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position, attack1Radius, whatIsDamageable);

        attackDetails[0] = attack1Damage;
        attackDetails[1] = transform.position.x;
        foreach (Collider2D coller in detectedObjects)
        {
            coller.transform.parent.SendMessage("Damage", attackDetails);
        }
    }
    private void FinishAttack1()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }
    private void OnDrawGizmos()//画出攻击范围
    {
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }
    private void Damage(float[] attackDetails)
    {
        int direction;
        //伤害角色

        playerStats.DecreaseHealth(attackDetails[0]);

        if (!playerController.GetDashStatus())
        {
            if (attackDetails[1] < transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
            playerController.Knockback(direction);
        }
    }
}
