using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ч����
/// </summary>
public class BaseEffect : MonoBehaviour
{
    public Animator animator; // �༭����ȡ
    public string clipName; // �༭����ȡ
    private string resPath = "Effect/";
    public string resName = ""; // �༭����ȡ

    public virtual void Awake()
    {
        
    }

    public void OnEnable()
    {
        
    }

    public void InIt()
    {
        animator.Play(resName);
    }

    public void MUpdate()
    {
        if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator)-1) // �����ŵ����һ֡ʱ�˳�
        {
            Recycle();
        }
    }

    /// <summary>
    /// ���ն���
    /// </summary>
    public void Recycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, resPath + resName, this.gameObject);
    }
}
