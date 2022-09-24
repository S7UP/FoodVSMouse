using UnityEngine;

/// <summary>
/// 特效对象
/// </summary>
public class BaseEffect : MonoBehaviour, IGameControllerMember
{
    public Animator animator; // 编辑器获取
    public string AppearClipName; // 出现时的动画名，编辑器获取 
    public string clipName; // 编辑器获取
    public string DisappearClipName; // 消失时的动画名，编辑器获取
    private const string resPath = "Effect/";
    public string resName = ""; // 编辑器获取


    public bool isCycle;
    public AnimatorController animatorController = new AnimatorController();
    private SpriteRenderer spriteRenderer;
    private int state = 0;

    public virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void MInit()
    {
        if(spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1, 1, 1, 1);
            SetSpriteRendererSorting("Effect", 0);
        }
            

        state = 0;
        animatorController.ChangeAnimator(animator);
        if (AppearClipName != null && !AppearClipName.Equals(""))
            animatorController.Play(AppearClipName);
        else
        {
            animatorController.Play(clipName, isCycle);
            state = 1;
        }
    }

    public void SetSpriteRendererSorting(string LayerName, int order)
    {
        spriteRenderer.sortingLayerName = LayerName;
        spriteRenderer.sortingOrder = order;
    }

    public virtual void MUpdate()
    {
        animatorController.Update();

        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            if (state == 0)
            {
                animatorController.Play(clipName, isCycle);
                state = 1;
            }
            else if (state == 1 && !isCycle)
            {
                ExecuteDeath();
            }
            else if (state == 2)
                Recycle();
        }
    }

    public void ExecuteDeath()
    {
        GameController.Instance.SetEffectDefaultParentTrans(this);
        if (DisappearClipName != null && !DisappearClipName.Equals(""))
        {
            animatorController.Play(DisappearClipName);
            state = 2;
        }
        else
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
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, resPath + resName, gameObject);
    }

    public virtual void MPause()
    {
        animatorController.Pause();
    }

    public virtual void MResume()
    {
        animatorController.Resume();
    }

    public virtual void MPauseUpdate()
    {
        
    }

    public virtual void MDestory()
    {
        
    }

    public void Hide(bool enable)
    {
        if (spriteRenderer == null)
            return;

        if (enable)
            spriteRenderer.enabled = false;
        else
            spriteRenderer.enabled = true;
    }

    public static BaseEffect CreateInstance(RuntimeAnimatorController r, string AppearClipName, string clipName, string DisappearClipName, bool isCycle)
    {
        BaseEffect e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/EffectModel").GetComponent<BaseEffect>();
        e.animator.runtimeAnimatorController = r;
        e.AppearClipName = AppearClipName;
        e.clipName = clipName;
        e.DisappearClipName = DisappearClipName;
        e.isCycle = isCycle;
        e.MInit();
        return e;
    }

    public static BaseEffect GetInstance(string resName)
    {
        BaseEffect e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, resPath + resName).GetComponent<BaseEffect>();
        e.MInit();
        return e;
    }
}
