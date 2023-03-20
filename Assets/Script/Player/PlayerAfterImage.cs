using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImage : MonoBehaviour
{
    private Transform player;

    private SpriteRenderer SR;
    private SpriteRenderer playerSR;
    private Color color;

    private float timeActivated;
    private float activeTime = 0.1f;//残影持续时间
    private float alpha;
    public float alphaSet = 25f;//控制残影透明度
    public float alphaDecay = 10f;

    private void OnEnable()
    {//获取组件以及初始化
        SR = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerSR = player.GetComponent<SpriteRenderer>();

        alpha = alphaSet;
        SR.sprite = playerSR.sprite;
        transform.position = player.position;
        transform.rotation = player.rotation;
        timeActivated= Time.time;
    }
    private void Update()
    {
        alpha -= alphaDecay * Time.deltaTime;//设置其透明度
        color = new Color(1f,1f,1f,alpha);
        SR.color = color;
        if (Time.time >= (timeActivated + activeTime))
        {//将其放回对象池，从而可以再次使用
            PlayerAfterImagePool.Instance.AddToPool(gameObject);
        }
    }

}
