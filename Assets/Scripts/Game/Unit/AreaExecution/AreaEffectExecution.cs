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

    public virtual void Awake()
    {
        rigibody2D = GetComponent<Rigidbody2D>();
        resourcePath = "AreaEffect/";
    }

    private void OnDisable()
    {
        fixCount = 0;
        foodUnitList.Clear();
        mouseUnitList.Clear();
        isAlive = false;
    }

    private void OnEnable()
    {
        fixCount = 0;
        foodUnitList.Clear();
        mouseUnitList.Clear();
        isAlive = true;
    }

    public void OnCollision(Collider2D collision)
    {
        // ֻ�е�һ��fixUpdateʱ���⣡
        if (fixCount > 1)
            return;

        if (collision.tag.Equals("Food"))
        {
            FoodUnit food = collision.GetComponent<FoodUnit>();
            if (IsMeetingCondition(food))
            {
                foodUnitList.Add(food);
                //EventFood(food);
            }
        }else if (collision.tag.Equals("Mouse"))
        {
            MouseUnit mouse = collision.GetComponent<MouseUnit>();
            if (IsMeetingCondition(mouse))
            {
                mouseUnitList.Add(mouse);
                //EventMouse(mouse);
            }
        }
    }

    // rigibody���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    /// <summary>
    /// �Ƿ����㱬ը�¼�����
    /// </summary>
    /// <param name="baseUnit"></param>
    /// <returns></returns>
    public virtual bool IsMeetingCondition(BaseUnit baseUnit)
    {
        return true;
    }

    /// <summary>
    /// ��ʳ��λ��ը����¼�
    /// </summary>
    public virtual void EventFood(FoodUnit unit)
    {

    }

    /// <summary>
    /// ����λ��ը����¼�
    /// </summary>
    public virtual void EventMouse(MouseUnit unit)
    {

    }

    public bool IsValid()
    {
        return isAlive && isActiveAndEnabled;
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
