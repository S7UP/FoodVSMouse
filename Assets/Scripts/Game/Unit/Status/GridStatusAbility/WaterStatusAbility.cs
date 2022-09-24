using System;

using UnityEngine;

/// <summary>
/// 溺水状态debuff
/// </summary>
public class WaterStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // 当前提供减速效果的修饰器
    private FloatModifier decAttackSpeedModifier = new FloatModifier(-20); // 减攻速效果修饰器
    private BoolModifier waterStatusBoolModifier; // 溺水状态标志
    private ITask mInWaterTask;
    private const string TaskKey = StringManager.IgnoreWaterGridState;
    private float descendGridCount; // 下降格数
    private const int TotalTime = 60;
    private int triggerDamgeTimeLeft; // 触发伤害剩余时间 若为-1则永远不触发伤害

    public WaterStatusAbility(BaseUnit pmaster, float descendGridCount) : base(pmaster)
    {
        this.descendGridCount = descendGridCount;
        triggerDamgeTimeLeft = -1; // 默认为-1
    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 产生一个下水任务
        // 获取目标身上挂载的下水任务，如果没有则挂一个上去
        mInWaterTask = master.GetTask(TaskKey);
        if (mInWaterTask == null)
        {
            mInWaterTask = new InWaterTask(master, descendGridCount, (f) => {
                if (master.GetSpriteRendererList() == null)
                {
                    master.GetSpriteRenderer().material.SetFloat("_CutRateY", f);
                }
                else
                {
                    foreach (var item in master.GetSpriteRendererList())
                    {
                        item.material.SetFloat("_CutRateY", f);
                    }
                }

            });
            master.AddUniqueTask(TaskKey, mInWaterTask);
        }
        else
        {
            mInWaterTask.OnEnter();
        }

        // 使目标强制入水
        LetMasterEnterWater();
    }


    /// <summary>
    /// 在触发禁用效果时的事件
    /// </summary>
    public override void OnDisableEffect()
    {
        triggerDamgeTimeLeft = -1;
        if (slowDownFloatModifier != null)
        {
            master.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
            slowDownFloatModifier = null;
        }
        // 移除减攻速
        master.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedModifier);
        if (waterStatusBoolModifier != null)
        {
            master.NumericBox.RemoveDecideModifierToBoolDict(StringManager.WaterGridState, waterStatusBoolModifier);
            waterStatusBoolModifier = null;
        }
    }

    /// <summary>
    /// 在触发启用效果时的事件
    /// </summary>
    public override void OnEnableEffect()
    {
        triggerDamgeTimeLeft = TotalTime;
        if (slowDownFloatModifier == null)
        {
            // 添加水地形减速效果，具体减速效果值通过读取当前关卡预设值
            // slowDownFloatModifier = new FloatModifier(GameController.Instance.GetNumberManager().GetValue(StringManager.WaterSlowDown));
            slowDownFloatModifier = new FloatModifier(-20);
            master.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
        }
        // 减攻速
        master.NumericBox.AttackSpeed.AddFinalPctAddModifier(decAttackSpeedModifier);
        if(waterStatusBoolModifier == null)
        {
            waterStatusBoolModifier = new BoolModifier(true);
            master.NumericBox.AddDecideModifierToBoolDict(StringManager.WaterGridState, waterStatusBoolModifier);
        }
    }

    /// <summary>
    /// 在启用效果期间
    /// </summary>
    public override void OnEffecting()
    {
        // 检测目标是否有下水接口，如果有则额外调用对应方法
        if (typeof(IInWater).IsAssignableFrom(master.GetType()))
        {
            IInWater InWater = (IInWater)master;
            InWater.OnStayWater();
        }

        // 无来源的持续伤害 （每秒造成 1%已损失生命值伤害（最小值为2））
        if (triggerDamgeTimeLeft == 0)
        {
            // float percentDamgePerSeconds = GameController.Instance.GetNumberManager().GetValue(StringManager.WaterPerCentDamge);
            new DamageAction(CombatAction.ActionType.CauseDamage, null, master, Mathf.Max(2, master.GetLostHp() * 0.01f)).ApplyAction();
            triggerDamgeTimeLeft = TotalTime;
        }else if(triggerDamgeTimeLeft > 0)
            triggerDamgeTimeLeft--;
    }

    /// <summary>
    /// 在非启用效果期间
    /// </summary>
    public override void OnNotEffecting()
    {

    }

    /// <summary>
    /// 结束的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEndCondition()
    {
        return master.isDeathState;
    }

    /// <summary>
    /// BUFF结束时要做的事
    /// </summary>
    public override void AfterEffect()
    {
        // OnDisableEffect();
        LetMasterExitWater();
        //SetEffectEnable(false);
    }

    /// <summary>
    /// 使BUFF持有者强制入水
    /// </summary>
    public void LetMasterEnterWater()
    {
        mInWaterTask.OnEnter();
        // 检测是否免疫溺水
        if (master.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
        {
            SetEffectEnable(false);
        }
        else
        {
            SetEffectEnable(true);
        }
    }

    /// <summary>
    /// 使BUFF持有者强制出水（在BUFF结束时或者其他遇到等效陆地要暂时屏蔽水效果时调用）
    /// </summary>
    public void LetMasterExitWater()
    {
        InWaterTask t = (InWaterTask)mInWaterTask;
        t.OnExitWater();
        SetEffectEnable(false);
    }


    /////////////////////////////////////////////////以下为内部类的定义/////////////////////////////////////////////////

    /// <summary>
    /// 溺水任务
    /// </summary>
    public class InWaterTask: ITask
    {
        private BaseUnit unit;
        private FloatModifier EnterWaterSpriteOffsetY = new FloatModifier(0); // 下水时的贴图Y总偏移量
        private float offsetY;
        private float offsetYEnd;
        private float cutRate;
        private float cutRateEnd;
        private int lastTime;
        private int currentTime = 0;
        private int totalTime = 30;
        private bool isInWater;
        private bool isDie;
        private Action<float> SetCutRateFunc;
        private float descendGridCount;

        public InWaterTask(BaseUnit unit, float descendGridCount, Action<float> SetCutRateFunc)
        {
            this.unit = unit;
            this.descendGridCount = descendGridCount;
            this.SetCutRateFunc = SetCutRateFunc;
        }

        private InWaterTask()
        {

        }

        private void Initial()
        {
            offsetY = 0;
            offsetYEnd = 0;
            cutRate = 0;
            cutRateEnd = 0;
            lastTime = 0;
            currentTime = 0;
            totalTime = 30;
            isInWater = false;
            isDie = false;
        }

        public void OnEnter()
        {
            // 检测目标是否有下水接口，如果有则额外调用对应方法
            if (typeof(IInWater).IsAssignableFrom(unit.GetType()))
            {
                IInWater InWater = (IInWater)unit;
                InWater.OnEnterWater();
            }
            else
            {
                Initial();
                EffectManager.AddWaterWaveEffectToUnit(unit); // 添加下水特效
                Sprite sprite = null;
                if (unit.GetSpriteList() == null)
                    sprite = unit.GetSpirte();
                else
                    sprite = unit.GetSpriteList()[0];
                isInWater = true;
                offsetY = descendGridCount * MapManager.gridHeight;
                cutRate = (sprite.pivot.y + TransManager.WorldToTex(offsetY)) / sprite.rect.height; // 裁剪高度
            }
        }

        /// <summary>
        /// 离开水，但是任务还在
        /// </summary>
        public void OnExitWater()
        {
            // 检测目标是否有下水接口，如果有则额外调用对应方法
            if (typeof(IInWater).IsAssignableFrom(unit.GetType()))
            {
                IInWater InWater = (IInWater)unit;
                InWater.OnExitWater();
            }
            else
            {
                isInWater = false;
                // 如果这家伙死了则执行溺水动作
                if (!unit.IsAlive())
                {
                    DieInWater();
                }
                EffectManager.RemoveWaterWaveEffectFromUnit(unit);
            }
        }

        /// <summary>
        /// 当该任务被移除时（触发时机基本只有该对象被回收了）
        /// </summary>
        public void OnExit()
        {
            // 恢复剪切掉的部分
            SetCutRateFunc(0);
        }

        public void DieInWater()
        {
            Sprite sprite = null;
            if (unit.GetSpriteList() == null)
                sprite = unit.GetSpirte();
            else
                sprite = unit.GetSpriteList()[0];
            isDie = true;
            currentTime = 0;
            totalTime = 120;
            offsetYEnd = 1 * MapManager.gridHeight;  // 下降1格
            cutRateEnd = (sprite.pivot.y + TransManager.WorldToTex(offsetYEnd)) / sprite.rect.height;
            unit.RemoveSpriteOffsetY(EnterWaterSpriteOffsetY);
            EnterWaterSpriteOffsetY.Value = -offsetY;
            unit.AddSpriteOffsetY(EnterWaterSpriteOffsetY);
            SetCutRateFunc(cutRate);
        }

        public void OnUpdate()
        {
            if (isDie)
            {
                currentTime++;
                float r = ((float)currentTime) / totalTime;
                if (r > 1)
                    r = 1;
                unit.RemoveSpriteOffsetY(EnterWaterSpriteOffsetY);
                EnterWaterSpriteOffsetY.Value = -(offsetY + (offsetYEnd - offsetY) * r);
                unit.AddSpriteOffsetY(EnterWaterSpriteOffsetY);
                SetCutRateFunc(cutRate + (cutRateEnd - cutRate) * r);
            }
            else
            {
                if (isInWater)
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
                    EnterWaterSpriteOffsetY.Value = -offsetY * r;
                    unit.AddSpriteOffsetY(EnterWaterSpriteOffsetY);
                    SetCutRateFunc(cutRate * r);
                }
                lastTime = currentTime;
            }
        }

        /// <summary>
        /// 不会自然消失
        /// </summary>
        /// <returns></returns>
        public bool IsMeetingExitCondition()
        {
            return false;
        }

        /// <summary>
        /// 是否在水里
        /// </summary>
        /// <returns></returns>
        public bool IsInWater()
        {
            return isInWater;
        }
    }
}
