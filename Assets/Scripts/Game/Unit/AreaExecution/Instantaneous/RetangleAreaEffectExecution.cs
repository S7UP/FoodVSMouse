using UnityEngine;
/// <summary>
/// ���η�ΧЧ��
/// </summary>
public class RetangleAreaEffectExecution : AreaEffectExecution
{
    public int currentRowIndex; // ��ǰ���±�
    public float offsetX;
    public float offsetY;
    public float colCount; // ��Ӱ������
    public float rowCount; // ��Ӱ������
    public BoxCollider2D boxCollider2D;

    public override void Awake()
    {
        base.Awake();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public void Init(int currentRowIndex, float colCount, float rowCount, float offsetX, float offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.currentRowIndex = currentRowIndex;
        this.rowCount = rowCount;
        this.colCount = colCount;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.isAffectFood = isAffectFood;
        this.isAffectMouse = isAffectMouse;
        SetBoxCollider2D(new Vector2(offsetX * MapManager.gridWidth, offsetY * MapManager.gridHeight), new Vector2(colCount * MapManager.gridWidth, rowCount * MapManager.gridHeight));
    }

    public void SetBoxCollider2D(Vector2 offset, Vector2 size)
    {
        boxCollider2D.offset = offset;
        boxCollider2D.size = size;
    }


    public override void MInit()
    {
        currentRowIndex = 0;
        offsetX = 0;
        offsetY = 0;
        colCount = 0;
        rowCount = 0;
        base.MInit();
    }

    public override void MUpdate()
    {
        currentRowIndex = MapManager.GetYIndex(transform.position.y);
        base.MUpdate();
    }

    /// <summary>
    /// ���жϣ����ж��Ѿ���������ײ֮���ˣ���˿��Բ�����
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <returns></returns>
    public override bool IsMeetingCondition(BaseUnit unit)
    {
        if (unit.isDeathState)
            return false;
        //int c = (rowCount - 1) / 2;
        //int startIndex = Mathf.Max(0, currentRowIndex - c - offsetY);
        //int endIndex = Mathf.Min(MapController.yRow - 1, currentRowIndex + c - offsetY);
        //int index = baseUnit.GetRowIndex();
        //if (index >= startIndex && index <= endIndex)
        //    return true;
        //else
        //    return false;
        return true;
    }

    public static RetangleAreaEffectExecution GetInstance(Vector2 pos, float colCount, float rowCount, string CollisionLayer)
    {
        RetangleAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/RetangleAreaEffectExecution").GetComponent<RetangleAreaEffectExecution>();
        e.MInit();
        e.Init(MapManager.GetYIndex(pos.y), colCount, rowCount, 0, 0, false, false);
        e.transform.position = pos;
        e.SetCollisionLayer(CollisionLayer);
        return e;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/RetangleAreaEffectExecution", gameObject);
    }
}
