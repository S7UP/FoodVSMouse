using System.Collections.Generic;
using System;
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
    public List<CharacterUnit> charcaterList = new List<CharacterUnit>();
    public bool isAlive = true;
    public bool isAffectFood;
    public bool isAffectMouse;
    public bool isAffectCharacter;
    public bool isIgnoreHeight; // 是否无视高度
    public float affectHeight; // 目标受影响的高度

    private Action<FoodUnit> EventFoodAction;
    private Action<MouseUnit> EventMouseAction;
    private Action<CharacterUnit> EventCharacterAction;

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
        isAffectCharacter = false;
        isIgnoreHeight = true; // 默认情况下无视高度
        affectHeight = 0;
        EventFoodAction = null;
        EventMouseAction = null;
        EventCharacterAction = null;
    }

    public void SetEventFoodAction(Action<FoodUnit> action)
    {
        EventFoodAction = action;
    }

    public void SetEventMouseAction(Action<MouseUnit> action)
    {
        EventMouseAction = action;
    }

    public void SetEventCharacterAction(Action<CharacterUnit> action)
    {
        EventCharacterAction = action;
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
        }else if (isAffectCharacter && collision.tag.Equals("Character"))
        {
            CharacterUnit c = collision.GetComponent<CharacterUnit>();
            if (IsMeetingCondition(c) && (isIgnoreHeight || c.GetHeight() == affectHeight))
            {
                charcaterList.Add(c);
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
        if (EventFoodAction != null)
            EventFoodAction(unit);
    }

    /// <summary>
    /// 老鼠单位的事件
    /// </summary>
    public virtual void EventMouse(MouseUnit unit)
    {
        if (EventMouseAction != null)
            EventMouseAction(unit);
    }

    /// <summary>
    /// 人物单位事件
    /// </summary>
    /// <param name="unit"></param>
    public virtual void EventCharacter(CharacterUnit unit)
    {
        if (EventCharacterAction != null)
            EventCharacterAction(unit);
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
        affectHeight = height;
        isIgnoreHeight = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    //private void Update()
    //{
    //    fixCount++;
    //}

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
                //if (EventFoodAction != null)
                //    EventFoodAction(item);
                //else
                    EventFood(item);
            }
            foreach (var item in mouseUnitList)
            {
                //if (EventMouseAction != null)
                //    EventMouseAction(item);
                //else
                    EventMouse(item);
            }
            foreach (var item in charcaterList)
            {
                //if (EventCharacterAction != null)
                //    EventCharacterAction(item);
                //else
                    EventCharacter(item);
            }
            isAlive = false;
            ExecuteRecycle();
        }
        else
        {
            fixCount++;
        }
    }

    /// <summary>
    /// 执行回收
    /// </summary>
    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, resourcePath, this.gameObject);
    }
}
