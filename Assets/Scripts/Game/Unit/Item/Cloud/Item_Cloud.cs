using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

using static UnityEngine.UI.CanvasScaler;
/// <summary>
/// 云朵
/// </summary>
public class Item_Cloud : BaseItem
{
    private List<BaseUnit> unitList = new List<BaseUnit>();
    private int maxBearCount; // 最大承载单位数
    private IntModifier BearInSkyModifier = new IntModifier(1); // 高空承载
    private bool isHide;
    private float bear_rate; // 承载比例（空载时为0，满载时为1）
    private int breakTimer = 0; // 破裂动画计时器（0为完全破裂，60为完全正常）
    private int recoverTimeLeft; // 剩余恢复时间（为0则自动恢复，如果填-1则永不恢复）
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
        // 设置判定大小
        SetBoxCollider2DParam(Vector2.zero, new Vector2(0.55f*MapManager.gridWidth, 0.55f*MapManager.gridHeight));
    }

    public override void MUpdate()
    {
        timer++;
        // 检测大于0.5格的单位并释放
        foreach (var u in unitList.ToArray())
        {
            float dist = u.transform.position.x - transform.position.x;
            if (dist <= -0.5f * MapManager.gridWidth || dist > 0.5f * MapManager.gridWidth)
            {
                OnUnitExit(u);
                unitList.Remove(u);
            }
        }

        // 超过承受数直接隐藏（破裂）
        if (!isHide && unitList.Count >= maxBearCount)
        {
            Hide(60 * 24); // 这个触发了的话 isHide就会变成true
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

        // 根据承载情况来设置云朵透明度
        {
            float rate = (float)unitList.Count / maxBearCount;
            if (bear_rate < rate)
                bear_rate = Mathf.Min(rate, bear_rate + 0.01f);
            else if (bear_rate > rate)
                bear_rate = Mathf.Max(rate, bear_rate - 0.01f);
            // bear_rate = rate;

            float break_rate = (float)breakTimer / 60;
            // 云朵大小变化
            spriteRenderer.transform.localScale = Vector3.one * (0.25f + 0.75f * (1 - bear_rate)) * break_rate;
            // 云朵相对高度变化
            spriteRenderer.transform.localPosition = new Vector3(0, 0.025f*Mathf.Sin((float)timer/180*Mathf.PI) - 0.4f*bear_rate, 0);
            // 如果还剩一个就破了，则会变色提示
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
        // 为目标添加一层<高空承载>标签
        if (!unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky))
            unit.NumericBox.IntDict.Add(StringManager.BearInSky, new IntNumeric());
        unit.NumericBox.IntDict[StringManager.BearInSky].AddAddModifier(BearInSkyModifier);
    }

    private void OnUnitExit(BaseUnit unit)
    {
        // 移除目标一层<高空承载>标签
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
        // 释放其上的所有单位
        foreach (var item in unitList)
            OnUnitExit(item);
        unitList.Clear();
        // 下面这个好像暂时是空的
        base.CloseCollision();
    }

    public override void BeforeDeath()
    {
        // 释放其上的所有单位
        foreach (var item in unitList)
        {
            OnUnitExit(item);
        }
        unitList.Clear();
        base.BeforeDeath();
    }

    public override void AfterDeath()
    {
        // 释放其上的所有单位
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
    /// 获取云实例
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
    /// 返回一个云层组
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static RetangleAreaEffectExecution GetCloudGroup(int type, Vector2 pos, int count)
    {
        RetangleAreaEffectExecution e = RetangleAreaEffectExecution.GetInstance(pos, count, 1, "Enemy"); // 碰撞层设为等同于老鼠碰撞层
        e.SetBoxCollider2D(Vector2.zero, new Vector2(count * MapManager.gridWidth, 0.5f* 1 * MapManager.gridHeight));
        e.FloatDict.Add("offsetX", 0); // 用于给 风域 处理偏移量的变量
        GameController.Instance.AddAreaEffectExecution(e);

        float length = count*MapManager.gridWidth;
        float leftBoundPos = pos.x - (float)count / 2* MapManager.gridWidth; // 左边界
        float leftFadePos = leftBoundPos + MapManager.gridWidth; // 左渐出线
        float rightBoundPos = pos.x + (float)count / 2 * MapManager.gridWidth; // 右边界
        float rightFadePos = rightBoundPos - MapManager.gridWidth; // 右渐出线

        Item_Cloud[] cloudArray = new Item_Cloud[count];
        float[] offsetXArray = new float[count];
        for (int i = 0; i < cloudArray.Length; i++)
        {
            Item_Cloud c = Item_Cloud.GetInstance(type, (i % 2 == 0 ? true : false));
            cloudArray[i] = c;
            offsetXArray[i] = (i - (float)(count-1)/2)* MapManager.gridWidth;
            GameController.Instance.AddItem(c);

            // 云朵依次出现
            int timeLeft = 120/count * i;
            {
                c.Hide(0);
                c.timer = 360 / count * i;
            }
        }

        float last_offsetX = 0;
        // 产生一个总控任务
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
                        // 向右
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
                            // 向左
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
