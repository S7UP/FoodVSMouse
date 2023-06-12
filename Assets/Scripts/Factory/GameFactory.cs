using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 专门用来取游戏内游戏物体对象
/// </summary>
public class GameFactory : BaseFactory
{
    protected const int BufferTime = 3; // 缓冲时间（游戏帧）
    /// <summary>
    /// 表明正在缓冲池倒计时的GameObject
    /// 在剩余时间到0之前会留在缓冲池，关于缓冲池下看
    /// </summary>
    protected class BufferingGameObject
    {
        public GameObject gameObject; // 被标记的GameObject
        public int timeLeft; // 剩余时间

        public void DecTimeLeft()
        {
            timeLeft--;
        }
    }

    /// <summary>
    /// 对象池缓冲区字典（每当一个对象被回收时，会先在当前帧内进入缓冲区字典，再后一帧将缓冲区的内容真正放入对象池字典，方可复用）
    /// 这一步的做法目的是，保证本帧回收的对象在本帧内不能再被复用，以防止游戏中，对象失活后被立即重新启用，重新启用时对象引用再度被放到一些专门用来管理
    /// 对象更新的表，然后表中原本有的对象引用又没被删去，导致一个对象在某表中有两个它引用，而每次遍历处理时执行两次功能，这显然是不对的
    /// </summary>
    protected Dictionary<string, Queue<BufferingGameObject>> objectPoolBufferDict = new Dictionary<string, Queue<BufferingGameObject>>();

    public GameFactory()
    {
        loadPath += "Game/";
    }

    // 放入池子（放的是缓冲区）
    public override void PushItem(string itemName, GameObject item)
    {
        item.SetActive(false);
        item.transform.SetParent(GameManager.Instance.transform);
        if (objectPoolBufferDict.ContainsKey(itemName))
        {
            // 入队
            bool flag = true;
            foreach (var buf in objectPoolBufferDict[itemName])
            {
                if(buf.gameObject == item)
                {
                    flag = false;
                    break;
                }
            }
            if(flag && !objectPoolDict[itemName].Contains(item))
                objectPoolBufferDict[itemName].Enqueue(new BufferingGameObject() { gameObject = item, timeLeft = BufferTime });
        }
        else
        {
            objectPoolBufferDict.Add(itemName, new Queue<BufferingGameObject>());
            objectPoolBufferDict[itemName].Enqueue(new BufferingGameObject() { gameObject = item, timeLeft = BufferTime });
        }
    }

    // 将缓冲区里的对象放进真对象池
    public void PushItemFromBuffer()
    {
        foreach (var i in objectPoolBufferDict)
        {
            if (objectPoolDict.ContainsKey(i.Key))
            {
                // 获取队列
                Queue<BufferingGameObject> queue = i.Value;
                // 队头的剩余时间一定是最少的，在队头取出剩余时间为0的元素，重新推回对象池
                while (queue.Count > 0 && queue.Peek().timeLeft <= 0)
                {
                    objectPoolDict[i.Key].Push(queue.Dequeue().gameObject);
                }
                // 剩下的项--
                foreach (var bufferingGameObject in queue)
                {
                    bufferingGameObject.DecTimeLeft();
                }
            }
            else
            {
                Debug.Log("对象池缓冲区中的itemName:"+ i.Key + "，在对象池中无法被找到！");
            }
        }
    }


    public override void Clear()
    {
        foreach (var keyValuePair in objectPoolBufferDict)
        {
            foreach (var item in keyValuePair.Value)
            {
                GameObject.Destroy(item.gameObject);
            }
            keyValuePair.Value.Clear();
        }
        base.Clear();
    }
    // test
}
