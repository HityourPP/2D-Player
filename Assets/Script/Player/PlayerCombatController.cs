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

    private float lastInputTime = Mathf.NegativeInfinity;//Mathf.NegativeInfinityΪ�������

    //�ɹ��������������
    public float attack1Radius;//������Χ
    public float attack1Damage;//�����˺�
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
        if (Input.GetMouseButtonDown(0))//��ȡ����������
        {
            if (combatEnabled)
            {//���й���
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }
    private void CheckAttacks()
    {
        if (gotInput)
        {//���ж���attack1
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
        {//�����ʱ����ȴ��µ�����
            gotInput = false;
        }
    }
    public void CheckAttackHitBox()//���ɹ��������壬��������
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
    private void OnDrawGizmos()//����������Χ
    {
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }
    private void Damage(float[] attackDetails)
    {
        int direction;
        //�˺���ɫ

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