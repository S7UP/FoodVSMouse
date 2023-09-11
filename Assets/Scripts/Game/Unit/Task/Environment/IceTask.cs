using S7P.Numeric;

using System;

using UnityEngine;

namespace Environment
{
    public class IceTask : BaseTask
    {

        private const float decValue = -25f / 60; // 凝结情况下每帧下降速度
        private const string TaskKey = "IceTask";

        private BaseUnit master;
        private MultiplyFloatModifierCollector DecRate = new MultiplyFloatModifierCollector();
        private float value; // 当前积累值
        private bool isForzen; // 是否触发凝结
        private FloatModifier decAttackSpeedMod = new FloatModifier(-34f); // 减攻速

        private Action<BaseUnit> OnEnterWaterAction;
        private Action<BaseUnit> OnExitWaterAction;

        private Action<BaseUnit> OnEnterLavaAction;
        private Action<BaseUnit> OnStayLavaAction;
        private Action<BaseUnit> OnExitLavaAction;

        private BaseEffect eff; // 冰雕特效
        private float gobal_alpha; // 全局透明度

        public IceTask(BaseUnit master, float value)
        {
            gobal_alpha = 0;
            this.value = value;
            this.master = master;
            isForzen = false;

            // 与水的交互
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

            // 与岩浆的交互
            {
                FloatModifier IceDecRateMod = new FloatModifier(5);
                FloatModifier LavaRateMod = new FloatModifier(2);
                OnEnterLavaAction = (u) =>
                {
                    // 消耗所有冰冻损伤使目标立即受到岩浆伤害
                    new DamageAction(CombatAction.ActionType.BurnDamage, null, master, this.value * 0.004f * master.mMaxHp).ApplyAction();
                    this.value = 0;
                };
                OnStayLavaAction = (u) =>
                {
                    // 消耗所有冰冻损伤使目标立即受到岩浆伤害
                    new DamageAction(CombatAction.ActionType.BurnDamage, null, master, this.value * 0.004f * master.mMaxHp).ApplyAction();
                    this.value = 0;
                };
                OnExitLavaAction = (u) =>
                {

                };
            }

            // 冰雕特效生成
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
                // 记录一个困扰了一周的BUG：2023.6.10
                // 如果特效在目标被彻底回收之前就被回收了，那么要取消目标回收时再回收特效的监听
                // 因为如果不取消，会发生特效错误引用的BUG，比如小火贴图飞到敌怪身上，更严重点会引发一系列逻辑错误
                // 正确逻辑是这样的：特效被回收->目标被回收（调用了目标回收时回收特效的方法，但因为特效本来就被回收了所以无事发生）->特效被重新引用、
                // 但异常的逻辑是：特效被回收->特效被重新引用->目标被回收（调用了目标回收时回收特效的方法，但特效被引用到其他地方去，然后又被错误得回收了）->特效被回收->特效再被引用，但此时特效同时被两个Task引用，这显然是不对的！
                // 于是，只要加上下面这行，在特效被回收时，移除目标被回收时回收特效的监听，即可破局！
            }

        }

        #region 由子类继承
        protected override void O_OnEnter()
        {
            // 与水的交互
            {
                master.actionPointController.AddAction("OnEnterWater", OnEnterWaterAction);
                master.actionPointController.AddAction("OnExitWater", OnExitWaterAction);

                // 被施加时检测一下自身在不在水里
                if (master.taskController.GetTask("WaterTask") != null)
                {
                    master.actionPointController.TriggerAction("OnEnterWater");
                }
            }

            // 与岩浆的交互
            {
                master.actionPointController.AddAction("OnEnterLava", OnEnterLavaAction);
                master.actionPointController.AddAction("OnStayLava", OnStayLavaAction);
                master.actionPointController.AddAction("OnExitLava", OnExitLavaAction);

                // 被施加时检测一下自身在不在岩浆里
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
                TriggerIce(); // 自我触发凝结
            }

            if (isForzen)
            {
                master.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(master, 2, false));
                value += decValue * DecRate.TotalValue;
            }

            // 更新冰雕显示
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
            // 与水的交互
            {
                master.actionPointController.RemoveAction("OnEnterWater", OnEnterWaterAction);
                master.actionPointController.RemoveAction("OnExitWater", OnExitWaterAction);
            }

            // 与岩浆的交互
            {
                master.actionPointController.RemoveAction("OnEnterLava", OnEnterLavaAction);
                master.actionPointController.RemoveAction("OnExitLava", OnExitLavaAction);
            }

            // 冰雕特效移除
            {
                eff.ExecuteDeath();
            }

            // 减攻速特效移除
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
        /// 增加一定冰冻值
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
        /// 是否触发凝结
        /// </summary>
        /// <returns></returns>
        public bool IsForzen()
        {
            return isForzen;
        }

        /// <summary>
        /// 触发凝结
        /// </summary>
        public void TriggerIce()
        {
            isForzen = true;
            master.NumericBox.AttackSpeed.AddFinalPctAddModifier(decAttackSpeedMod);
        }

        #region 对外开放的静态方法
        /// <summary>
        /// 凝结效果
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
