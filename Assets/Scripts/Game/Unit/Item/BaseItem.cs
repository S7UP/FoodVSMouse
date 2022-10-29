using UnityEngine;

public class BaseItem : BaseUnit
{
    // �������ڸ��ӣ���ǰ����Ӧ�����и��ӵ����ã�������
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
        // ������������animator
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
    /// ���Լ��������
    /// </summary>
    /// <param name="grid"></param>
    public override void SetGrid(BaseGrid grid)
    {
        mGrid = grid;
    }

    /// <summary>
    /// �������Ƴ�������
    /// </summary>
    public virtual void RemoveFromGrid()
    {
        mGrid.RemoveItemUnit(this);
    }

    // �����󣬽�������Ϣ�Ӷ�Ӧ�����Ƴ���ͬʱ�Ƴ��¼�
    public override void AfterDeath()
    {
        if(mGrid!=null)
            RemoveFromGrid();
    }

    /// <summary>
    /// ������ͬ������˵���Ⱦ�㼶
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, GetRowIndex(), -1, arrayIndex);
    }

    /// <summary>
    /// ����ͼ����ʾ���ж�����ƫ����
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
    /// ��������
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
    /// ˤ������˲��
    /// </summary>
    public override void OnDropStateEnter()
    {
        // ��ֹ���Ŷ���
        PauseCurrentAnimatorState(boolModifier);
    }

    /// <summary>
    /// ˤ����������
    /// </summary>
    public override void OnDropState(float r)
    {
        SetAlpha(1-r);
        spriteRenderer.transform.localPosition = spriteRenderer.transform.localPosition + 0.25f * MapManager.gridHeight * r * Vector3.down;
        spriteRenderer.transform.localScale = Vector3.one * (1 - r);
        // ����1�Ϳ��Ի�����
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
    /// �ɷ�ѡ��ΪĿ��
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget()
    {
        return mBoxCollider2D.enabled && base.CanBeSelectedAsTarget();
    }

    /// <summary>
    /// ��ȡ��ͼ����
    /// </summary>
    public override Sprite GetSpirte()
    {
        return spriteRenderer.sprite;
    }

    /// <summary>
    /// ��ȡSpriterRenderer
    /// </summary>
    /// <returns></returns>
    public override SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    /// <summary>
    /// �ɷ�ǿ���Ƴ�
    /// </summary>
    /// <returns></returns>
    public virtual bool CanRemove()
    {
        return false;
    }

    /// <summary>
    /// �����ж�
    /// </summary>
    public override void OpenCollision()
    {
        if(mBoxCollider2D!=null)
            mBoxCollider2D.enabled = true;
    }

    /// <summary>
    /// �ر��ж�
    /// </summary>
    public override void CloseCollision()
    {
        if(mBoxCollider2D!=null)
            mBoxCollider2D.enabled = false;
    }
}
