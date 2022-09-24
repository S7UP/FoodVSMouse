using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ľ����
/// </summary>
public class WoodenDisk : FoodUnit, IInWater
{
    private const string TaskKey = "ľ�����ṩ������Ч��";
    private BoolModifier IgnoreWaterModifier = new BoolModifier(true); // ����ˮʴ������
    private List<BaseUnit> unitList = new List<BaseUnit>(); // ��Ӱ��ĵ�λ�� 
    // �����±��е���������ľ�������ò���Ч
    private List<MouseNameTypeMap> noEffectMouseList = new List<MouseNameTypeMap>() 
    { 
        MouseNameTypeMap.SubmarineMouse,
        MouseNameTypeMap.RowboatMouse,
    };


    public override void MInit()
    {
        unitList.Clear();
        base.MInit();
        // ����ˮʴЧ��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, IgnoreWaterModifier);
        typeAndShapeToLayer = -1;
        // ���ˮ����Ч��
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
        // �޳��Ѿ���Ч�ĵ�λ
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
            // ���Ŀ���Ƿ�����Ч
            if (unit is MouseUnit && noEffectMouseList.Contains((MouseNameTypeMap)unit.mType) && unit.GetHeight()!=0)
                return;

            if (!unitList.Contains(unit))
            {
                unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, IgnoreWaterModifier);
                // ʹĿ��ǿ�Ƴ�ˮ��ǿ�Ƴ�ˮ�ķ����������ʹˮʴʧЧ��
                StatusAbility s = unit.GetUniqueStatus(StringManager.WaterGridState);
                if (s != null && s is WaterStatusAbility)
                {
                    WaterStatusAbility s2 = s as WaterStatusAbility;
                    s2.LetMasterExitWater();
                }
                // �ṩ �����ṩ�� Ч��
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
                // ��Ŀ���������Լ����صı���
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
            // ������������ˮʴ��ǩʱ��ʹ��ˮʴ��Ч
            //StatusAbility s = unit.GetUniqueStatus(StringManager.WaterGridState);
            //if (s != null && !unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
            //    s.SetEffectEnable(true);
            // �Ƴ���ǰ �����ṩ�� Ч��
            ITask task = unit.GetTask(TaskKey);
            if (task != null && task is FloatTask)
            {
                FloatTask floatTask = task as FloatTask;
                floatTask.RemoveMaster(this);
            }
            // ��Ŀ������Ƴ��Լ����صı���
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
        // �Ƴ�ˮ����Ч��
        EffectManager.RemoveWaterWaveEffectFromUnit(this);

        // �Ƴ����л����ڵĵ�λ���ϵ��ɸ����Ӳ���������ˮʴ��ǩ
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




    ////////////////////////////////////////////////////////////�������ڲ���Ķ���///////////////////////

    /// <summary>
    /// ��������
    /// </summary>
    public class FloatTask : ITask
    {
        private List<BaseUnit> masterList = new List<BaseUnit>(); // �ṩ�����Ķ��� 
        private BaseUnit unit;
        private FloatModifier EnterWaterSpriteOffsetY = new FloatModifier(0); // ��ˮʱ����ͼY��ƫ����
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
        /// ���һ���ṩ�����Ķ���
        /// </summary>
        public void AddMaster(BaseUnit master)
        {
            masterList.Add(master);
        }

        /// <summary>
        /// �Ƴ�һ���ṩ�����Ķ���
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
            // ���������ˮ״̬��ʹĿ���ٴ���ˮ
            StatusAbility s = unit.GetUniqueStatus(StringManager.WaterGridState);
            if (s != null && s is WaterStatusAbility)
            {
                WaterStatusAbility s2 = s as WaterStatusAbility;
                s2.LetMasterEnterWater();
            }
        }

        public void OnUpdate()
        {
            // ��������������Ч�ԣ����޳���Ч�Ķ���
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
            // �ټ���Ƿ����ṩ���������ؾߣ�
            if (masterList.Count > 0)
                isHasVehicle = true;
            else
                isHasVehicle = false;

            // �����Ƿ񻹴����ؾ߾����ϸ������³�
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
        /// ���ؾ���ʧ�ҳ�������ʱ
        /// </summary>
        /// <returns></returns>
        public bool IsMeetingExitCondition()
        {
            return !isHasVehicle && currentTime==0;
        }
    }

}
