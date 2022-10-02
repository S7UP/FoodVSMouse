using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 基础地形类形
/// </summary>
public class BaseGridType : MonoBehaviour, IGameControllerMember
{
    public GridType mType;
    public int mShape;

    public BoxCollider2D mBoxCollider2D;
    public SpriteRenderer mSpriteRenderer;

    public List<BaseUnit> unitList = new List<BaseUnit>();

    public void Awake()
    {
        mBoxCollider2D = GetComponent<BoxCollider2D>();
        mSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetBoxCollider2D(Vector2 offset, Vector2 size)
    {
        mBoxCollider2D.offset = offset;
        mBoxCollider2D.size = size;
    }

    public virtual void MInit()
    {
        SetBoxCollider2D(Vector2.zero, new Vector2(0.51f*MapManager.gridWidth, 0.51f*MapManager.gridHeight));
        unitList.Clear();
    }

    public virtual void MUpdate()
    {

    }

    public virtual void MDestory()
    {
        foreach (var item in unitList)
        {
            OnUnitExit(item);
        }
        unitList.Clear();
        Recycle();
    }

    public virtual void MPause()
    {
        
    }

    public virtual void MPauseUpdate()
    {
        
    }

    public virtual void MResume()
    {
        
    }

    /// <summary>
    /// 当有单位进入地形时施加给单位的效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public virtual void OnUnitEnter(BaseUnit baseUnit)
    {

    }

    /// <summary>
    /// 当有单位处于地形时持续给单位的效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public virtual void OnUnitStay(BaseUnit baseUnit)
    {

    }

    /// <summary>
    /// 当有单位离开地形时施加给单位的效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public virtual void OnUnitExit(BaseUnit baseUnit)
    {

    }

    /// <summary>
    /// 是否满足进入条件
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetingEnterCondition(BaseUnit unit)
    {
        return true;
    }

    public virtual void OnCollision(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Character"))
        {
            BaseUnit u = collision.GetComponent<BaseUnit>();
            if (!unitList.Contains(u) && IsMeetingEnterCondition(u))
            {
                unitList.Add(u);
                OnUnitEnter(u);
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Character"))
        {
            BaseUnit u = collision.GetComponent<BaseUnit>();
            if (unitList.Contains(u))
            {
                unitList.Remove(u);
                OnUnitExit(u);
            }
        }
    }


    public static BaseGridType GetInstance(GridType type, int shape)
    {
        BaseGridType g = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/GridType/" + (int)type + "/" + shape).GetComponent<BaseGridType>();
        g.mType = type;
        g.mShape = shape;
        return g;
    }

    public void Recycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Grid/GridType/" + (int)mType + "/" + mShape, gameObject);
    }
}
