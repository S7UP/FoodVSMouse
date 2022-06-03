using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效对象
/// </summary>
public class BaseEffect : MonoBehaviour
{
    public Animator animator; // 编辑器获取
    public string clipName; // 编辑器获取
    private string resPath = "Effect/";
    public string resName = ""; // 编辑器获取

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
        if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator)-1) // 当播放到最后一帧时退出
        {
            Recycle();
        }
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    public void Recycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, resPath + resName, this.gameObject);
    }
}
