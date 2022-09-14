using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// ��ΧЧ��ִ���壨����һ�������ϣ�
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
    public bool isIgnoreHeight; // �Ƿ����Ӹ߶�
    public float affectHeight; // Ŀ����Ӱ��ĸ߶�

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
        isIgnoreHeight = true; // Ĭ����������Ӹ߶�
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
        // ֻ�е�һ��fixUpdateʱ���⣡
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

    // rigibody���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    /// <summary>
    /// �Ƿ������¼�����
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <returns></returns>
    public virtual bool IsMeetingCondition(BaseUnit baseUnit)
    {
        return true;
    }

    /// <summary>
    /// ��ʳ��λ���¼�
    /// </summary>
    public virtual void EventFood(FoodUnit unit)
    {
        if (EventFoodAction != null)
            EventFoodAction(unit);
    }

    /// <summary>
    /// ����λ���¼�
    /// </summary>
    public virtual void EventMouse(MouseUnit unit)
    {
        if (EventMouseAction != null)
            EventMouseAction(unit);
    }

    /// <summary>
    /// ���ﵥλ�¼�
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
    /// ������Ӱ���Ŀ��߶�
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
        // OnTriggerEnter2D()��FixUpdate()��
        // ����һ��fixUpdate�������Destory����Ϊ��Ϸ��60fps����fixUpdate��50fps������������������п���δ����һ��FixUpdate�;���UpdateȻ��ֱ��ɾ��ʹ��ըЧ��ʧЧ
        if (fixCount>0)
        {
            // ����OnTriggerEnter2D()�Ƿ�����Update֮ǰ�ģ�������ֻ��Ҫһ֡������Ч��
            // �����Ϊ����ֱ�ӻ��յ��������
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
    /// ִ�л���
    /// </summary>
    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, resourcePath, this.gameObject);
    }
}
