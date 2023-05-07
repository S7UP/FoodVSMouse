using System.Collections.Generic;
using UnityEngine;

namespace S7P.Numeric
{
    /// <summary>
    /// �������������ռ���
    /// </summary>
    public class IntModifierCollector
    {
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }
        public int TotalValue { get; private set; }
        public float MulValue { get; private set; }
        private List<IntModifier> Modifiers = new List<IntModifier>();

        public IntModifierCollector()
        {
            MinValue = int.MaxValue;
            MaxValue = int.MinValue;
            TotalValue = 0;
            MulValue = 1;
        }

        public void Clear()
        {
            Modifiers.Clear();
            MinValue = int.MaxValue;
            MaxValue = int.MinValue;
            TotalValue = 0;
            MulValue = 1;
        }

        public void AddModifier(IntModifier modifier)
        {
            Modifiers.Add(modifier);
            TotalValue += modifier.Value;
            MinValue = Mathf.Min(modifier.Value, MinValue);
            MaxValue = Mathf.Max(modifier.Value, MaxValue);
            MulValue *= modifier.Value;
            // �����ֵ�仯����Զ����»���
            modifier.AddBeforeValueChangeAction(ModifierBeforeValueChangeAction);
            modifier.AddAfterValueChangeAction(ModifierAfterValueChangeAction);
        }

        public void RemoveModifier(IntModifier modifier)
        {            
            // �Ƴ���ǰ�����������а�
            if (!Modifiers.Contains(modifier))
                return;
            // �Ƴ���ֵ�仯����Զ����»���
            modifier.RemoveBeforeValueChangeAction(ModifierBeforeValueChangeAction);
            modifier.RemoveAfterValueChangeAction(ModifierAfterValueChangeAction);

            Modifiers.Remove(modifier);
            TotalValue -= modifier.Value;
            if (modifier.Value != 0)
                MulValue /= modifier.Value;
            else
            {
                MulValue = 1;
                foreach (var m in Modifiers)
                {
                    MulValue *= m.Value;
                }
            }
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

        #region ˽�з���
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
            if (mod.Value != 0)
                MulValue /= mod.Value;
            else
            {
                MulValue = 1;
                foreach (var m in Modifiers)
                {
                    if (m != mod)
                        MulValue *= m.Value;
                }
            }
            UpdateMinAndMaxValueWhenRemoveModifier(mod);
        }

        private void ModifierAfterValueChangeAction(IntModifier mod)
        {
            TotalValue += mod.Value;
            MulValue *= mod.Value;
            MinValue = Mathf.Min(mod.Value, MinValue);
            MaxValue = Mathf.Max(mod.Value, MaxValue);
        }
        #endregion
    }
}
