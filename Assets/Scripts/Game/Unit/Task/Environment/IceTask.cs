using S7P.Numeric;

using System;

using UnityEngine;

namespace Environment
{
    public class IceTask : BaseTask
    {

        private const float decValue = -25f / 60; // ���������ÿ֡�½��ٶ�
        private const string TaskKey = "IceTask";

        private BaseUnit master;
        private MultiplyFloatModifierCollector DecRate = new MultiplyFloatModifierCollector();
        private float value; // ��ǰ����ֵ
        private bool isForzen; // �Ƿ񴥷�����
        private FloatModifier decAttackSpeedMod = new FloatModifier(-34f); // ������

        private Action<BaseUnit> OnEnterWaterAction;
        private Action<BaseUnit> OnExitWaterAction;

        private Action<BaseUnit> OnEnterLavaAction;
        private Action<BaseUnit> OnStayLavaAction;
        private Action<BaseUnit> OnExitLavaAction;

        private BaseEffect eff; // ������Ч
        private float gobal_alpha; // ȫ��͸����

        public IceTask(BaseUnit master, float value)
        {
            gobal_alpha = 0;
            this.value = value;
            this.master = master;
            isForzen = false;

            // ��ˮ�Ľ���
            {
                FloatModifier IceDecRateMod = new FloatModifier(0.8f);
                OnEnterWaterAction = (u) =>
                {
                    DecRate.AddModifier(IceDecRateMod);
                };
                OnExitWaterAction = (u) =>
                {
                    DecRate.RemoveModifier(IceDecRateMod);
                };
            }

            // ���ҽ��Ľ���
            {
                FloatModifier IceDecRateMod = new FloatModifier(5);
                FloatModifier LavaRateMod = new FloatModifier(2);
                OnEnterLavaAction = (u) =>
                {
                    // �������б�������ʹĿ�������ܵ��ҽ��˺�
                    new DamageAction(CombatAction.ActionType.BurnDamage, null, master, this.value * 0.004f * master.mMaxHp).ApplyAction();
                    this.value = 0;
                };
                OnStayLavaAction = (u) =>
                {
                    // �������б�������ʹĿ�������ܵ��ҽ��˺�
                    new DamageAction(CombatAction.ActionType.BurnDamage, null, master, this.value * 0.004f * master.mMaxHp).ApplyAction();
                    this.value = 0;
                };
                OnExitLavaAction = (u) =>
                {

                };
            }

            // ������Ч����
            {
                eff = BaseEffect.CreateInstance(master.GetSpriteRenderer().sprite);
                eff.spriteRenderer.sortingLayerName = master.GetSpriteRenderer().sortingLayerName;
                eff.spriteRenderer.sortingOrder = master.GetSpriteRenderer().sortingOrder;
                eff.spriteRenderer.material = GameManager.Instance.GetMaterial("Ice");
                eff.spriteRenderer.material.SetFloat("_Alpha", 0);
                eff.spriteRenderer.material.SetFloat("_lineWidth", 0);
                eff.AddSetAlphaAction((alpha) => {
                    gobal_alpha = 0.8f + 0.2f*alpha;
                });
                GameController.Instance.AddEffect(eff);
                master.mEffectController.AddEffectToGroup("Skin", 0, eff);
                eff.transform.localPosition = Vector2.zero;
                eff.transform.localScale = Vector2.one;
                // ��¼һ��������һ�ܵ�BUG��2023.6.10
                // �����Ч��Ŀ�걻���׻���֮ǰ�ͱ������ˣ���ôҪȡ��Ŀ�����ʱ�ٻ�����Ч�ļ���
                // ��Ϊ�����ȡ�����ᷢ����Ч�������õ�BUG������С����ͼ�ɵ��й����ϣ������ص������һϵ���߼�����
                // ��ȷ�߼��������ģ���Ч������->Ŀ�걻���գ�������Ŀ�����ʱ������Ч�ķ���������Ϊ��Ч�����ͱ��������������·�����->��Ч���������á�
                // ���쳣���߼��ǣ���Ч������->��Ч����������->Ŀ�걻���գ�������Ŀ�����ʱ������Ч�ķ���������Ч�����õ������ط�ȥ��Ȼ���ֱ�����û����ˣ�->��Ч������->��Ч�ٱ����ã�����ʱ��Чͬʱ������Task���ã�����Ȼ�ǲ��Եģ�
                // ���ǣ�ֻҪ�����������У�����Ч������ʱ���Ƴ�Ŀ�걻����ʱ������Ч�ļ����������ƾ֣�
            }

        }

        #region ������̳�
        protected override void O_OnEnter()
        {
            // ��ˮ�Ľ���
            {
                master.actionPointController.AddAction("OnEnterWater", OnEnterWaterAction);
                master.actionPointController.AddAction("OnExitWater", OnExitWaterAction);

                // ��ʩ��ʱ���һ�������ڲ���ˮ��
                if (master.taskController.GetTask("WaterTask") != null)
                {
                    master.actionPointController.TriggerAction("OnEnterWater");
                }
            }

            // ���ҽ��Ľ���
            {
                master.actionPointController.AddAction("OnEnterLava", OnEnterLavaAction);
                master.actionPointController.AddAction("OnStayLava", OnStayLavaAction);
                master.actionPointController.AddAction("OnExitLava", OnExitLavaAction);

                // ��ʩ��ʱ���һ�������ڲ����ҽ���
                if (master.taskController.GetTask("LavaTask") != null)
                {
                    master.actionPointController.TriggerAction("OnEnterLava");
                }
            }
        }

        protected override void O_OnUpdate()
        {
            if (value >= 100 && !isForzen)
            {
                TriggerIce(); // ���Ҵ�������
            }

            if (isForzen)
            {
                master.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(master, 2, false));
                value += decValue * DecRate.TotalValue;
            }

            // ���±�����ʾ
            {
                eff.spriteRenderer.sprite = master.GetSpirte();
                eff.spriteRenderer.sortingLayerName = master.GetSpriteRenderer().sortingLayerName;
                eff.spriteRenderer.sortingOrder = master.GetSpriteRenderer().sortingOrder;

                if (isForzen && !master.NumericBox.GetBoolNumericValue(StringManager.IgnoreStun))
                {
                    eff.spriteRenderer.material.SetFloat("_lineWidth", 3);
                    eff.spriteRenderer.material.SetFloat("_Alpha", gobal_alpha * Mathf.Lerp(0, 1, Mathf.Min(1, value / 100)));
                }
                else
                {
                    eff.spriteRenderer.material.SetFloat("_Alpha", gobal_alpha * Mathf.Lerp(0, 0.5f, Mathf.Min(1, value / 100)));
                    eff.spriteRenderer.material.SetFloat("_lineWidth", 0);
                } 
            }
        }

        protected override bool O_IsMeetingCondition()
        {
            return value <= 0 || !master.IsAlive();
        }

        protected override void O_OnExit()
        {
            // ��ˮ�Ľ���
            {
                master.actionPointController.RemoveAction("OnEnterWater", OnEnterWaterAction);
                master.actionPointController.RemoveAction("OnExitWater", OnExitWaterAction);
            }

            // ���ҽ��Ľ���
            {
                master.actionPointController.RemoveAction("OnEnterLava", OnEnterLavaAction);
                master.actionPointController.RemoveAction("OnExitLava", OnExitLavaAction);
            }

            // ������Ч�Ƴ�
            {
                eff.ExecuteDeath();
            }

            // ��������Ч�Ƴ�
            {
                master.NumericBox.AttackSpeed.RemoveFinalPctAddModifier(decAttackSpeedMod);
            }
        }

        protected override void O_ShutDown()
        {
            // O_OnExit();
        }
        #endregion

        /// <summary>
        /// ����һ������ֵ
        /// </summary>
        /// <param name="value"></param>
        public void AddValue(float value)
        {
            this.value += value;
        }

        public float GetValue()
        {
            return value;
        }

        /// <summary>
        /// �Ƿ񴥷�����
        /// </summary>
        /// <returns></returns>
        public bool IsForzen()
        {
            return isForzen;
        }

        /// <summary>
        /// ��������
        /// </summary>
        public void TriggerIce()
        {
            isForzen = true;
            master.NumericBox.AttackSpeed.AddFinalPctAddModifier(decAttackSpeedMod);
        }

        #region ���⿪�ŵľ�̬����
        /// <summary>
        /// ����Ч��
        /// </summary>
        public static void TriggerIce(BaseUnit u)
        {
            if (u.taskController.GetTask(TaskKey) != null)
            {
                IceTask t = u.taskController.GetTask(TaskKey) as IceTask;
                t.TriggerIce();
            }
        }
        #endregion
    }

}
