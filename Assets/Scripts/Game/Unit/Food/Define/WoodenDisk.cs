using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 木盘子
/// </summary>
public class WoodenDisk : FoodUnit, IInWater
{
    private const string TaskKey = "木盘子提供的悬浮效果";
    private BoolModifier IgnoreWaterModifier = new BoolModifier(true); // 免疫水蚀修饰器
    private List<BaseUnit> unitList = new List<BaseUnit>(); // 受影响的单位表 
    // 对以下表中的老鼠类型木盘子作用不生效
    private List<MouseNameTypeMap> noEffectMouseList = new List<MouseNameTypeMap>() 
    { 
        MouseNameTypeMap.SubmarineMouse,
        MouseNameTypeMap.RowboatMouse,
    };


    public override void MInit()
    {
        unitList.Clear();
        base.MInit();
        // 免疫水蚀效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, IgnoreWaterModifier);
        typeAndShapeToLayer = -1;
        // 添加水波纹效果
        EffectManager.AddWaterWaveEffectToUnit(this);
    }

    public override void UpdateAttributeByLevel()
    {
        SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    public override void LoadSkillAbility()
    {
        
    }

    public override void MUpdate()
    {
        // 剔除已经无效的单位
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var item in unitList)
        {
            if (!item.IsValid())
                delList.Add(item);
        }
        foreach (var item in delList)
        {
            unitList.Remove(item);
        }
        base.MUpdate();
    }

    private void OnCollision(Collider2D collision)
    {
        if(collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Item") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            // 检查目标是否不能生效
            if (unit is MouseUnit && noEffectMouseList.Contains((MouseNameTypeMap)unit.mType) && unit.GetHeight()!=0)
                return;

            if (!unitList.Contains(unit))
            {
                unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, IgnoreWaterModifier);
                // 使目标强制出水（强制出水的方法里包含了使水蚀失效）
                StatusAbility s = unit.GetUniqueStatus(StringManager.WaterGridState);
                if (s != null && s is WaterStatusAbility)
                {
                    WaterStatusAbility s2 = s as WaterStatusAbility;
                    s2.LetMasterExitWater();
                }
                // 提供 悬浮提供者 效果
                ITask task = unit.GetTask(TaskKey);
                FloatTask floatTask = null;
                if(task != null && task is FloatTask)
                {
                    floatTask = task as FloatTask;
                }
                else
                {
                    float h = 0.2f;
                    if (unit is FoodUnit || unit is CharacterUnit)
                        h = 0.2f;
                    floatTask = new FloatTask(unit, h);
                    unit.AddUniqueTask(TaskKey, floatTask);
                }
                floatTask.AddMaster(this);
                // 把目标对象加入自己承载的表中
                unitList.Add(unit);
            } 
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        // OnCollision(collision);
    }

    private void OnUnitExit(BaseUnit unit)
    {
        if (unitList.Contains(unit))
        {
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, IgnoreWaterModifier);
            // 当不包含免疫水蚀标签时，使得水蚀生效
            //StatusAbility s = unit.GetUniqueStatus(StringManager.WaterGridState);
            //if (s != null && !unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
            //    s.SetEffectEnable(true);
            // 移除当前 悬浮提供者 效果
            ITask task = unit.GetTask(TaskKey);
            if (task != null && task is FloatTask)
            {
                FloatTask floatTask = task as FloatTask;
                floatTask.RemoveMaster(this);
            }
            // 把目标对象移出自己承载的表中
            unitList.Remove(unit);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Item") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            OnUnitExit(unit);
        }
    }

    public override void AfterDeath()
    {
        // 移除水波纹效果
        EffectManager.RemoveWaterWaveEffectFromUnit(this);

        // 移除表中还存在的单位身上的由该盘子产生的免疫水蚀标签
        for (int i = 0; i < unitList.Count; i++)
        {
            OnUnitExit(unitList[0]);
        }
        base.AfterDeath();
    }

    public void OnEnterWater()
    {
        
    }

    public void OnStayWater()
    {
        
    }

    public void OnExitWater()
    {
        
    }




    ////////////////////////////////////////////////////////////以下是内部类的定义///////////////////////

    /// <summary>
    /// 悬浮任务
    /// </summary>
    public class FloatTask : ITask
    {
        private List<BaseUnit> masterList = new List<BaseUnit>(); // 提供悬浮的对象 
        private BaseUnit unit;
        private FloatModifier EnterWaterSpriteOffsetY = new FloatModifier(0); // 下水时的贴图Y总偏移量
        private float offsetY;
        private int lastTime;
        private int currentTime = 0;
        private int totalTime = 30;
        private bool isHasVehicle;

        public FloatTask(BaseUnit unit, float offsetY)
        {
            this.unit = unit;
            this.offsetY = offsetY;
        }

        private FloatTask()
        {

        }

        private void Initial()
        {
            lastTime = 0;
            currentTime = 0;
            totalTime = 30;
            isHasVehicle = true;
        }

        /// <summary>
        /// 添加一个提供悬浮的对象
        /// </summary>
        public void AddMaster(BaseUnit master)
        {
            masterList.Add(master);
        }

        /// <summary>
        /// 移除一个提供悬浮的对象
        /// </summary>
        /// <param name="master"></param>
        public void RemoveMaster(BaseUnit master)
        {
            masterList.Remove(master);
        }

        public void OnEnter()
        {
            Initial();
        }

        public void OnExit()
        {
            // 如果还存在水状态，使目标再次入水
            StatusAbility s = unit.GetUniqueStatus(StringManager.WaterGridState);
            if (s != null && s is WaterStatusAbility)
            {
                WaterStatusAbility s2 = s as WaterStatusAbility;
                s2.LetMasterEnterWater();
            }
        }

        public void OnUpdate()
        {
            // 检测悬浮对象的有效性，并剔除无效的对象
            List<BaseUnit> delList = new List<BaseUnit>();
            foreach (var item in masterList)
            {
                if(!item.IsValid())
                    delList.Add(item);
            }
            foreach (var item in delList)
            {
                masterList.Remove(item);
            }
            // 再检测是否还有提供悬浮对象（载具）
            if (masterList.Count > 0)
                isHasVehicle = true;
            else
                isHasVehicle = false;

            // 根据是否还存在载具决定上浮还是下沉
            if (isHasVehicle)
            {
                if (currentTime < totalTime)
                    currentTime++;
            }
            else
            {
                if (currentTime > 0)
                    currentTime--;
            }
            if (lastTime != currentTime)
            {
                float r = ((float)currentTime) / totalTime;
                unit.RemoveSpriteOffsetY(EnterWaterSpriteOffsetY);
                EnterWaterSpriteOffsetY.Value = offsetY * r;
                unit.AddSpriteOffsetY(EnterWaterSpriteOffsetY);
            }
            lastTime = currentTime;
        }

        /// <summary>
        /// 当载具消失且沉到最深时
        /// </summary>
        /// <returns></returns>
        public bool IsMeetingExitCondition()
        {
            return !isHasVehicle && currentTime==0;
        }
    }

}
