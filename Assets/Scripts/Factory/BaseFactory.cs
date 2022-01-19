using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ��Ϸ�������͵Ĺ�������
public class BaseFactory : IBaseFactory
{
    // ��ǰӵ�е�gameObject���͵���Դ(UI,UIPanel,Game) �мǣ�����ŵ�����Ϸ������Դ����Ԥ���壩
    protected Dictionary<string, GameObject> factoryDict = new Dictionary<string, GameObject>();
    // ����أ��������Ǿ����������Ϸ�������͵Ķ���ע����Ӧ����һ���������Ϸ�������

    // ������ֵ�
    protected Dictionary<string, Stack<GameObject>> objectPoolDict = new Dictionary<string, Stack<GameObject>>();

    // ����·��
    protected string loadPath;

    public BaseFactory()
    {
        loadPath = "Prefabs/";
    }

    //�������
    public void PushItem(string itemName, GameObject item)
    {
        item.SetActive(false);
        item.transform.SetParent(GameManager.Instance.transform);
        if (objectPoolDict.ContainsKey(itemName))
        {
            objectPoolDict[itemName].Push(item);
        }
        else
        {
            Debug.Log("��ǰ�ֵ�û��" +itemName+"��ջ");
        }
    }

    // ȡʵ��
    public GameObject GetItem(string itemName)
    {
        GameObject itemGo = null;
        if (objectPoolDict.ContainsKey(itemName)) //�����˶���
        {
            if (objectPoolDict[itemName].Count == 0)
            {
                // �����û������Ҳ��Ҫ�Լ�����ʵ��
                GameObject go = GetResource(itemName); // ����ȡ�����Ĳ���ʵ����ֻ��Ԥ����֮���
                itemGo = GameManager.Instance.CreateItem(go); // ��GameManagerΪԤ����ʵ����һ�£�����������ʵ��
            }
            else
            {
                itemGo = objectPoolDict[itemName].Pop(); // ��ջ
                itemGo.SetActive(true); //�ǵü���
            }
        }
        else // �������˶����
        {
            objectPoolDict.Add(itemName, new Stack<GameObject>()); // Ҫ�Ƚ���һ�����ֵĶ����
            GameObject go = GetResource(itemName); // ����ȡ�����Ĳ���ʵ����ֻ��Ԥ����֮���
            itemGo = GameManager.Instance.CreateItem(go); // ��GameManagerΪԤ����ʵ����һ�£�����������ʵ��
        }
        // ��ȫ�Լ��
        if (itemGo == null)
        {
            Debug.Log(itemName+"��ʵ����ȡʧ��");
        }
        return itemGo;
    }

    // ȡ��Դ
    private GameObject GetResource(string itemName)
    {
        GameObject itemGo = null;
        string itemLoadPath = loadPath + itemName;
        if (factoryDict.ContainsKey(itemName))
        {
            itemGo = factoryDict[itemName];
        }
        else
        {
            itemGo = Resources.Load<GameObject>(itemLoadPath);
            factoryDict.Add(itemName, itemGo);
        }
        if (itemGo == null)
        {
            Debug.Log(itemName + "����Դ��ȡʧ��");
            Debug.Log("ʧ��·����" + itemLoadPath);
        }
        return itemGo;
    }
}
