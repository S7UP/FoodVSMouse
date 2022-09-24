using System;

using UnityEngine;

/// <summary>
/// ��ˮ״̬debuff
/// </summary>
public class WaterStatusAbility : StatusAbility
{
    private FloatModifier slowDownFloatModifier; // ��ǰ�ṩ����Ч����������
    private FloatModifier decAttackSpeedModifier = new FloatModifier(-20); // ������Ч��������
    private BoolModifier waterStatusBoolModifier; // ��ˮ״̬��־
    private ITask mInWaterTask;
    private const string TaskKey = StringManager.IgnoreWaterGridState;
    private float descendGridCount; // �½�����
    private const int TotalTime = 60;
    private int triggerDamgeTimeLeft; // �����˺�ʣ��ʱ�� ��Ϊ-1����Զ�������˺�

    public WaterStatusAbility(BaseUnit pmaster, float descendGridCount) : base(pmaster)
    {
        this.descendGridCount = descendGridCount;
        triggerDamgeTimeLeft = -1; // Ĭ��Ϊ-1
    }

    /// <summary>
    /// ��Ч����Чǰ��BUFF����һ˲�����������£�
    /// </summary>
    public override void BeforeEffect()
    {
        // ����һ����ˮ����
        // ��ȡĿ�����Ϲ��ص���ˮ�������û�����һ����ȥ
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

        // ʹĿ��ǿ����ˮ
        LetMasterEnterWater();
    }


    /// <summary>
    /// �ڴ�������Ч��ʱ���¼�
    /// </summary>
    public override void OnDisableEffect()
    {
        triggerDamgeTimeLeft = -1;
        if (slowDownFloatModifier != null)
        {
            master.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(slowDownFloatModifier);
            slowDownFloatModifier = null;
        }
        // �Ƴ�������
        master.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedModifier);
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
        triggerDamgeTimeLeft = TotalTime;
        if (slowDownFloatModifier == null)
        {
            // ���ˮ���μ���Ч�����������Ч��ֵͨ����ȡ��ǰ�ؿ�Ԥ��ֵ
            // slowDownFloatModifier = new FloatModifier(GameController.Instance.GetNumberManager().GetValue(StringManager.WaterSlowDown));
            slowDownFloatModifier = new FloatModifier(-20);
            master.NumericBox.MoveSpeed.AddFinalPctAddModifier(slowDownFloatModifier);
        }
        // ������
        master.NumericBox.AttackSpeed.AddFinalPctAddModifier(decAttackSpeedModifier);
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

        // ����Դ�ĳ����˺� ��ÿ����� 1%����ʧ����ֵ�˺�����СֵΪ2����
        if (triggerDamgeTimeLeft == 0)
        {
            // float percentDamgePerSeconds = GameController.Instance.GetNumberManager().GetValue(StringManager.WaterPerCentDamge);
            new DamageAction(CombatAction.ActionType.CauseDamage, null, master, Mathf.Max(2, master.GetLostHp() * 0.01f)).ApplyAction();
            triggerDamgeTimeLeft = TotalTime;
        }else if(triggerDamgeTimeLeft > 0)
            triggerDamgeTimeLeft--;
    }

    /// <summary>
    /// �ڷ�����Ч���ڼ�
    /// </summary>
    public override void OnNotEffecting()
    {

    }

    /// <summary>
    /// ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEndCondition()
    {
        return master.isDeathState;
    }

    /// <summary>
    /// BUFF����ʱҪ������
    /// </summary>
    public override void AfterEffect()
    {
        // OnDisableEffect();
        LetMasterExitWater();
        //SetEffectEnable(false);
    }

    /// <summary>
    /// ʹBUFF������ǿ����ˮ
    /// </summary>
    public void LetMasterEnterWater()
    {
        mInWaterTask.OnEnter();
        // ����Ƿ�������ˮ
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
    /// ʹBUFF������ǿ�Ƴ�ˮ����BUFF����ʱ��������������Ч½��Ҫ��ʱ����ˮЧ��ʱ���ã�
    /// </summary>
    public void LetMasterExitWater()
    {
        InWaterTask t = (InWaterTask)mInWaterTask;
        t.OnExitWater();
        SetEffectEnable(false);
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
            // ���Ŀ���Ƿ�����ˮ�ӿڣ�������������ö�Ӧ����
            if (typeof(IInWater).IsAssignableFrom(unit.GetType()))
            {
                IInWater InWater = (IInWater)unit;
                InWater.OnEnterWater();
            }
            else
            {
                Initial();
                EffectManager.AddWaterWaveEffectToUnit(unit); // �����ˮ��Ч
                Sprite sprite = null;
                if (unit.GetSpriteList() == null)
                    sprite = unit.GetSpirte();
                else
                    sprite = unit.GetSpriteList()[0];
                isInWater = true;
                offsetY = descendGridCount * MapManager.gridHeight;
                cutRate = (sprite.pivot.y + TransManager.WorldToTex(offsetY)) / sprite.rect.height; // �ü��߶�
            }
        }

        /// <summary>
        /// �뿪ˮ������������
        /// </summary>
        public void OnExitWater()
        {
            // ���Ŀ���Ƿ�����ˮ�ӿڣ�������������ö�Ӧ����
            if (typeof(IInWater).IsAssignableFrom(unit.GetType()))
            {
                IInWater InWater = (IInWater)unit;
                InWater.OnExitWater();
            }
            else
            {
                isInWater = false;
                // �����һ�������ִ����ˮ����
                if (!unit.IsAlive())
                {
                    DieInWater();
                }
                EffectManager.RemoveWaterWaveEffectFromUnit(unit);
            }
        }

        /// <summary>
        /// ���������Ƴ�ʱ������ʱ������ֻ�иö��󱻻����ˣ�
        /// </summary>
        public void OnExit()
        {
            // �ָ����е��Ĳ���
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

        /// <summary>
        /// �Ƿ���ˮ��
        /// </summary>
        /// <returns></returns>
        public bool IsInWater()
        {
            return isInWater;
        }
    }
}
