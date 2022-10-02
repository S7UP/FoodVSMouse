using UnityEngine;
/// <summary>
/// 矩形范围效果
/// </summary>
public class RetangleAreaEffectExecution : AreaEffectExecution
{
    public int currentRowIndex; // 当前行下标
    public float offsetX;
    public int offsetY;
    public float colCount; // 受影响列数
    public int rowCount; // 受影响行数
    public BoxCollider2D boxCollider2D;

    public override void Awake()
    {
        base.Awake();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public void Init(int currentRowIndex, float colCount, int rowCount, float offsetX, int offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.currentRowIndex = currentRowIndex;
        this.rowCount = rowCount;
        this.colCount = colCount;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        this.isAffectFood = isAffectFood;
        this.isAffectMouse = isAffectMouse;
        SetBoxCollider2D(new Vector2(offsetX * MapManager.gridWidth, offsetY * MapManager.gridHeight), new Vector2(colCount * 1.05f * MapManager.gridWidth, rowCount * MapManager.gridHeight));
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
    /// 行判断（列判断已经包含在碰撞之中了，因此可以不做）
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <returns></returns>
    public override bool IsMeetingCondition(BaseUnit baseUnit)
    {
        if (baseUnit.isDeathState)
            return false;
        int c = (rowCount - 1) / 2;
        int startIndex = Mathf.Max(0, currentRowIndex - c - offsetY);
        int endIndex = Mathf.Min(MapController.yRow - 1, currentRowIndex + c - offsetY);
        int index = baseUnit.GetRowIndex();
        if (index >= startIndex && index <= endIndex)
            return true;
        else
            return false;
    }

    public static RetangleAreaEffectExecution GetInstance(Vector2 pos, int currentRowIndex, float colCount, int rowCount, float offsetX, int offsetY)
    {
        RetangleAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/RetangleAreaEffectExecution").GetComponent<RetangleAreaEffectExecution>();
        e.MInit();
        e.Init(currentRowIndex, colCount, rowCount, offsetX, offsetY, false, false);
        e.transform.position = pos;
        e.SetCollisionLayer("Default");
        return e;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/RetangleAreaEffectExecution", gameObject);
    }
}
