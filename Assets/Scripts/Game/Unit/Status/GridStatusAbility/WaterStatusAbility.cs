using System;

using UnityEngine;

/// <summary>
/// 溺水状态debuff
/// </summary>
public class WaterStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // 当前提供减速效果的修饰器
    private BoolModifier waterStatusBoolModifier; // 溺水状态标志
    private ITask mInWaterTask;
    private const string TaskKey = StringManager.IgnoreWaterGridState;
    private float descendGridCount; // 下降格数

    public WaterStatusAbility(BaseUnit pmaster, float descendGridCount) : base(pmaster)
    {
        this.descendGridCount = descendGridCount;
    }

    /// <summary>
    /// 在效果生效前（BUFF挂上一瞬间最早做的事）
    /// </summary>
    public override void BeforeEffect()
    {
        // 获取目标身上挂载的下水任务，如果没有则挂一个上去
        mInWaterTask = master.GetTask(TaskKey);
        if (mInWaterTask == null)
        {
            mInWaterTask = new InWaterTask(master, descendGridCount, (f) => { master.GetSpriteRenderer().material.SetFloat("_CutRateY", f); });
            master.AddUniqueTask(TaskKey, mInWaterTask);
        }
        else
        {
            mInWaterTask.OnEnter();
        }

        // 检测目标是否有下水接口，如果有则额外调用对应方法
        if (typeof(IInWater).IsAssignableFrom(master.GetType()))
        {
            IInWater InWater = (IInWater)master;
            InWater.OnEnterWater();
        }

        // 检测是否免疫溺水
        if (master.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
        {
            // 免疫的话直接禁用效果即可
            SetEffectEnable(false);
        }
        else
        {
            OnEnableEffect();
        }
    }


    /// <summary>
    /// 在触发禁用效果时的事件
    /// </summary>
    public override void OnDisableEffect()
    {
        if (slowDownFloatModifier != null)
        {
            master.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
            slowDownFloatModifier = null;
        }
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
        if (slowDownFloatModifier == null)
        {
            // 添加水地形减速效果，具体减速效果值通过读取当前关卡预设值
            // slowDownFloatModifier = new FloatModifier(GameController.Instance.GetNumberManager().GetValue(StringManager.WaterSlowDown));
            slowDownFloatModifier = new FloatModifier(-50);
            master.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
        }
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
        // 无来源的持续伤害
        // float percentDamgePerSeconds = GameController.Instance.GetNumberManager().GetValue(StringManager.WaterPerCentDamge);
        new DamageAction(CombatAction.ActionType.CauseDamage, null, master, master.NumericBox.Hp.Value*0.05f/ConfigManager.fps).ApplyAction();
    }

    /// <summary>
    /// 在非启用效果期间
    /// </summary>
    public override void OnNotEffecting()
    {

    }

    /// <summary>
    /// 结束的条件，与持续时间是或关系！
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEndCondition()
    {
        return master.isDeathState;
        // return false;
    }

    /// <summary>
    /// BUFF结束时要做的事
    /// </summary>
    public override void AfterEffect()
    {
        // 检测目标是否有下水接口，如果有则额外调用对应方法
        if (typeof(IInWater).IsAssignableFrom(master.GetType()))
        {
            IInWater InWater = (IInWater)master;
            InWater.OnExitWater();
        }

        if (master.IsAlive())
        {
            // 活着上岸则上升身位
            mInWaterTask.OnExit();
        }
        else
        {
            // 否则执行溺水
            InWaterTask t = (InWaterTask)mInWaterTask;
            t.DieInWater();
        }
        OnDisableEffect();
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
            Initial();
            EffectManager.AddWaterWaveEffectToUnit(unit); // 添加下水特效
            Sprite sprite = unit.GetSpirte();
            isInWater = true;
            offsetY = descendGridCount * MapManager.gridHeight;  
            cutRate = (sprite.pivot.y + TransManager.WorldToTex(offsetY)) / sprite.rect.height; // 裁剪高度
        }

        public void OnExit()
        {
            isInWater = false;
            EffectManager.RemoveWaterWaveEffectFromUnit(unit);
        }

        public void DieInWater()
        {
            Sprite sprite = unit.GetSpirte();
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
    }
}
