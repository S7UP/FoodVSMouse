using UnityEngine;

/// <summary>
/// ��Ч����
/// </summary>
public class BaseEffect : MonoBehaviour, IGameControllerMember
{
    public Animator animator; // �༭����ȡ
    public string clipName; // �༭����ȡ
    private string resPath = "Effect/";
    public string resName = ""; // �༭����ȡ
    public bool isCycle;

    public virtual void Awake()
    {
        
    }

    public void OnEnable()
    {
        
    }

    public virtual void MUpdate()
    {
        if (!isCycle && AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator)-1) // �����ŵ����һ֡ʱ�˳�
        {
            Recycle();
        }
    }

    public virtual bool IsValid()
    {
        return isActiveAndEnabled;
    }

    /// <summary>
    /// ���ն���
    /// </summary>
    public virtual void Recycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, resPath + resName, this.gameObject);
    }

    public virtual void MInit()
    {
        animator.Play(clipName, 0, 0);
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

    public static BaseEffect CreateInstance(RuntimeAnimatorController r, string clipName, bool isCycle)
    {
        BaseEffect e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/EffectModel").GetComponent<BaseEffect>();
        e.animator.runtimeAnimatorController = r;
        e.clipName = clipName;
        e.isCycle = isCycle;
        return e;
    }
}
