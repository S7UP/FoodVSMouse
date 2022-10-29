using UnityEngine;

public class BaseItem : BaseUnit
{
    // 若依附于格子，则当前对象应当持有格子的引用，否则无
    public BaseGrid mGrid;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public BoxCollider2D mBoxCollider2D;

    public override void Awake()
    {
        base.Awake();
        spriteRenderer = transform.Find("SpriteGo").GetComponent<SpriteRenderer>();
        animator = transform.Find("SpriteGo").GetComponent<Animator>();
        mBoxCollider2D = transform.GetComponent<BoxCollider2D>();
    }

    public override void MInit()
    {
        base.MInit();
        // 动画控制器绑定animator
        animatorController.ChangeAnimator(animator);
        mGrid = null;
        //SetBoxCollider2DParam(Vector2.zero, new Vector2(0.5f*MapManager.gridWidth, 0.5f*MapManager.gridHeight));
    }

    public void SetBoxCollider2DParam(Vector2 offset, Vector2 size)
    {
        mBoxCollider2D.offset = offset;
        mBoxCollider2D.size = size;
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Item;
    }

    public override BaseGrid GetGrid()
    {
        return mGrid;
    }

    /// <summary>
    /// 把自己加入格子
    /// </summary>
    /// <param name="grid"></param>
    public override void SetGrid(BaseGrid grid)
    {
        mGrid = grid;
    }

    /// <summary>
    /// 将自身移除出格子
    /// </summary>
    public virtual void RemoveFromGrid()
    {
        mGrid.RemoveItemUnit(this);
    }

    // 死亡后，将自身信息从对应格子移除，同时移除事件
    public override void AfterDeath()
    {
        if(mGrid!=null)
            RemoveFromGrid();
    }

    /// <summary>
    /// 设置在同种类敌人的渲染层级
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, GetRowIndex(), -1, arrayIndex);
    }

    /// <summary>
    /// 设置图像显示与判定中心偏移量
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteRenderer.transform.localPosition = vector2;
    }

    public override void OnDieStateEnter()
    {
        animatorController.Play("Die");
    }

    /// <summary>
    /// 死亡动画
    /// </summary>
    public override void DuringDeath()
    {
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            DeathEvent();
        }
    }

    private BoolModifier boolModifier = new BoolModifier(true);

    /// <summary>
    /// 摔落死亡瞬间
    /// </summary>
    public override void OnDropStateEnter()
    {
        // 禁止播放动画
        PauseCurrentAnimatorState(boolModifier);
    }

    /// <summary>
    /// 摔落死亡过程
    /// </summary>
    public override void OnDropState(float r)
    {
        SetAlpha(1-r);
        spriteRenderer.transform.localPosition = spriteRenderer.transform.localPosition + 0.25f * MapManager.gridHeight * r * Vector3.down;
        spriteRenderer.transform.localScale = Vector3.one * (1 - r);
        // 超过1就可以回收了
        if (r >= 1.0)
        {
            ResumeCurrentAnimatorState(boolModifier);
            SetAlpha(1);
            spriteRenderer.transform.localPosition = Vector3.zero;
            spriteRenderer.transform.localScale = Vector3.one;
            DeathEvent();
        }
    }

    /// <summary>
    /// 可否被选择为目标
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget()
    {
        return mBoxCollider2D.enabled && base.CanBeSelectedAsTarget();
    }

    /// <summary>
    /// 获取贴图对象
    /// </summary>
    public override Sprite GetSpirte()
    {
        return spriteRenderer.sprite;
    }

    /// <summary>
    /// 获取SpriterRenderer
    /// </summary>
    /// <returns></returns>
    public override SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    /// <summary>
    /// 可否被强制移除
    /// </summary>
    /// <returns></returns>
    public virtual bool CanRemove()
    {
        return false;
    }

    /// <summary>
    /// 启动判定
    /// </summary>
    public override void OpenCollision()
    {
        if(mBoxCollider2D!=null)
            mBoxCollider2D.enabled = true;
    }

    /// <summary>
    /// 关闭判定
    /// </summary>
    public override void CloseCollision()
    {
        if(mBoxCollider2D!=null)
            mBoxCollider2D.enabled = false;
    }
}
