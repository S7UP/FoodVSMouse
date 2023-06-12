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
        spriteRenderer.color = new Color(1, 1, 1, 1);
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


    public override bool TryGetSpriteRenternerSorting(out string name, out int order)
    {
        if (spriteRenderer == null)
            return base.TryGetSpriteRenternerSorting(out name, out order);
        name = spriteRenderer.sortingLayerName;
        order = spriteRenderer.sortingOrder;
        return true;
    }

    /// <summary>
    /// ����ͼ����ʾ���ж�����ƫ����
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteRenderer.transform.localPosition = vector2;
    }

    /// <summary>
    /// ��ȡ��ͼ�����������
    /// </summary>
    /// <returns></returns>
    public override Vector2 GetSpriteLocalPosition()
    {
        return spriteRenderer.transform.localPosition;
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

    /// <summary>
    /// �ɷ�ѡ��ΪĿ��
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return mBoxCollider2D.enabled && base.CanBeSelectedAsTarget(otherUnit);
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

    public override void SetAlpha(float a)
    {
        spriteRenderer.color = new Color(1, 1, 1, a);
    }
}
