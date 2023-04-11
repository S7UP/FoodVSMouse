using System;
using System.Collections.Generic;
/// <summary>
/// µ¥¸¡µãÐÎÐÞÊÎÆ÷
/// </summary>
namespace S7P.Numeric
{
    public class FloatModifier
    {
        private float _Value;
        public float Value { get { return _Value; } set {
                foreach (var action in BeforeValueChangeActionList)
                {
                    action(this);
                }
                _Value = value;
                foreach (var action in AfterValueChangeActionList)
                {
                    action(this);
                }
            } }
        private List<Action<FloatModifier>> BeforeValueChangeActionList = new List<Action<FloatModifier>>();
        private List<Action<FloatModifier>> AfterValueChangeActionList = new List<Action<FloatModifier>>();

        public FloatModifier(float value)
        {
            _Value = value;
        }

        public void AddBeforeValueChangeAction(Action<FloatModifier> action)
        {
            BeforeValueChangeActionList.Add(action);
        }

        public void RemoveBeforeValueChangeAction(Action<FloatModifier> action)
        {
            BeforeValueChangeActionList.Remove(action);
        }

        public void ClearBeforeValueChangeAction()
        {
            BeforeValueChangeActionList.Clear();
        }

        public void AddAfterValueChangeAction(Action<FloatModifier> action)
        {
            AfterValueChangeActionList.Add(action);
        }

        public void RemoveAfterValueChangeAction(Action<FloatModifier> action)
        {
            AfterValueChangeActionList.Remove(action);
        }

        public void ClearAfterValueChangeAction()
        {
            AfterValueChangeActionList.Clear();
        }
    }

}
