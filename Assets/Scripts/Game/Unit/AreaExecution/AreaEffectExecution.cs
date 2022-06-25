using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 范围效果执行体（挂在一个对象上）
/// </summary>
public class AreaEffectExecution : MonoBehaviour
{
    private Rigidbody2D rigibody2D;
    public string resourcePath;
    private int fixCount = 0;
    public List<FoodUnit> foodUnitList = new List<FoodUnit>();
    public List<MouseUnit> mouseUnitList = new List<MouseUnit>();
    public bool isAlive = true;
    public bool isAffectFood;
    public bool isAffectMouse;
    public bool isIgnoreHeight; // 是否无视高度
    public float affectHeight; // 目标受影响的高度

    public virtual void Awake()
    {
        rigibody2D = GetComponent<Rigidbody2D>();
        resourcePath = "AreaEffect/";
    }

    public virtual void OnEnable()
    {
        fixCount = 0;
        foodUnitList.Clear();
        mouseUnitList.Clear();
        isAlive = true;
        isAffectFood = false;
        isAffectMouse = false;
        isIgnoreHeight = true; // 默认情况下无视高度
        affectHeight = 0;
    }

    public void OnCollision(Collider2D collision)
    {
        // 只有第一次fixUpdate时会检测！
        if (fixCount > 1)
            return;

        if (isAffectFood && collision.tag.Equals("Food"))
        {
            FoodUnit food = collision.GetComponent<FoodUnit>();
            if (IsMeetingCondition(food) && (isIgnoreHeight || food.GetHeight()==affectHeight))
            {
                foodUnitList.Add(food);
            }
        }else if (isAffectMouse && collision.tag.Equals("Mouse"))
        {
            MouseUnit mouse = collision.GetComponent<MouseUnit>();
            if (IsMeetingCondition(mouse) && (isIgnoreHeight || mouse.GetHeight() == affectHeight))
            {
                mouseUnitList.Add(mouse);
            }
        }
    }

    // rigibody相关
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    /// <summary>
    /// 是否满足事件条件
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <returns></returns>
    public virtual bool IsMeetingCondition(BaseUnit baseUnit)
    {
        return true;
    }

    /// <summary>
    /// 美食单位的事件
    /// </summary>
    public virtual void EventFood(FoodUnit unit)
    {

    }

    /// <summary>
    /// 老鼠单位的事件
    /// </summary>
    public virtual void EventMouse(MouseUnit unit)
    {

    }

    public bool IsValid()
    {
        return isAlive && isActiveAndEnabled;
    }

    /// <summary>
    /// 设置能影响的目标高度
    /// </summary>
    public void SetAffectHeight(float height)
    {
        this.affectHeight = height;
        this.isIgnoreHeight = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        fixCount++;
    }

    public void MUpdate()
    {
        // OnTriggerEnter2D()在FixUpdate()中
        // 经过一次fixUpdate后才允许Destory，因为游戏是60fps，而fixUpdate是50fps，不经过这个过程是有可能未经过一次FixUpdate就经历Update然后直接删掉使爆炸效果失效
        if (fixCount>0)
        {
            // 由于OnTriggerEnter2D()是发生在Update之前的，而我们只需要一帧的这种效果
            // 因此认为可以直接回收掉这个对象
            foreach (var item in foodUnitList)
            {
                EventFood(item);
            }
            foreach (var item in mouseUnitList)
            {
                EventMouse(item);
            }
            isAlive = false;
            GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, resourcePath, this.gameObject);
        }
    }
}
