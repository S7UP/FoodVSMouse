using System;

using UnityEngine;

/// <summary>
/// ��ˮ״̬debuff
/// </summary>
public class WaterStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // ��ǰ�ṩ����Ч����������
    private BoolModifier waterStatusBoolModifier; // ��ˮ״̬��־
    private ITask mInWaterTask;
    private const string TaskKey = StringManager.IgnoreWaterGridState;
    private float descendGridCount; // �½�����

    public WaterStatusAbility(BaseUnit pmaster, float descendGridCount) : base(pmaster)
    {
        this.descendGridCount = descendGridCount;
    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ��ȡĿ�����Ϲ��ص���ˮ�������û�����һ����ȥ
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

        // ���Ŀ���Ƿ�����ˮ�ӿڣ�������������ö�Ӧ����
        if (typeof(IInWater).IsAssignableFrom(master.GetType()))
        {
            IInWater InWater = (IInWater)master;
            InWater.OnEnterWater();
        }

        // ����Ƿ�������ˮ
        if (master.NumericBox.GetBoolNumericValue(StringManager.IgnoreWaterGridState))
        {
            // ���ߵĻ�ֱ�ӽ���Ч������
            SetEffectEnable(false);
        }
        else
        {
            OnEnableEffect();
        }
    }


    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
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
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnEnableEffect()
    {
        if (slowDownFloatModifier == null)
        {
            // ���ˮ���μ���Ч�����������Ч��ֵͨ����ȡ��ǰ�ؿ�Ԥ��ֵ
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
    /// ������Ч���ڼ�
    /// </summary>
    public override void OnEffecting()
    {
        // ���Ŀ���Ƿ�����ˮ�ӿڣ�������������ö�Ӧ����
        if (typeof(IInWater).IsAssignableFrom(master.GetType()))
        {
            IInWater InWater = (IInWater)master;
            InWater.OnStayWater();
        }
        // ����Դ�ĳ����˺�
        // float percentDamgePerSeconds = GameController.Instance.GetNumberManager().GetValue(StringManager.WaterPerCentDamge);
        new DamageAction(CombatAction.ActionType.CauseDamage, null, master, master.NumericBox.Hp.Value*0.05f/ConfigManager.fps).ApplyAction();
    }

    /// <summary>
    /// �ڷ�����Ч���ڼ�
    /// </summary>
    public override void OnNotEffecting()
    {

    }

    /// <summary>
    /// �����������������ʱ���ǻ��ϵ��
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEndCondition()
    {
        return master.isDeathState;
        // return false;
    }

    /// <summary>
    /// BUFF����ʱҪ������
    /// </summary>
    public override void AfterEffect()
    {
        // ���Ŀ���Ƿ�����ˮ�ӿڣ�������������ö�Ӧ����
        if (typeof(IInWater).IsAssignableFrom(master.GetType()))
        {
            IInWater InWater = (IInWater)master;
            InWater.OnExitWater();
        }

        if (master.IsAlive())
        {
            // �����ϰ���������λ
            mInWaterTask.OnExit();
        }
        else
        {
            // ����ִ����ˮ
            InWaterTask t = (InWaterTask)mInWaterTask;
            t.DieInWater();
        }
        OnDisableEffect();
    }


    /////////////////////////////////////////////////����Ϊ�ڲ���Ķ���/////////////////////////////////////////////////

    /// <summary>
    /// ��ˮ����
    /// </summary>
    public class InWaterTask: ITask
    {
        private BaseUnit unit;
        private FloatModifier EnterWaterSpriteOffsetY = new FloatModifier(0); // ��ˮʱ����ͼY��ƫ����
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
            EffectManager.AddWaterWaveEffectToUnit(unit); // �����ˮ��Ч
            Sprite sprite = unit.GetSpirte();
            isInWater = true;
            offsetY = descendGridCount * MapManager.gridHeight;  
            cutRate = (sprite.pivot.y + TransManager.WorldToTex(offsetY)) / sprite.rect.height; // �ü��߶�
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
            offsetYEnd = 1 * MapManager.gridHeight;  // �½�1��
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
        /// ������Ȼ��ʧ
        /// </summary>
        /// <returns></returns>
        public bool IsMeetingExitCondition()
        {
            return false;
        }
    }
}
