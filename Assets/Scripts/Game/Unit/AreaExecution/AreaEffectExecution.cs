using System.Collections;
using System.Collections.Generic;
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
    public bool isAlive = true;
    public bool isAffectFood;
    public bool isAffectMouse;
    public bool isIgnoreHeight; // �Ƿ����Ӹ߶�
    public float affectHeight; // Ŀ����Ӱ��ĸ߶�

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
        isIgnoreHeight = true; // Ĭ����������Ӹ߶�
        affectHeight = 0;
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

    }

    /// <summary>
    /// ����λ���¼�
    /// </summary>
    public virtual void EventMouse(MouseUnit unit)
    {

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
        // OnTriggerEnter2D()��FixUpdate()��
        // ����һ��fixUpdate�������Destory����Ϊ��Ϸ��60fps����fixUpdate��50fps������������������п���δ����һ��FixUpdate�;���UpdateȻ��ֱ��ɾ��ʹ��ըЧ��ʧЧ
        if (fixCount>0)
        {
            // ����OnTriggerEnter2D()�Ƿ�����Update֮ǰ�ģ�������ֻ��Ҫһ֡������Ч��
            // �����Ϊ����ֱ�ӻ��յ��������
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
