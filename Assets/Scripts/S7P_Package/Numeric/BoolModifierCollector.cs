using System.Collections.Generic;
/// <summary>
/// �ҹ�ϵ���������ռ���
/// </summary>
namespace S7P.Numeric
{
    public class BoolModifierCollector
    {
        // ������Ϊ��ʱ��TotalValueĬ��Ϊfalse
        public bool TotalValue { get; private set; }
        private List<BoolModifier> Modifiers = new List<BoolModifier>();

        public void AddModifier(BoolModifier modifier)
        {
            Modifiers.Add(modifier);
            Update();
        }

        public void RemoveModifier(BoolModifier modifier)
        {
            // �Ƴ���ǰ�����������а�
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
