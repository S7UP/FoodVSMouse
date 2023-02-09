using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// ��ΧЧ��ִ���壨����һ�������ϣ�
/// </summary>
public abstract class AreaEffectExecution : MonoBehaviour, IGameControllerMember
{
    public int totalTime; // �ܳ���ʱ��
    public int timeLeft = -1; // ʣ�����ʱ�� Ĭ��Ϊ-1��-1��ʾ����ʱ������

    public List<FoodUnit> foodUnitList = new List<FoodUnit>();
    public List<MouseUnit> mouseUnitList = new List<MouseUnit>();
    public List<CharacterUnit> characterList = new List<CharacterUnit>();
    public List<BaseGrid> gridList = new List<BaseGrid>();
    public List<BaseBullet> bulletList = new List<BaseBullet>();

    /// <summary>
    /// �ų����ڱ��еĵ�λ�ᱻ�ų�
    /// </summary>
    public List<FoodUnit> excludeFoodUnitList = new List<FoodUnit>();
    public List<MouseUnit> excludeMouseUnitList = new List<MouseUnit>();
    public List<CharacterUnit> excludeCharacterList = new List<CharacterUnit>();
    public List<BaseGrid> excludeGridList = new List<BaseGrid>();
    public List<BaseBullet> excludeBulletList = new List<BaseBullet>();

    public bool isAlive = true;
    public bool isAffectFood;
    public bool isAffectMouse;
    public bool isAffectCharacter;
    public bool isAffectGrid;
    public bool isAffectBullet;
    public bool isIgnoreHeight; // �Ƿ����Ӹ߶�
    public float affectHeight; // Ŀ����Ӱ��ĸ߶�

    public Dictionary<string, float> FloatDict = new Dictionary<string, float>();
    public List<string> TagList = new List<string>();

    private List<Func<FoodUnit, bool>> FoodEnterConditionFuncList = new List<Func<FoodUnit, bool>>();
    private List<Func<MouseUnit, bool>> EnemyEnterConditionFuncList = new List<Func<MouseUnit, bool>>();
    private List<Func<CharacterUnit, bool>> CharacterEnterConditionFuncList = new List<Func<CharacterUnit, bool>>();
    private List<Func<BaseGrid, bool>> GridEnterConditionFuncList = new List<Func<BaseGrid, bool>>();
    private List<Func<BaseBullet, bool>> BulletEnterConditionFuncList = new List<Func<BaseBullet, bool>>();

    private Action<FoodUnit> OnFoodEnterAction;
    private Action<MouseUnit> OnEnemyEnterAction;
    private Action<CharacterUnit> OnCharacterEnterAction;
    private Action<BaseGrid> OnGridEnterAction;
    private Action<BaseBullet> OnBulletEnterAction;

    private Action<FoodUnit> OnFoodStayAction;
    private Action<MouseUnit> OnEnemyStayAction;
    private Action<CharacterUnit> OnCharacterStayAction;
    private Action<BaseGrid> OnGridStayAction;
    private Action<BaseBullet> OnBulletStayAction;

    private Action<FoodUnit> OnFoodExitAction;
    private Action<MouseUnit> OnEnemyExitAction;
    private Action<CharacterUnit> OnCharacterExitAction;
    private Action<BaseGrid> OnGridExitAction;
    private Action<BaseBullet> OnBulletExitAction;

    private Action<AreaEffectExecution> OnDestoryExtraAction;

    public TaskController taskController = new TaskController();

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
        gridList.Clear();
        bulletList.Clear();

        excludeFoodUnitList.Clear();
        excludeMouseUnitList.Clear();
        excludeCharacterList.Clear();
        excludeGridList.Clear();
        excludeBulletList.Clear();

        isAlive = true;
        isAffectFood = false;
        isAffectMouse = false;
        isAffectCharacter = false;
        isAffectGrid = false;
        isAffectBullet = false;
        isIgnoreHeight = true; // Ĭ����������Ӹ߶�
        affectHeight = 0;

        FoodEnterConditionFuncList.Clear();
        EnemyEnterConditionFuncList.Clear();
        CharacterEnterConditionFuncList.Clear();
        GridEnterConditionFuncList.Clear();
        BulletEnterConditionFuncList.Clear();

        OnFoodEnterAction = null;
        OnEnemyEnterAction = null;
        OnCharacterEnterAction = null;
        OnGridEnterAction = null;
        OnBulletEnterAction = null;

        OnFoodStayAction = null;
        OnEnemyStayAction = null;
        OnCharacterStayAction = null;
        OnGridStayAction = null;
        OnBulletStayAction = null;

        OnFoodExitAction = null;
        OnEnemyExitAction = null;
        OnCharacterExitAction = null;
        OnGridExitAction = null;
        OnBulletExitAction = null;

        OnDestoryExtraAction = null;

        FloatDict.Clear();
        TagList.Clear();

        taskController.Initial();

        transform.right = Vector2.right;
    }

    /// <summary>
    /// ���ó�˲ʱЧ��
    /// </summary>
    public void SetInstantaneous()
    {
        SetTotoalTimeAndTimeLeft(1);
    }

    /// <summary>
    /// ����Ч������ʱ��
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

    public void SetOnBulletEnterAction(Action<BaseBullet> action)
    {
        OnBulletEnterAction = action;
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

    public void SetOnBulletStayAction(Action<BaseBullet> action)
    {
        OnBulletStayAction = action;
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
    public void SetOnBulletExitAction(Action<BaseBullet> action)
    {
        OnBulletExitAction = action;
    }

    public void SetOnDestoryExtraAction(Action<AreaEffectExecution> action)
    {
        OnDestoryExtraAction = action;
    }

    public virtual void OnCollision(Collider2D collision)
    {
        if (isAffectFood && collision.tag.Equals("Food"))
        {
            FoodUnit food = collision.GetComponent<FoodUnit>();
            if (IsMeetingCondition(food))
            {
                foodUnitList.Add(food);
                OnFoodEnter(food);
            }
        }else if (isAffectMouse && collision.tag.Equals("Mouse"))
        {
            MouseUnit mouse = collision.GetComponent<MouseUnit>();
            if (IsMeetingCondition(mouse))
            {
                mouseUnitList.Add(mouse);
                OnEnemyEnter(mouse);
            }
        }else if (isAffectCharacter && collision.tag.Equals("Character"))
        {
            CharacterUnit c = collision.GetComponent<CharacterUnit>();
            if (IsMeetingCondition(c))
            {
                characterList.Add(c);
                OnCharacterEnter(c);
            }
        }else if (isAffectGrid && collision.tag.Equals("Grid"))
        {
            BaseGrid g = collision.GetComponent<BaseGrid>();
            if(IsMeetingCondition(g))
            {
                gridList.Add(g);
                OnGridEnter(g);
            }
        }
        else if (isAffectBullet && collision.tag.Equals("Bullet"))
        {
            BaseBullet b = collision.GetComponent<BaseBullet>();
            if (IsMeetingCondition(b))
            {
                bulletList.Add(b);
                OnBulletEnter(b);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public virtual void OnExit(Collider2D collision)
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
        else if (collision.tag.Equals("Bullet"))
        {
            BaseBullet b = collision.GetComponent<BaseBullet>();
            if (bulletList.Contains(b))
            {
                bulletList.Remove(b);
                OnBulletExit(b);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        OnExit(collision);
    }

    private bool IsMeetingCondition(FoodUnit u)
    {
        if (foodUnitList.Contains(u) || excludeFoodUnitList.Contains(u) || !(isIgnoreHeight || u.GetHeight() == affectHeight))
            return false;
        // ����һ��������Food�����ǿɱ��������͵�
        if (!FoodManager.IsAttackableFoodType(u))
            return false;
        foreach (var func in FoodEnterConditionFuncList)
        {
            if (!func(u))
                return false;
        }
        return true;
    }

    private bool IsMeetingCondition(MouseUnit u)
    {
        if (mouseUnitList.Contains(u) || excludeMouseUnitList.Contains(u) || !(isIgnoreHeight || u.GetHeight() == affectHeight))
            return false;

        foreach (var func in EnemyEnterConditionFuncList)
        {
            if (!func(u))
                return false;
        }
        return true;
    }

    private bool IsMeetingCondition(CharacterUnit u)
    {
        if (characterList.Contains(u) || excludeCharacterList.Contains(u) || !(isIgnoreHeight || u.GetHeight() == affectHeight))
            return false;

        foreach (var func in CharacterEnterConditionFuncList)
        {
            if (!func(u))
                return false;
        }
        return true;
    }

    /// <summary>
    /// �Ƿ������¼����������ӣ�
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    private bool IsMeetingCondition(BaseGrid grid)
    {
        if (gridList.Contains(grid) || excludeGridList.Contains(grid))
            return false;

        foreach (var func in GridEnterConditionFuncList)
        {
            if (!func(grid))
                return false;
        }
        return true;
    }

    /// <summary>
    /// �Ƿ������¼��������ӵ���
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    private bool IsMeetingCondition(BaseBullet bullet)
    {
        if (bulletList.Contains(bullet) || excludeBulletList.Contains(bullet))
            return false;

        foreach (var func in BulletEnterConditionFuncList)
        {
            if (!func(bullet))
                return false;
        }
        return true;
    }

    public void AddFoodEnterConditionFunc(Func<FoodUnit, bool> func)
    {
        FoodEnterConditionFuncList.Add(func);
    }

    public void AddEnemyEnterConditionFunc(Func<MouseUnit, bool> func)
    {
        EnemyEnterConditionFuncList.Add(func);
    }

    public void AddCharacterEnterConditionFunc(Func<CharacterUnit, bool> func)
    {
        CharacterEnterConditionFuncList.Add(func);
    }

    public void AddGridEnterConditionFunc(Func<BaseGrid, bool> func)
    {
        GridEnterConditionFuncList.Add(func);
    }

    public void AddBulletEnterConditionFunc(Func<BaseBullet, bool> func)
    {
        BulletEnterConditionFuncList.Add(func);
    }

    public void RemoveFoodEnterConditionFunc(Func<FoodUnit, bool> func)
    {
        FoodEnterConditionFuncList.Remove(func);
    }

    public void RemoveEnemyEnterConditionFunc(Func<MouseUnit, bool> func)
    {
        EnemyEnterConditionFuncList.Remove(func);
    }

    public void RemoveCharacterEnterConditionFunc(Func<CharacterUnit, bool> func)
    {
        CharacterEnterConditionFuncList.Remove(func);
    }

    public void RemoveGridEnterConditionFunc(Func<BaseGrid, bool> func)
    {
        GridEnterConditionFuncList.Remove(func);
    }

    public void RemoveBulletEnterConditionFunc(Func<BaseBullet, bool> func)
    {
        BulletEnterConditionFuncList.Remove(func);
    }

    /// <summary>
    /// ��ʳ��λ�Ľ����¼�
    /// </summary>
    public virtual void OnFoodEnter(FoodUnit unit)
    {
        if (OnFoodEnterAction != null)
            OnFoodEnterAction(unit);
    }

    /// <summary>
    /// ����λ�Ľ����¼�
    /// </summary>
    public virtual void OnEnemyEnter(MouseUnit unit)
    {
        if (OnEnemyEnterAction != null)
            OnEnemyEnterAction(unit);
    }

    /// <summary>
    /// ���ﵥλ�����¼�
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

    public virtual void OnBulletEnter(BaseBullet unit)
    {
        if (OnBulletEnterAction != null)
            OnBulletEnterAction(unit);
    }

    /// <summary>
    /// ��ʳ��λ�ĳ����¼�
    /// </summary>
    public virtual void OnFoodStay(FoodUnit unit)
    {
        if (OnFoodStayAction != null)
            OnFoodStayAction(unit);
    }

    /// <summary>
    /// ����λ�ĳ����¼�
    /// </summary>
    public virtual void OnEnemyStay(MouseUnit unit)
    {
        if (OnEnemyStayAction != null)
            OnEnemyStayAction(unit);
    }

    /// <summary>
    /// ���ﵥλ�����¼�
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

    public virtual void OnBulletStay(BaseBullet unit)
    {
        if (OnBulletStayAction != null)
            OnBulletStayAction(unit);
    }

    /// <summary>
    /// ��ʳ��λ���뿪�¼�
    /// </summary>
    public virtual void OnFoodExit(FoodUnit unit)
    {
        if (OnFoodExitAction != null)
            OnFoodExitAction(unit);
    }

    /// <summary>
    /// ����λ���뿪�¼�
    /// </summary>
    public virtual void OnEnemyExit(MouseUnit unit)
    {
        if (OnEnemyExitAction != null)
            OnEnemyExitAction(unit);
    }

    /// <summary>
    /// ���ﵥλ�뿪�¼�
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

    public virtual void OnBulletExit(BaseBullet unit)
    {
        if (OnBulletExitAction != null)
            OnBulletExitAction(unit);
    }

    public bool IsValid()
    {
        return isAlive && isActiveAndEnabled;
    }

    /// <summary>
    /// ������Ӱ���Ŀ��߶�
    /// </summary>
    public void SetAffectHeight(float height)
    {
        affectHeight = height;
        isIgnoreHeight = false;
    }

    /// <summary>
    /// ���µķ���
    /// </summary>
    public virtual void MUpdate()
    {
        if(timeLeft != 0)
        {
            // ִ�и���
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
            f_delList.Clear();
            foreach (var item in excludeFoodUnitList)
            {
                if (!item.IsAlive())
                    f_delList.Add(item);
            }
            foreach (var item in f_delList)
            {
                excludeFoodUnitList.Remove(item);
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
            m_delList.Clear();
            foreach (var item in excludeMouseUnitList)
            {
                if (!item.IsAlive())
                    m_delList.Add(item);
            }
            foreach (var item in m_delList)
            {
                excludeMouseUnitList.Remove(item);
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
            c_delList.Clear();
            foreach (var item in excludeCharacterList)
            {
                if (!item.IsAlive())
                    c_delList.Add(item);
            }
            foreach (var item in c_delList)
            {
                excludeCharacterList.Remove(item);
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
            g_delList.Clear();
            foreach (var item in excludeGridList)
            {
                if (!item.isActiveAndEnabled)
                    g_delList.Add(item);
            }
            foreach (var item in g_delList)
            {
                excludeGridList.Remove(item);
            }

            List<BaseBullet> b_delList = new List<BaseBullet>();
            foreach (var item in bulletList)
            {
                if (item.isActiveAndEnabled)
                    OnBulletStay(item);
                else
                    b_delList.Add(item);
            }
            foreach (var item in b_delList)
            {
                bulletList.Remove(item);
            }
            b_delList.Clear();
            foreach (var item in excludeBulletList)
            {
                if (!item.IsAlive())
                    b_delList.Add(item);
            }
            foreach (var item in b_delList)
            {
                excludeBulletList.Remove(item);
            }

            taskController.Update();

            if (timeLeft > 0)
                timeLeft--;
        }
        else
        {
            MDestory();
        }
    }

    /// <summary>
    /// ִ�л���
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
        // �ͷ�����Χ�ڵ����е�λ��ִ��һ��OnExit
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
        foreach (var item in gridList)
        {
            OnGridExit(item);
        }
        gridList.Clear();
        foreach (var item in bulletList)
        {
            OnBulletExit(item);
        }
        bulletList.Clear();
        if (OnDestoryExtraAction != null)
            OnDestoryExtraAction(this);
        ExecuteRecycle();
    }

    public void SetCollisionLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    /// <summary>
    /// �����Ҫ���ų�����ʳ��λ
    /// </summary>
    /// <param name="unit"></param>
    public void AddExcludeFoodUnit(FoodUnit unit)
    {
        excludeFoodUnitList.Add(unit);
        if (foodUnitList.Contains(unit))
        {
            OnFoodExit(unit);
            foodUnitList.Remove(unit);
        }
    }

    /// <summary>
    /// �����Ҫ���ų�������λ
    /// </summary>
    /// <param name="unit"></param>
    public void AddExcludeMouseUnit(MouseUnit unit)
    {
        excludeMouseUnitList.Add(unit);
        if (mouseUnitList.Contains(unit))
        {
            OnEnemyExit(unit);
            mouseUnitList.Remove(unit);
        }
    }

    /// <summary>
    /// �����Ҫ���ų������ﵥλ
    /// </summary>
    /// <param name="unit"></param>
    public void AddExcludeCharacterUnit(CharacterUnit unit)
    {
        excludeCharacterList.Add(unit);
        if (characterList.Contains(unit))
        {
            OnCharacterExit(unit);
            characterList.Remove(unit);
        }
    }

    /// <summary>
    /// �����Ҫ���ų��ĸ��ӵ�λ
    /// </summary>
    /// <param name="unit"></param>
    public void AddExcludeGrid(BaseGrid g)
    {
        excludeGridList.Add(g);
        if (gridList.Contains(g))
        {
            OnGridExit(g);
            gridList.Remove(g);
        }
    }

    /// <summary>
    /// �����Ҫ���ų����ӵ���λ
    /// </summary>
    /// <param name="unit"></param>
    public void AddExcludeBullet(BaseBullet b)
    {
        excludeBulletList.Add(b);
        if (bulletList.Contains(b))
        {
            OnBulletExit(b);
            bulletList.Remove(b);
        }
    }

    /// <summary>
    /// ���Ψһ������
    /// </summary>
    public void AddUniqueTask(string key, ITask t)
    {
        taskController.AddUniqueTask(key, t);
    }

    /// <summary>
    /// ���һ������
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        taskController.AddTask(t);
    }

    /// <summary>
    /// �Ƴ�Ψһ������
    /// </summary>
    public void RemoveUniqueTask(string key)
    {
        taskController.RemoveUniqueTask(key);
    }

    /// <summary>
    /// �Ƴ�һ������
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        taskController.RemoveTask(t);
    }

    /// <summary>
    /// ��ȡĳ�����Ϊkey������
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ITask GetTask(string key)
    {
        return taskController.GetTask(key);
    }
}
