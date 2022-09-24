using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 范围效果执行体（挂在一个对象上）
/// </summary>
public abstract class AreaEffectExecution : MonoBehaviour, IGameControllerMember
{
    public int totalTime; // 总持续时间
    public int timeLeft = -1; // 剩余持续时间 默认为-1，-1表示持续时间无限

    public List<FoodUnit> foodUnitList = new List<FoodUnit>();
    public List<MouseUnit> mouseUnitList = new List<MouseUnit>();
    public List<CharacterUnit> characterList = new List<CharacterUnit>();
    public List<BaseGrid> gridList = new List<BaseGrid>();
    public bool isAlive = true;
    public bool isAffectFood;
    public bool isAffectMouse;
    public bool isAffectCharacter;
    public bool isIgnoreHeight; // 是否无视高度
    public float affectHeight; // 目标受影响的高度

    private Action<FoodUnit> OnFoodEnterAction;
    private Action<MouseUnit> OnEnemyEnterAction;
    private Action<CharacterUnit> OnCharacterEnterAction;
    private Action<BaseGrid> OnGridEnterAction;

    private Action<FoodUnit> OnFoodStayAction;
    private Action<MouseUnit> OnEnemyStayAction;
    private Action<CharacterUnit> OnCharacterStayAction;
    private Action<BaseGrid> OnGridStayAction;

    private Action<FoodUnit> OnFoodExitAction;
    private Action<MouseUnit> OnEnemyExitAction;
    private Action<CharacterUnit> OnCharacterExitAction;
    private Action<BaseGrid> OnGridExitAction;

    private Action<AreaEffectExecution> OnDestoryExtraAction;

    public virtual void Awake()
    {
        
    }

    public virtual void MInit()
    {
        totalTime = -1;
        timeLeft = -1;

        foodUnitList.Clear();
        mouseUnitList.Clear();
        characterList.Clear();
        isAlive = true;
        isAffectFood = false;
        isAffectMouse = false;
        isAffectCharacter = false;
        isIgnoreHeight = true; // 默认情况下无视高度
        affectHeight = 0;

        OnFoodEnterAction = null;
        OnEnemyEnterAction = null;
        OnCharacterEnterAction = null;
        OnGridEnterAction = null;

        OnFoodStayAction = null;
        OnEnemyStayAction = null;
        OnCharacterStayAction = null;
        OnGridStayAction = null;

        OnFoodExitAction = null;
        OnEnemyExitAction = null;
        OnCharacterExitAction = null;
        OnGridExitAction = null;

        OnDestoryExtraAction = null;
    }

    /// <summary>
    /// 设置成瞬时效果
    /// </summary>
    public void SetInstantaneous()
    {
        SetTotoalTimeAndTimeLeft(1);
    }

    /// <summary>
    /// 设置效果持续时间
    /// </summary>
    public void SetTotoalTimeAndTimeLeft(int time)
    {
        totalTime = time;
        timeLeft = time;
    }

    public void SetOnFoodEnterAction(Action<FoodUnit> action)
    {
        OnFoodEnterAction = action;
    }

    public void SetOnEnemyEnterAction(Action<MouseUnit> action)
    {
        OnEnemyEnterAction = action;
    }

    public void SetOnCharacterEnterAction(Action<CharacterUnit> action)
    {
        OnCharacterEnterAction = action;
    }

    public void SetOnGridEnterAction(Action<BaseGrid> action)
    {
        OnGridEnterAction = action;
    }

    public void SetOnFoodStayAction(Action<FoodUnit> action)
    {
        OnFoodStayAction = action;
    }

    public void SetOnEnemyStayAction(Action<MouseUnit> action)
    {
        OnEnemyStayAction = action;
    }

    public void SetOnCharacterStayAction(Action<CharacterUnit> action)
    {
        OnCharacterStayAction = action;
    }

    public void SetOnGridStayAction(Action<BaseGrid> action)
    {
        OnGridStayAction = action;
    }

    public void SetOnFoodExitAction(Action<FoodUnit> action)
    {
        OnFoodExitAction = action;
    }

    public void SetOnEnemyExitAction(Action<MouseUnit> action)
    {
        OnEnemyExitAction = action;
    }

    public void SetOnCharacterExitAction(Action<CharacterUnit> action)
    {
        OnCharacterExitAction = action;
    }
    public void SetOnGridExitAction(Action<BaseGrid> action)
    {
        OnGridExitAction = action;
    }

    public void SetOnDestoryExtraAction(Action<AreaEffectExecution> action)
    {
        OnDestoryExtraAction = action;
    }

    public void OnCollision(Collider2D collision)
    {
        if (isAffectFood && collision.tag.Equals("Food"))
        {
            FoodUnit food = collision.GetComponent<FoodUnit>();
            if (!foodUnitList.Contains(food) && IsMeetingCondition(food) && (isIgnoreHeight || food.GetHeight() == affectHeight))
            {
                foodUnitList.Add(food);
                OnFoodEnter(food);
            }
        }else if (isAffectMouse && collision.tag.Equals("Mouse"))
        {
            MouseUnit mouse = collision.GetComponent<MouseUnit>();
            if (!mouseUnitList.Contains(mouse) && IsMeetingCondition(mouse) && (isIgnoreHeight || mouse.GetHeight() == affectHeight))
            {
                mouseUnitList.Add(mouse);
                OnEnemyEnter(mouse);
            }
        }else if (isAffectCharacter && collision.tag.Equals("Character"))
        {
            CharacterUnit c = collision.GetComponent<CharacterUnit>();
            if (!characterList.Contains(c) && IsMeetingCondition(c) && (isIgnoreHeight || c.GetHeight() == affectHeight))
            {
                characterList.Add(c);
                OnCharacterEnter(c);
            }
        }else if (collision.tag.Equals("Grid"))
        {
            BaseGrid g = collision.GetComponent<BaseGrid>();
            if(!gridList.Contains(g) && IsMeetingCondition(g))
            {
                gridList.Add(g);
                OnGridEnter(g);
            }
        }
    }

    // rigibody相关
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Food"))
        {
            FoodUnit food = collision.GetComponent<FoodUnit>();
            if (foodUnitList.Contains(food))
            {
                foodUnitList.Remove(food);
                OnFoodExit(food);
            }
        }
        else if (collision.tag.Equals("Mouse"))
        {
            MouseUnit mouse = collision.GetComponent<MouseUnit>();
            if (mouseUnitList.Contains(mouse))
            {
                mouseUnitList.Remove(mouse);
                OnEnemyExit(mouse);
            }
        }
        else if (collision.tag.Equals("Character"))
        {
            CharacterUnit c = collision.GetComponent<CharacterUnit>();
            if (characterList.Contains(c))
            {
                characterList.Remove(c);
                OnCharacterExit(c);
            }
        }
        else if (collision.tag.Equals("Grid"))
        {
            BaseGrid g = collision.GetComponent<BaseGrid>();
            if (gridList.Contains(g))
            {
                gridList.Remove(g);
                OnGridExit(g);
            }
        }
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
    /// 是否满足事件条件（格子）
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    public virtual bool IsMeetingCondition(BaseGrid grid)
    {
        return true;
    }

    /// <summary>
    /// 美食单位的进入事件
    /// </summary>
    public virtual void OnFoodEnter(FoodUnit unit)
    {
        if (OnFoodEnterAction != null)
            OnFoodEnterAction(unit);
    }

    /// <summary>
    /// 老鼠单位的进入事件
    /// </summary>
    public virtual void OnEnemyEnter(MouseUnit unit)
    {
        if (OnEnemyEnterAction != null)
            OnEnemyEnterAction(unit);
    }

    /// <summary>
    /// 人物单位进入事件
    /// </summary>
    /// <param name="unit"></param>
    public virtual void OnCharacterEnter(CharacterUnit unit)
    {
        if (OnCharacterEnterAction != null)
            OnCharacterEnterAction(unit);
    }

    public virtual void OnGridEnter(BaseGrid unit)
    {
        if (OnGridEnterAction != null)
            OnGridEnterAction(unit);
    }

    /// <summary>
    /// 美食单位的持续事件
    /// </summary>
    public virtual void OnFoodStay(FoodUnit unit)
    {
        if (OnFoodStayAction != null)
            OnFoodStayAction(unit);
    }

    /// <summary>
    /// 老鼠单位的持续事件
    /// </summary>
    public virtual void OnEnemyStay(MouseUnit unit)
    {
        if (OnEnemyStayAction != null)
            OnEnemyStayAction(unit);
    }

    /// <summary>
    /// 人物单位持续事件
    /// </summary>
    /// <param name="unit"></param>
    public virtual void OnCharacterStay(CharacterUnit unit)
    {
        if (OnCharacterStayAction != null)
            OnCharacterStayAction(unit);
    }

    public virtual void OnGridStay(BaseGrid unit)
    {
        if (OnGridStayAction != null)
            OnGridStayAction(unit);
    }

    /// <summary>
    /// 美食单位的离开事件
    /// </summary>
    public virtual void OnFoodExit(FoodUnit unit)
    {
        if (OnFoodExitAction != null)
            OnFoodExitAction(unit);
    }

    /// <summary>
    /// 老鼠单位的离开事件
    /// </summary>
    public virtual void OnEnemyExit(MouseUnit unit)
    {
        if (OnEnemyExitAction != null)
            OnEnemyExitAction(unit);
    }

    /// <summary>
    /// 人物单位离开事件
    /// </summary>
    /// <param name="unit"></param>
    public virtual void OnCharacterExit(CharacterUnit unit)
    {
        if (OnCharacterExitAction != null)
            OnCharacterExitAction(unit);
    }

    public virtual void OnGridExit(BaseGrid unit)
    {
        if (OnGridExitAction != null)
            OnGridExitAction(unit);
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

    /// <summary>
    /// 更新的方法
    /// </summary>
    public virtual void MUpdate()
    {
        if(timeLeft != 0)
        {
            // 执行更新
            List<FoodUnit> f_delList = new List<FoodUnit>();
            foreach (var item in foodUnitList)
            {
                if (item.IsAlive())
                    OnFoodStay(item);
                else
                    f_delList.Add(item);
            }
            foreach (var item in f_delList)
            {
                foodUnitList.Remove(item);
            }

            List<MouseUnit> m_delList = new List<MouseUnit>();
            foreach (var item in mouseUnitList)
            {
                if (item.IsAlive())
                    OnEnemyStay(item);
                else
                    m_delList.Add(item);
            }
            foreach (var item in m_delList)
            {
                mouseUnitList.Remove(item);
            }

            List<CharacterUnit> c_delList = new List<CharacterUnit>();
            foreach (var item in characterList)
            {
                if (item.IsAlive())
                    OnCharacterStay(item);
                else
                    c_delList.Add(item);
            }
            foreach (var item in c_delList)
            {
                characterList.Remove(item);
            }

            List<BaseGrid> g_delList = new List<BaseGrid>();
            foreach (var item in gridList)
            {
                if (item.isActiveAndEnabled)
                    OnGridStay(item);
                else
                    g_delList.Add(item);
            }
            foreach (var item in g_delList)
            {
                gridList.Remove(item);
            }

            if (timeLeft > 0)
                timeLeft--;
        }
        else
        {
            MDestory();
        }
    }

    /// <summary>
    /// 执行回收
    /// </summary>
    public abstract void ExecuteRecycle();

    public void MPause()
    {
        
    }

    public void MPauseUpdate()
    {
        
    }

    public void MResume()
    {
        
    }

    public void MDestory()
    {
        isAlive = false;
        // 释放自身范围内的所有单位，执行一次OnExit
        foreach (var item in foodUnitList)
        {
            OnFoodExit(item);
        }
        foodUnitList.Clear();
        foreach (var item in mouseUnitList)
        {
            OnEnemyExit(item);
        }
        mouseUnitList.Clear();
        foreach (var item in characterList)
        {
            OnCharacterExit(item);
        }
        characterList.Clear();
        if (OnDestoryExtraAction != null)
            OnDestoryExtraAction(this);
        ExecuteRecycle();
    }

    public void SetCollisionLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }
}
