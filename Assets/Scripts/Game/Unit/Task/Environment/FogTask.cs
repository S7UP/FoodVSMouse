using S7P.Numeric;

using System;

using UnityEngine;
namespace Environment
{
    public class FogTask : ITask
    {
        private static FloatModifier mouseMoveSpeedModifier = new FloatModifier(-20); // 老鼠在雾中减速
        private static Texture[] texArray;

        // 禁止阻挡方法
        private static Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate
        {
            return false;
        };
        // 禁止被弹幕攻击方法
        private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate
        {
            return false;
        };
        // 禁止被选取的方法
        private static Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };

        private int count; // 进入的迷雾数
        private BaseUnit unit;
        private BaseEffect eff;
        private float current_alpha;
        private float gobal_alpha;
        private int timer;

        public FogTask(BaseUnit unit)
        {
            if (texArray == null)
            {
                texArray = new Texture[13];
                for (int i = 0; i < texArray.Length; i++)
                {
                    texArray[i] = GameManager.Instance.GetSprite("Effect/Hidden/" + (i + 1)).texture;
                }
            }

            this.unit = unit;
            gobal_alpha = 0;
            current_alpha = 0;
            timer = 0;

            // 隐匿特效生成
            {
                eff = BaseEffect.CreateInstance(unit.GetSpriteRenderer().sprite);
                eff.spriteRenderer.sortingLayerName = unit.GetSpriteRenderer().sortingLayerName;
                eff.spriteRenderer.sortingOrder = unit.GetSpriteRenderer().sortingOrder;
                eff.spriteRenderer.material = GameManager.Instance.GetMaterial("Hide");
                eff.spriteRenderer.material.SetFloat("_Alpha", 0);
                GameController.Instance.AddEffect(eff);
                eff.AddSetAlphaAction((alpha) => {
                    gobal_alpha = alpha;
                });
                unit.mEffectController.AddEffectToGroup("Skin", 1, eff);
                eff.transform.localPosition = Vector2.zero;
                eff.transform.localScale = Vector2.one;
            }
        }

        public void OnEnter()
        {
            count = 1;
            if (unit.mUnitType == UnitType.Food || unit.mUnitType == UnitType.Character)
            {

            }
            else if (unit.mUnitType == UnitType.Mouse)
            {
                // 减速
                unit.NumericBox.MoveSpeed.AddFinalPctAddModifier(mouseMoveSpeedModifier);
                // 立即取消目标的阻挡状态
                MouseUnit m = unit as MouseUnit;
                m.SetNoCollideAllyUnit();
            }
            // 使得目标既不可被阻挡也不可被弹幕攻击
            unit.AddCanBlockFunc(noBlockFunc);
            unit.AddCanHitFunc(noHitFunc);
            unit.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
        }

        public void OnUpdate()
        {
            timer++;
            eff.spriteRenderer.sprite = unit.GetSpirte();
            eff.spriteRenderer.sortingLayerName = unit.GetSpriteRenderer().sortingLayerName;
            eff.spriteRenderer.sortingOrder = unit.GetSpriteRenderer().sortingOrder;
            current_alpha = Mathf.Min(1, current_alpha + 0.033f);
            eff.spriteRenderer.material.SetFloat("_Alpha", current_alpha * gobal_alpha);
            eff.spriteRenderer.material.SetTexture("_FogTex", texArray[(timer / 5) % 13]);
        }

        public bool IsMeetingExitCondition()
        {
            return count == 0 || !unit.IsAlive();
        }

        public void OnExit()
        {
            if (unit.mUnitType == UnitType.Food || unit.mUnitType == UnitType.Character)
            {

            }
            else if (unit.mUnitType == UnitType.Mouse)
            {
                unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(mouseMoveSpeedModifier);
            }
            // 取消以上特性
            unit.RemoveCanBlockFunc(noBlockFunc);
            unit.RemoveCanHitFunc(noHitFunc);
            unit.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
            // 隐匿特效移除
            {
                eff.ExecuteDeath();
            }
        }

        // 自定义方法
        public void AddCount()
        {
            count++;
        }

        public void DecCount()
        {
            count--;
        }

        public void ShutDown()
        {

        }

        public bool IsClearWhenDie()
        {
            return true;
        }
    }
}

