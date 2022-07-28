using UnityEngine;
/// <summary>
/// 矩形范围的范围效果生成器
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
        boxCollider2D.offset = new Vector2(offsetX, offsetY);
        boxCollider2D.size = new Vector2(colCount * 1.05f, rowCount);
    }

    /// <summary>
    /// 回收后重置数据
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable();
        currentRowIndex = 0;
        rowCount = 0;
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
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (index == i)
                return true;
        }
        return false;
    }
}
