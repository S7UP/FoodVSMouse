using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// �����������������ռ���
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
            // �����ֵ�仯����Զ����»���
            modifier.AddBeforeValueChangeAction(ModifierBeforeValueChangeAction);
            modifier.AddAfterValueChangeAction(ModifierAfterValueChangeAction);
        }

        public void RemoveModifier(FloatModifier modifier)
        {
            // �Ƴ���ǰ�����������а�
            if (!Modifiers.Contains(modifier))
                return;
            // �Ƴ���ֵ�仯����Զ����»���
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

        #region ˽�з���
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

