using UnityEngine;

/// <summary>
/// 特效对象
/// </summary>
public class BaseEffect : MonoBehaviour, IGameControllerMember
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

    public virtual void MUpdate()
    {
        if (AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator)-1) // 当播放到最后一帧时退出
        {
            Recycle();
        }
    }

    public virtual bool IsValid()
    {
        return isActiveAndEnabled;
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    public virtual void Recycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, resPath + resName, this.gameObject);
    }

    public virtual void MInit()
    {
        animator.Play(resName);
    }

    public virtual void MPause()
    {
        animator.speed = 0;
    }

    public virtual void MResume()
    {
        animator.speed = 1;
    }

    public virtual void MPauseUpdate()
    {
        
    }

    public virtual void MDestory()
    {
        
    }
}
