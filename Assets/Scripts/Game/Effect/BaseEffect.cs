using System;
using System.Collections.Generic;

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
    public TaskController taskController = new TaskController();
    private int state = 0;

    public List<Action<BaseEffect>> BeforeDeathActionList = new List<Action<BaseEffect>>();

    public virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void MInit()
    {
        taskController.Initial();
        BeforeDeathActionList.Clear();

        state = 0;
        animatorController.ChangeAnimator(animator);
        if (AppearClipName != null && !AppearClipName.Equals(""))
            animatorController.Play(AppearClipName);
        else
        {
            if(clipName!=null)
                animatorController.Play(clipName, isCycle);
            state = 1;
}

        transform.localScale = Vector2.one;
        SetSpriteRight(Vector2.right);
        spriteRenderer.color = new Color(1, 1, 1, 1);
        SetSpriteRendererSorting("Grid", 100);
    }

    public void SetSpriteRendererSorting(string LayerName, int order)
    {
        spriteRenderer.sortingLayerName = LayerName;
        spriteRenderer.sortingOrder = order;
    }

    public virtual void MUpdate()
    {
        animatorController.Update();

        if (animatorController.GetCurrentAnimatorStateRecorder()!=null && animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
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

        taskController.Update();
    }

    public void ExecuteDeath()
    {
        foreach (var action in BeforeDeathActionList)
        {
            action(this);
        }

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
        Recycle();
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

    /// <summary>
    /// 添加唯一性任务
    /// </summary>
    public void AddUniqueTask(string key, ITask t)
    {
        taskController.AddUniqueTask(key, t);
    }

    /// <summary>
    /// 添加一个任务
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        taskController.AddTask(t);
    }

    /// <summary>
    /// 移除唯一性任务
    /// </summary>
    public void RemoveUniqueTask(string key)
    {
        taskController.RemoveUniqueTask(key);
    }

    /// <summary>
    /// 移除一个任务
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        taskController.RemoveTask(t);
    }

    /// <summary>
    /// 获取某个标记为key的任务
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ITask GetTask(string key)
    {
        return taskController.GetTask(key);
    }

    public void AddBeforeDeathAction(Action<BaseEffect> action)
    {
        BeforeDeathActionList.Add(action);
    }

    public void RemoveBeforeDeathAction(Action<BaseEffect> action)
    {
        BeforeDeathActionList.Remove(action);
    }

    public void SetSpriteRight(Vector2 rot)
    {
        spriteRenderer.transform.right = rot;
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

    public static BaseEffect CreateInstance(Sprite sprite)
    {
        BaseEffect e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/EffectModel").GetComponent<BaseEffect>();
        e.animator.runtimeAnimatorController = null;
        e.AppearClipName = null;
        e.clipName = null;
        e.DisappearClipName = null;
        e.isCycle = true;
        e.spriteRenderer.sprite = sprite;
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
