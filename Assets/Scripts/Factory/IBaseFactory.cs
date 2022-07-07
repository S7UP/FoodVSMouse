using UnityEngine;

// ��Ϸ���幤���ӿ�
public interface IBaseFactory
{
    GameObject GetItem(string itemName);

    void PushItem(string itemName, GameObject item);

    void Clear();
}
