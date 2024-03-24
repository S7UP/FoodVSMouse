using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

using static UnityEngine.UI.CanvasScaler;
/// <summary>
/// �ƶ�
/// </summary>
public class Item_Cloud : BaseItem
{
    private List<BaseUnit> unitList = new List<BaseUnit>();
    private int maxBearCount; // �����ص�λ��
    private IntModifier BearInSkyModifier = new IntModifier(1); // �߿ճ���
    private bool isHide;
    private float bear_rate; // ���ر���������ʱΪ0������ʱΪ1��
    private int breakTimer = 0; // ���Ѷ�����ʱ����0Ϊ��ȫ���ѣ�60Ϊ��ȫ������
    private int recoverTimeLeft; // ʣ��ָ�ʱ�䣨Ϊ0���Զ��ָ��������-1�������ָ���
    private int timer = 0;

    private static List<FoodNameTypeMap> NoAffectFoodList = new List<FoodNameTypeMap>() 
    { 
        FoodNameTypeMap.CottonCandy
    };

    public override void MInit()
    {
        unitList.Clear();
        bear_rate = 0;
        maxBearCount = 5;
        isHide = false;
        recoverTimeLeft = 0;
        breakTimer = 0;
        base.MInit();
        // �����ж���С
        SetBoxCollider2DParam(Vector2.zero, new Vector2(0.55f*MapManager.gridWidth, 0.55f*MapManager.gridHeight));
    }

    public override void MUpdate()
    {
        timer++;
        // ������0.5��ĵ�λ���ͷ�
        foreach (var u in unitList.ToArray())
        {
            float dist = u.transform.position.x - transform.position.x;
            if (dist <= -0.5f * MapManager.gridWidth || dist > 0.5f * MapManager.gridWidth)
            {
                OnUnitExit(u);
                unitList.Remove(u);
            }
        }

        // ����������ֱ�����أ����ѣ�
        if (!isHide && unitList.Count >= maxBearCount)
        {
            Hide(60 * 24); // ��������˵Ļ� isHide�ͻ���true
        }

        if (isHide)
        {
            if (recoverTimeLeft > 0)
                recoverTimeLeft--;
            else if(recoverTimeLeft == 0)
                Show();

            if (breakTimer > 0)
                breakTimer--;
        }
        else
        {
            List<BaseUnit> delList = new List<BaseUnit>();
            foreach (var item in unitList)
                if (!item.IsAlive())
                    delList.Add(item);
            foreach (var item in delList)
                unitList.Remove(item);

            if (breakTimer < 60)
                breakTimer++;
        }

        // ���ݳ�������������ƶ�͸����
        {
            float rate = (float)unitList.Count / maxBearCount;
            if (bear_rate < rate)
                bear_rate = Mathf.Min(rate, bear_rate + 0.01f);
            else if (bear_rate > rate)
                bear_rate = Mathf.Max(rate, bear_rate - 0.01f);
            // bear_rate = rate;

            float break_rate = (float)breakTimer / 60;
            // �ƶ��С�仯
            spriteRenderer.transform.localScale = Vector3.one * (0.25f + 0.75f * (1 - bear_rate)) * break_rate;
            // �ƶ���Ը߶ȱ仯
            spriteRenderer.transform.localPosition = new Vector3(0, 0.025f*Mathf.Sin((float)timer/180*Mathf.PI) - 0.4f*bear_rate, 0);
            // �����ʣһ�������ˣ�����ɫ��ʾ
            float alpha = (0.1f + 0.9f * (1 - bear_rate)) * break_rate;
            if (unitList.Count + 1 >= maxBearCount)
                spriteRenderer.color = new Color(1, 0.8f, 0.8f, alpha);
            else
                spriteRenderer.color = new Color(1, 1, 1, alpha);
        }

        base.MUpdate();
    }

    private void OnUnitEnter(BaseUnit unit)
    {
        // ΪĿ�����һ��<�߿ճ���>��ǩ
        if (!unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky))
            unit.NumericBox.IntDict.Add(StringManager.BearInSky, new IntNumeric());
        unit.NumericBox.IntDict[StringManager.BearInSky].AddAddModifier(BearInSkyModifier);
    }

    private void OnUnitExit(BaseUnit unit)
    {
        // �Ƴ�Ŀ��һ��<�߿ճ���>��ǩ
        if (unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky))
            unit.NumericBox.IntDict[StringManager.BearInSky].RemoveAddModifier(BearInSkyModifier);
    }

    private void OnCollision(Collider2D collision)
    {
        if (isHide)
            return;

        if(collision.tag.Equals("Food"))
        {
            FoodUnit unit = collision.GetComponent<FoodUnit>();
            if(CanEnter(unit) && !NoAffectFoodList.Contains((FoodNameTypeMap)unit.mType))
            {
                unitList.Add(unit);
                OnUnitEnter(unit);
            }
        }else if (collision.tag.Equals("Mouse"))
        {
            MouseUnit unit = collision.GetComponent<MouseUnit>();
            if (CanEnter(unit) && !unit.IsBoss() && MouseManager.IsGeneralMouse(unit))
            {
                unitList.Add(unit);
                OnUnitEnter(unit);
            }
        }else if (collision.tag.Equals("Barrier"))
        {
            BaseItem unit = collision.GetComponent<BaseItem>();
            if (CanEnter(unit))
            {
                unitList.Add(unit);
                OnUnitEnter(unit);
            }
        }
    }

    private bool CanEnter(BaseUnit unit)
    {
        // return !unitList.Contains(unit) && unit.GetHeight() == 0 && unit.IsAlive() && (unit.transform.position.x - transform.position.x) <= 0.5f*MapManager.gridWidth;
        float dist = unit.transform.position.x - transform.position.x;

        return !unit.NumericBox.GetBoolNumericValue(StringManager.NoBearInSky) && !unitList.Contains(unit) && unit.GetHeight() == 0 && unit.IsAlive() && (dist > -0.5f * MapManager.gridWidth && dist <= 0.5f * MapManager.gridWidth);
    }

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
        if (collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Barrier"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            if (unitList.Contains(unit))
            {
                unitList.Remove(unit);
                OnUnitExit(unit);
            }
        }
    }

    public override void CloseCollision()
    {
        // �ͷ����ϵ����е�λ
        foreach (var item in unitList)
            OnUnitExit(item);
        unitList.Clear();
        // �������������ʱ�ǿյ�
        base.CloseCollision();
    }

    public override void BeforeDeath()
    {
        // �ͷ����ϵ����е�λ
        foreach (var item in unitList)
        {
            OnUnitExit(item);
        }
        unitList.Clear();
        base.BeforeDeath();
    }

    public override void AfterDeath()
    {
        // �ͷ����ϵ����е�λ
        foreach (var item in unitList)
            OnUnitExit(item);
        unitList.Clear();
        base.AfterDeath();
    }

    public override void OnIdleStateEnter()
    {
        
    }

    public void Show()
    {
        if (isHide)
        {
            isHide = false;
            OpenCollision();
        }
    }

    public void Hide(int recoverTimeLeft)
    {
        if (!isHide)
        {
            isHide = true;
            this.recoverTimeLeft = recoverTimeLeft;
            CloseCollision();
        }
    }

    /// <summary>
    /// ��ȡ��ʵ��
    /// </summary>
    /// <returns></returns>
    public static Item_Cloud GetInstance(int type, bool isShadow)
    {
        Item_Cloud c = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Item/5/0").GetComponent<Item_Cloud>();
        c.name = "Item_Cloud";
        c.MInit();
        c.spriteRenderer.sprite = GameManager.Instance.GetSprite("Item/5/"+type+"/"+(isShadow?0:1));
        return c;
    }

    protected override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Item/5/0", gameObject);
    }

    /// <summary>
    /// ����һ���Ʋ���
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static RetangleAreaEffectExecution GetCloudGroup(int type, Vector2 pos, int count)
    {
        RetangleAreaEffectExecution e = RetangleAreaEffectExecution.GetInstance(pos, count, 1, "Enemy"); // ��ײ����Ϊ��ͬ��������ײ��
        e.SetBoxCollider2D(Vector2.zero, new Vector2(count * MapManager.gridWidth, 0.5f* 1 * MapManager.gridHeight));
        e.FloatDict.Add("offsetX", 0); // ���ڸ� ���� ����ƫ�����ı���
        GameController.Instance.AddAreaEffectExecution(e);

        float length = count*MapManager.gridWidth;
        float leftBoundPos = pos.x - (float)count / 2* MapManager.gridWidth; // ��߽�
        float leftFadePos = leftBoundPos + MapManager.gridWidth; // �󽥳���
        float rightBoundPos = pos.x + (float)count / 2 * MapManager.gridWidth; // �ұ߽�
        float rightFadePos = rightBoundPos - MapManager.gridWidth; // �ҽ�����

        Item_Cloud[] cloudArray = new Item_Cloud[count];
        float[] offsetXArray = new float[count];
        for (int i = 0; i < cloudArray.Length; i++)
        {
            Item_Cloud c = Item_Cloud.GetInstance(type, (i % 2 == 0 ? true : false));
            cloudArray[i] = c;
            offsetXArray[i] = (i - (float)(count-1)/2)* MapManager.gridWidth;
            GameController.Instance.AddItem(c);

            // �ƶ����γ���
            int timeLeft = 120/count * i;
            {
                c.Hide(0);
                c.timer = 360 / count * i;
            }
        }

        float last_offsetX = 0;
        // ����һ���ܿ�����
        Tasker t = GameController.Instance.AddTasker(
            // Init
            delegate { 
                last_offsetX = e.FloatDict["offsetX"];
                for (int i = 0; i < cloudArray.Length; i++)
                    cloudArray[i].transform.position = pos + new Vector2(offsetXArray[i], 0);
            },
            // Update
            delegate {
                float current_offsetX = e.FloatDict["offsetX"];
                float d_offsetX = current_offsetX - last_offsetX;
                if(d_offsetX != 0)
                {
                    for (int i = 0; i < cloudArray.Length; i++)
                    {
                        cloudArray[i].transform.position += d_offsetX * Vector3.right;
                        // ����
                        if(d_offsetX > 0)
                        {
                            if (cloudArray[i].transform.position.x >= rightBoundPos)
                            {
                                cloudArray[i].transform.position -= length * Vector3.right;
                                cloudArray[i].Show();
                            }
                            else if(cloudArray[i].transform.position.x >= rightFadePos)
                            {
                                cloudArray[i].Hide(2);
                            }
                        }
                        else
                        {
                            // ����
                            if (cloudArray[i].transform.position.x <= leftBoundPos)
                            {
                                cloudArray[i].transform.position += length * Vector3.right;
                                cloudArray[i].Show();
                            }
                            else if (cloudArray[i].transform.position.x <= leftFadePos)
                            {
                                cloudArray[i].Hide(2);
                            }
                        }
                    }
                }
                last_offsetX = current_offsetX;
            },
            // EndCondition,
            delegate { return !e.isActiveAndEnabled; },
            // End
            delegate { }
            );
        return e;
    }
}
