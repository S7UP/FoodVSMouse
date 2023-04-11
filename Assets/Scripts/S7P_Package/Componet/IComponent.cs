using System;
using System.Collections.Generic;
/// <summary>
/// �����ڵ�λ�ϵ����
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
        /// �������߼��˳�ʱ���õģ����ѱ������򲻿���
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
        /// �������߼��˳�����ã������ֶ�����ʱ����
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
        /// ÿ֡��Updateǰ�жϣ����Ϊfalse����������������̳е�Update
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

        #region �������õķ���
        /// <summary>
        /// ������Ƿ��ѱ��Ƴ���ָ���ù�Exit������
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

        #region ������д�ķ���
        protected abstract void OnInitial();

        protected abstract void OnUpdate();

        protected abstract void OnPauseUpdate();

        protected abstract void OnExit();

        protected abstract void OnDestory();
        #endregion

        #region һЩί�е�������Ƴ�
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
