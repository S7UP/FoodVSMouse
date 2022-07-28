using UnityEngine;
/// <summary>
/// ���η�Χ�ķ�ΧЧ��������
/// </summary>
public class RetangleAreaEffectExecution : AreaEffectExecution
{
    public int currentRowIndex; // ��ǰ���±�
    public float offsetX;
    public int offsetY;
    public float colCount; // ��Ӱ������
    public int rowCount; // ��Ӱ������
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
    /// ���պ���������
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable();
        currentRowIndex = 0;
        rowCount = 0;
    }

    /// <summary>
    /// ���жϣ����ж��Ѿ���������ײ֮���ˣ���˿��Բ�����
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
