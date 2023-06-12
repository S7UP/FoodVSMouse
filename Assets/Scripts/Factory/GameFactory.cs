using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// ר������ȡ��Ϸ����Ϸ�������
/// </summary>
public class GameFactory : BaseFactory
{
    protected const int BufferTime = 3; // ����ʱ�䣨��Ϸ֡��
    /// <summary>
    /// �������ڻ���ص���ʱ��GameObject
    /// ��ʣ��ʱ�䵽0֮ǰ�����ڻ���أ����ڻ�����¿�
    /// </summary>
    protected class BufferingGameObject
    {
        public GameObject gameObject; // ����ǵ�GameObject
        public int timeLeft; // ʣ��ʱ��

        public void DecTimeLeft()
        {
            timeLeft--;
        }
    }

    /// <summary>
    /// ����ػ������ֵ䣨ÿ��һ�����󱻻���ʱ�������ڵ�ǰ֡�ڽ��뻺�����ֵ䣬�ٺ�һ֡���������������������������ֵ䣬���ɸ��ã�
    /// ��һ��������Ŀ���ǣ���֤��֡���յĶ����ڱ�֡�ڲ����ٱ����ã��Է�ֹ��Ϸ�У�����ʧ��������������ã���������ʱ���������ٶȱ��ŵ�һЩר����������
    /// ������µı�Ȼ�����ԭ���еĶ���������û��ɾȥ������һ��������ĳ���������������ã���ÿ�α�������ʱִ�����ι��ܣ�����Ȼ�ǲ��Ե�
    /// </summary>
    protected Dictionary<string, Queue<BufferingGameObject>> objectPoolBufferDict = new Dictionary<string, Queue<BufferingGameObject>>();

    public GameFactory()
    {
        loadPath += "Game/";
    }

    // ������ӣ��ŵ��ǻ�������
    public override void PushItem(string itemName, GameObject item)
    {
        item.SetActive(false);
        item.transform.SetParent(GameManager.Instance.transform);
        if (objectPoolBufferDict.ContainsKey(itemName))
        {
            // ���
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

    // ����������Ķ���Ž�������
    public void PushItemFromBuffer()
    {
        foreach (var i in objectPoolBufferDict)
        {
            if (objectPoolDict.ContainsKey(i.Key))
            {
                // ��ȡ����
                Queue<BufferingGameObject> queue = i.Value;
                // ��ͷ��ʣ��ʱ��һ�������ٵģ��ڶ�ͷȡ��ʣ��ʱ��Ϊ0��Ԫ�أ������ƻض����
                while (queue.Count > 0 && queue.Peek().timeLeft <= 0)
                {
                    objectPoolDict[i.Key].Push(queue.Dequeue().gameObject);
                }
                // ʣ�µ���--
                foreach (var bufferingGameObject in queue)
                {
                    bufferingGameObject.DecTimeLeft();
                }
            }
            else
            {
                Debug.Log("����ػ������е�itemName:"+ i.Key + "���ڶ�������޷����ҵ���");
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
