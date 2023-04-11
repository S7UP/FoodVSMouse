using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 单浮点形修饰器的收集器
/// </summary>
namespace S7P.Numeric
{
    public class FloatModifierCollector
    {
        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }
        public float TotalValue { get; private set; }
        private List<FloatModifier> Modifiers = new List<FloatModifier>();

        public FloatModifierCollector()
        {
            MinValue = float.MaxValue;
            MaxValue = float.MinValue;
            TotalValue = 0;
        }

        public void Clear()
        {
            Modifiers.Clear();
            MinValue = float.MaxValue;
            MaxValue = float.MinValue;
            TotalValue = 0;
        }

        public void AddModifier(FloatModifier modifier)
        {
            Modifiers.Add(modifier);
            TotalValue += modifier.Value;
            MinValue = Mathf.Min(modifier.Value, MinValue);
            MaxValue = Mathf.Max(modifier.Value, MaxValue);
            // 添加数值变化检测自动更新机制
            modifier.AddBeforeValueChangeAction(ModifierBeforeValueChangeAction);
            modifier.AddAfterValueChangeAction(ModifierAfterValueChangeAction);
        }

        public void RemoveModifier(FloatModifier modifier)
        {
            // 移除的前提是你必须得有啊
            if (!Modifiers.Contains(modifier))
                return;
            // 移除数值变化检测自动更新机制
            modifier.RemoveBeforeValueChangeAction(ModifierBeforeValueChangeAction);
            modifier.RemoveAfterValueChangeAction(ModifierAfterValueChangeAction);

            Modifiers.Remove(modifier);
            TotalValue -= modifier.Value;
            UpdateMinAndMaxValueWhenRemoveModifier(modifier);
        }

        public FloatModifier GetModifier(int index)
        {
            if(Modifiers.Count > index)
            {
                return Modifiers[index];
            }
            else
            {
                return null;
            }
        }

        public List<FloatModifier> GetModifierList()
        {
            return Modifiers;
        }

        #region 私有方法
        private void UpdateMinAndMaxValueWhenRemoveModifier(FloatModifier modifier)
        {
            if (MinValue == modifier.Value)
            {
                MinValue = float.MaxValue;
                foreach (var mod in Modifiers)
                    if (mod.Value < MinValue)
                        MinValue = mod.Value;
            }

            if (MaxValue == modifier.Value)
            {
                MaxValue = float.MinValue;
                foreach (var mod in Modifiers)
                    if (mod.Value > MaxValue)
                        MaxValue = mod.Value;
            }
        }

        private void ModifierBeforeValueChangeAction(FloatModifier mod)
        {
            TotalValue -= mod.Value;
            UpdateMinAndMaxValueWhenRemoveModifier(mod);
        }

        private void ModifierAfterValueChangeAction(FloatModifier mod)
        {
            TotalValue += mod.Value;
            MinValue = Mathf.Min(mod.Value, MinValue);
            MaxValue = Mathf.Max(mod.Value, MaxValue);
        }
        #endregion
    }
}

