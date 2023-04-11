using System;
using System.Collections.Generic;
/// <summary>
/// 附加在单位上的组件
/// </summary>
namespace S7P.Component
{
    public abstract class IComponent
    {
        private List<Action> InitialActionList = new List<Action>();
        private List<Action> UpdateActionList = new List<Action>();
        private List<Action> PauseUpdateActionList = new List<Action>();
        private List<Action> ExitActionList = new List<Action>();
        private List<Action> DestoryActionList = new List<Action>();
        private List<Func<bool>> IsPauseUpdateFuncList = new List<Func<bool>>();
        private bool isExit = false;
        private bool isDestory = false;

        public void Initial()
        {
            IsPauseUpdateFuncList.Clear();
            isExit = false;
            isDestory = false;
            OnInitial();
            foreach (var action in InitialActionList)
            {
                action();
            }
        }

        public void Update()
        {
            if (!IsPauseUpdate())
            {
                OnUpdate();
                foreach (var action in UpdateActionList)
                {
                    action();
                }
            }
            else
            {
                OnPauseUpdate();
                foreach (var action in PauseUpdateActionList)
                {
                    action();
                }
            }
        }

        /// <summary>
        /// 在正常逻辑退出时调用的，若已被销毁则不可用
        /// </summary>
        public void Exit()
        {
            if (!isExit && !isDestory)
            {
                OnExit();
                foreach (var action in ExitActionList)
                {
                    action();
                }
                Destory();
                isExit = true;
            }
        }

        /// <summary>
        /// 在正常逻辑退出后调用，或者手动销毁时调用
        /// </summary>
        public void Destory()
        {
            if (!isDestory)
            {
                OnDestory();
                foreach (var action in DestoryActionList)
                {
                    action();
                }
                isDestory = true;
            }
        }

        /// <summary>
        /// 每帧在Update前判断，如果为false则跳过本次由子类继承的Update
        /// </summary>
        private bool IsPauseUpdate()
        {
            foreach (var func in IsPauseUpdateFuncList)
            {
                if (func())
                    return true;
            }
            return false;
        }

        #region 供外界调用的方法
        /// <summary>
        /// 该组件是否已被移除（指调用过Exit方法）
        /// </summary>
        /// <returns></returns>
        public bool IsExit()
        {
            return isExit;
        }

        public bool IsDestory()
        {
            return isDestory;
        }
        #endregion

        #region 子类重写的方法
        protected abstract void OnInitial();

        protected abstract void OnUpdate();

        protected abstract void OnPauseUpdate();

        protected abstract void OnExit();

        protected abstract void OnDestory();
        #endregion

        #region 一些委托的添加与移除
        public void AddIsPauseUpdateFunc(Func<bool> func)
        {
            IsPauseUpdateFuncList.Add(func);
        }

        public void RemoveIsPauseUpdateFunc(Func<bool> func)
        {
            IsPauseUpdateFuncList.Remove(func);
        }

        public void AddInitialAction(Action action)
        {
            InitialActionList.Add(action);
        }

        public void RemoveInitialAction(Action action)
        {
            InitialActionList.Remove(action);
        }

        public void AddUpdateAction(Action action)
        {
            UpdateActionList.Add(action);
        }

        public void RemoveUpdateAction(Action action)
        {
            UpdateActionList.Remove(action);
        }

        public void AddPauseUpdateAction(Action action)
        {
            PauseUpdateActionList.Add(action);
        }

        public void RemovePauseUpdateAction(Action action)
        {
            PauseUpdateActionList.Remove(action);
        }

        public void AddExitAction(Action action)
        {
            ExitActionList.Add(action);
        }

        public void RemoveExitAction(Action action)
        {
            ExitActionList.Remove(action);
        }

        public void AddDestoryAction(Action action)
        {
            DestoryActionList.Add(action);
        }

        public void RemoveDestoryAction(Action action)
        {
            DestoryActionList.Remove(action);
        }
        #endregion
    }
}
