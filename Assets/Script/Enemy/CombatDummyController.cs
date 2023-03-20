using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDummyController : MonoBehaviour
{
    [SerializeField]
    private float maxHealth;
    [Header("击退参数")]
    [SerializeField]
    private bool applyKnockback;
    [SerializeField]
    private float knockBackspeedX, knockBackspeedY, knocbackDuration;
    [SerializeField]
    private float knockDeathSpeedX, knockDeathSpeedY, deathTorque;//deathTorque死亡扭矩，使其可以进行旋转
    private bool knockback;
    private float knockbackStart;
    [SerializeField]
    private GameObject hitParticle;//攻击特效
    //基础变量
    private int playerFacingDir;
    private bool playerOnLeft;
    private float currentHealth;
    //获取对应对象
    private PlayerController playerController;
    private GameObject alive, brokenTop, brokenBottom;
    private Rigidbody2D rbAlive, rbBrokenTop, rbBrokenBottom;
    private Animator anim;
    private void Start()
    {
        currentHealth = maxHealth;
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

        //alive = transform.Find("Alive").gameObject;
        //brokenTop = transform.Find("Broken Top").gameObject;
        //brokenBottom = transform.Find("Broken Bottom").gameObject;
        alive = transform.GetChild(0).gameObject;
        brokenTop = transform.GetChild(1).gameObject;
        brokenBottom = transform.GetChild(2).gameObject;

        anim = alive.GetComponent<Animator>();
        rbAlive = alive.GetComponent<Rigidbody2D>();
        rbBrokenTop = brokenTop.GetComponent<Rigidbody2D>();
        rbBrokenBottom = brokenBottom.GetComponent<Rigidbody2D>();  

        alive.SetActive(true);
        brokenTop.SetActive(false);
        brokenBottom.SetActive(false);
    }
    private void Update()
    {
        CheckKnockback();
    }
    private void Damage(float[] attackDetails)
    {
        currentHealth -= attackDetails[0];//该数值对应角色的伤害值，需要适当调整

        playerFacingDir = playerController.GetFacingDirection();
        //随机在一个角度生成攻击特效
        Instantiate(hitParticle, anim.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
        if (playerFacingDir == 1)
        {
            playerOnLeft = true;
        }
        else
        {
            playerOnLeft = false;
        }
        anim.SetBool("playerOnLeft", playerOnLeft);
        anim.SetTrigger("damage");

        if (applyKnockback && currentHealth > 0.0f)
        {//设置击退效果
            Knockback();
        }
        if(currentHealth <= 0.0f)
        {//死亡
            Die();
        }
    }
    private void Knockback()
    {
        knockback = true;
        knockbackStart = Time.time;
        rbAlive.velocity = new Vector2(knockBackspeedX * playerFacingDir, knockBackspeedY);
    }
    private void CheckKnockback()
    {
        if (Time.time >= (knockbackStart + knocbackDuration) && knockback)
        {//击退之后设置静止
            knockback = false;
            rbAlive.velocity = new Vector2(0.0f, rbAlive.velocity.y);
        }
    }
    private void Die()
    {
        alive.SetActive(false);
        brokenTop.SetActive(true);
        brokenBottom.SetActive(true);

        brokenTop.transform.position = alive.transform.position;
        brokenBottom.transform.position = brokenTop.transform.position;

        rbBrokenBottom.velocity = new Vector2(knockBackspeedX * playerFacingDir, knockBackspeedY);
        rbBrokenTop.velocity = new Vector2(knockDeathSpeedX * playerFacingDir, knockDeathSpeedY);
        rbBrokenTop.AddTorque(deathTorque * -playerFacingDir, ForceMode2D.Impulse);//让其在破裂时进行旋转
    }
}
