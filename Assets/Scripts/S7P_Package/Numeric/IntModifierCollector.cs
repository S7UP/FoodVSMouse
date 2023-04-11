using System.Collections.Generic;
using UnityEngine;

namespace S7P.Numeric
{
    /// <summary>
    /// 整形修饰器的收集器
    /// </summary>
    public class IntModifierCollector
    {
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }
        public int TotalValue { get; private set; }
        private List<IntModifier> Modifiers = new List<IntModifier>();

        public IntModifierCollector()
        {
            MinValue = int.MaxValue;
            MaxValue = int.MinValue;
            TotalValue = 0;
        }

        public void Clear()
        {
            Modifiers.Clear();
            MinValue = int.MaxValue;
            MaxValue = int.MinValue;
            TotalValue = 0;
        }

        public void AddModifier(IntModifier modifier)
        {
            Modifiers.Add(modifier);
            TotalValue += modifier.Value;
            MinValue = Mathf.Min(modifier.Value, MinValue);
            MaxValue = Mathf.Max(modifier.Value, MaxValue);
            // 添加数值变化检测自动更新机制
            modifier.AddBeforeValueChangeAction(ModifierBeforeValueChangeAction);
            modifier.AddAfterValueChangeAction(ModifierAfterValueChangeAction);
        }

        public void RemoveModifier(IntModifier modifier)
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

        public IntModifier GetModifier(int index)
        {
            if (Modifiers.Count > index)
            {
                return Modifiers[index];
            }
            else
            {
                return null;
            }
        }

        public List<IntModifier> GetModifierList()
        {
            return Modifiers;
        }

        #region 私有方法
        private void UpdateMinAndMaxValueWhenRemoveModifier(IntModifier modifier)
        {
            if (MinValue == modifier.Value)
            {
                MinValue = int.MaxValue;
                foreach (var mod in Modifiers)
                    if (mod.Value < MinValue)
                        MinValue = mod.Value;
            }

            if (MaxValue == modifier.Value)
            {
                MaxValue = int.MinValue;
                foreach (var mod in Modifiers)
                    if (mod.Value > MaxValue)
                        MaxValue = mod.Value;
            }
        }

        private void ModifierBeforeValueChangeAction(IntModifier mod)
        {
            TotalValue -= mod.Value;
            UpdateMinAndMaxValueWhenRemoveModifier(mod);
        }

        private void ModifierAfterValueChangeAction(IntModifier mod)
        {
            TotalValue += mod.Value;
            MinValue = Mathf.Min(mod.Value, MinValue);
            MaxValue = Mathf.Max(mod.Value, MaxValue);
        }
        #endregion
    }
}
