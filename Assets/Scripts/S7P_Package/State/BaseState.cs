using System;
using System.Collections.Generic;
namespace S7P.State
{
    public class BaseState
    {
        public string name;
        public BaseState lastState;

        private List<Action<BaseState>> OnEnterActionList = new List<Action<BaseState>>();
        private List<Action<BaseState>> OnUpdateActionList = new List<Action<BaseState>>();
        private List<Action<BaseState>> OnExitActionList = new List<Action<BaseState>>();
        private List<Action<BaseState>> OnPauseActionList = new List<Action<BaseState>>();
        private List<Action<BaseState>> OnResumeActionList = new List<Action<BaseState>>();
        private List<Action<BaseState>> OnDestoryActionList = new List<Action<BaseState>>();
        private bool isDestory;

        public void OnEnter()
        {
            foreach (var action in OnEnterActionList)
            {
                action(this);
            }
        }

        public void OnUpdate()
        {
            foreach (var action in OnUpdateActionList)
            {
                action(this);
            }
        }

        public void OnExit()
        {
            if (!isDestory)
            {
                foreach (var action in OnExitActionList)
                {
                    action(this);
                }
                OnDestory();
            }
        }

        public void OnPause()
        {
            foreach (var action in OnPauseActionList)
            {
                action(this);
            }
        }

        public void OnResume()
        {
            foreach (var action in OnResumeActionList)
            {
                action(this);
            }
        }

        public void OnDestory()
        {
            if (!isDestory)
            {
                foreach (var action in OnDestoryActionList)
                {
                    action(this);
                }
                isDestory = true;
            }
        }

        public bool IsDestory()
        {
            return isDestory;
        }

        #region Ìí¼Ó¡¢É¾³ý¼àÌý
        public void AddOnEnterAction(Action<BaseState> action)
        {
            OnEnterActionList.Add(action);
        }

        public void RemoveOnEnterAction(Action<BaseState> action)
        {
            OnEnterActionList.Remove(action);
        }

        public void AddOnUpdateAction(Action<BaseState> action)
        {
            OnUpdateActionList.Add(action);
        }

        public void RemoveOnUpdateAction(Action<BaseState> action)
        {
            OnUpdateActionList.Remove(action);
        }

        public void AddOnExitAction(Action<BaseState> action)
        {
            OnExitActionList.Add(action);
        }

        public void RemoveOnExitAction(Action<BaseState> action)
        {
            OnExitActionList.Remove(action);
        }

        public void AddOnPauseAction(Action<BaseState> action)
        {
            OnPauseActionList.Add(action);
        }

        public void RemoveOnPauseAction(Action<BaseState> action)
        {
            OnPauseActionList.Remove(action);
        }

        public void AddOnResumeAction(Action<BaseState> action)
        {
            OnResumeActionList.Add(action);
        }

        public void RemoveOnResumeAction(Action<BaseState> action)
        {
            OnResumeActionList.Remove(action);
        }

        public void AddOnDestoryAction(Action<BaseState> action)
        {
            OnDestoryActionList.Add(action);
        }

        public void RemoveOnDestoryAction(Action<BaseState> action)
        {
            OnDestoryActionList.Remove(action);
        }
        #endregion
    }
}
