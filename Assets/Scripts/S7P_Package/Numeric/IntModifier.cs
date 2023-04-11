using System.Collections.Generic;
using System;
/// <summary>
/// ÕûÐÎÐÞÊÎÆ÷
/// </summary>
namespace S7P.Numeric
{
    public class IntModifier
    {
        private int _Value;
        public int Value
        {
            get { return _Value; }
            set
            {
                foreach (var action in BeforeValueChangeActionList)
                {
                    action(this);
                }
                _Value = value;
                foreach (var action in AfterValueChangeActionList)
                {
                    action(this);
                }
            }
        }
        private List<Action<IntModifier>> BeforeValueChangeActionList = new List<Action<IntModifier>>();
        private List<Action<IntModifier>> AfterValueChangeActionList = new List<Action<IntModifier>>();

        public IntModifier(int value)
        {
            Value = value;
        }

        public void AddBeforeValueChangeAction(Action<IntModifier> action)
        {
            BeforeValueChangeActionList.Add(action);
        }

        public void RemoveBeforeValueChangeAction(Action<IntModifier> action)
        {
            BeforeValueChangeActionList.Remove(action);
        }

        public void ClearBeforeValueChangeAction()
        {
            BeforeValueChangeActionList.Clear();
        }

        public void AddAfterValueChangeAction(Action<IntModifier> action)
        {
            AfterValueChangeActionList.Add(action);
        }

        public void RemoveAfterValueChangeAction(Action<IntModifier> action)
        {
            AfterValueChangeActionList.Remove(action);
        }

        public void ClearAfterValueChangeAction()
        {
            AfterValueChangeActionList.Clear();
        }
    }

}
