using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImagePool : MonoBehaviour
{
    public GameObject afterImagePrefab;

    //存储未激活的对象
    private Queue<GameObject> availableObjects= new Queue<GameObject>();

    public static PlayerAfterImagePool Instance { get; private set; }//使用单例
    private void Awake()
    {
        Instance = this;
        GrowPool();
    }
    private void GrowPool()//产生对象池
    {
        for(int i = 0; i < 10; i++)
        {
            var instanceToAdd = Instantiate(afterImagePrefab);//产生一个预制体
            instanceToAdd.transform.SetParent(transform);//将新产生的物体放在脚本挂在的物体下
            AddToPool(instanceToAdd);//将其添加到对象池中
        }
    }
    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObjects.Enqueue(instance);//将其添加到队尾
    }
    public GameObject GetFromPool()//从对象池中取出对象
    {
        if (availableObjects.Count == 0)
        {
            GrowPool();
        }
        var instance = availableObjects.Dequeue();//取队首的一个对象，并将其从队首删去
        instance.SetActive(true);
        return instance;
    }
}
