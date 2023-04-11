using System.Collections.Generic;
/// <summary>
/// 且关系的修饰器收集器
/// </summary>
namespace S7P.Numeric
{
    public class BoolModifierCollector
    {
        // 当集合为空时，TotalValue默认为false
        public bool TotalValue { get; private set; }
        private List<BoolModifier> Modifiers = new List<BoolModifier>();

        public void AddModifier(BoolModifier modifier)
        {
            Modifiers.Add(modifier);
            Update();
        }

        public void RemoveModifier(BoolModifier modifier)
        {
            // 移除的前提是你必须得有啊
            if (!Modifiers.Contains(modifier))
                return;

            Modifiers.Remove(modifier);
            Update();
        }

        public List<BoolModifier> GetModifierList()
        {
            return Modifiers;
        }

        public void Update()
        {
            if (Modifiers.Count <= 0)
            {
                TotalValue = false;
            }
            else
            {
                TotalValue = true;
                foreach (var item in Modifiers)
                {
                    if (!item.Value)
                    {
                        TotalValue = false;
                        break;
                    }
                }
            }
        }
    }
}
