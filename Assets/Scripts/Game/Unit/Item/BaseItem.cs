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
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (currentStateTimer <= 0)
        {
            return;
        }

        if (AnimatorManager.GetNormalizedTime(animator)>1.0) // ����������Ϻ����DeathEvent()
        {
            DeathEvent();
        }
    }

    /// <summary>
    /// �ɷ�ѡ��ΪĿ��
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget()
    {
        return mBoxCollider2D.enabled;
    }
}
