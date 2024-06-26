using UnityEngine;
/// <summary>
/// 矩形范围效果
/// </summary>
public class RetangleAreaEffectExecution : AreaEffectExecution
{
    public int currentRowIndex; // 当前行下标
    public float offsetX;
    public float offsetY;
    public BoxCollider2D boxCollider2D;

    public override void Awake()
    {
        base.Awake();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public void Init(int currentRowIndex, float colCount, float rowCount, float offsetX, float offsetY, bool isAffectFood, bool isAffectMouse)
    {
        this.currentRowIndex = currentRowIndex;
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
        base.MInit();
    }

    public override void MUpdate()
    {
        currentRowIndex = MapManager.GetYIndex(transform.position.y);
        base.MUpdate();
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

    public static RetangleAreaEffectExecution GetInstance(Vector2 pos, Vector2 size, string CollisionLayer)
    {
        RetangleAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/RetangleAreaEffectExecution").GetComponent<RetangleAreaEffectExecution>();
        e.MInit();
        e.transform.position = pos;
        e.SetBoxCollider2D(Vector2.zero, size);
        e.SetCollisionLayer(CollisionLayer);
        return e;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/RetangleAreaEffectExecution", gameObject);
    }
}
