using System;
using System.Collections.Generic;

using UnityEngine;
namespace S7P.State
{
    public class StateController
    {
        private Dictionary<string, Func<BaseState>> createStateFuncDict = new Dictionary<string, Func<BaseState>>();
        private BaseState currentState;

        private List<Action<StateController>> BeforeChangeStateActionList = new List<Action<StateController>>();
        private List<Action<StateController>> AfterChangeStateActionList = new List<Action<StateController>>();

        public void Initial()
        {
            createStateFuncDict.Clear();
            currentState = null;

            BeforeChangeStateActionList.Clear();
            AfterChangeStateActionList.Clear();
        }

        public void Update()
        {
            if(currentState != null)
            {
                currentState.OnUpdate();
            }
        }

        public void Destory()
        {
            if(currentState != null)
            {
                currentState.OnDestory();
            }
        }

        #region �������õķ���
        /// <summary>
        /// �����е��ֵ�������һ��״̬ȡ����ǰ״̬
        /// </summary>
        /// <param name="name"></param>
        public void ChangeState(string name)
        {
            if (createStateFuncDict.ContainsKey(name))
            {
                TriggerBeforeChangeStateAction();
                if (currentState!=null)
                    currentState.OnExit();
                
                BaseState new_state = createStateFuncDict[name]();
                new_state.lastState = currentState;
                new_state.name = name;
                new_state.OnEnter();
                currentState = new_state;
                TriggerAfterChangeStateAction();
            }
            else
            {
                Debug.LogWarning("��ǰ״̬�������ﲻ���ڴ���name='"+name+"'��״̬�ķ���");
            }
        }

        /// <summary>
        /// ����紫��һ��״̬��ȡ����ǰ״̬���������ã�
        /// </summary>
        /// <param name="new_state"></param>
        public void ChangeState(BaseState new_state)
        {
            TriggerBeforeChangeStateAction();
            if (currentState != null)
                currentState.OnExit();

            new_state.lastState = currentState;
            new_state.OnEnter();
            currentState = new_state;
            TriggerAfterChangeStateAction();
        }

        /// <summary>
        /// ��ǰ��״̬�Ƿ�Ϊ��Ϊname��״̬
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsCurrentState(string name)
        {
            if (currentState == null)
                return false;
            else
                return currentState.name.Equals(name);
        }

        /// <summary>
        /// ���һ������״̬�ķ���
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        public void AddCreateStateFunc(string key, Func<BaseState> func)
        {
            if(createStateFuncDict.ContainsKey(key))
                createStateFuncDict[key] = func;
            else
                createStateFuncDict.Add(key, func);
        }

        public void AddBeforeChangeStateAction(Action<StateController> action)
        {
            BeforeChangeStateActionList.Add(action);
        }

        public void RemoveBeforeChangeStateAction(Action<StateController> action)
        {
            BeforeChangeStateActionList.Remove(action);
        }

        public void AddAfterChangeStateAction(Action<StateController> action)
        {
            AfterChangeStateActionList.Add(action);
        }

        public void RemoveAfterChangeStateAction(Action<StateController> action)
        {
            AfterChangeStateActionList.Remove(action);
        }
        #endregion


        #region ˽�з���
        private void TriggerBeforeChangeStateAction()
        {
            foreach (var action in BeforeChangeStateActionList)
            {
                action(this);
            }
        }

        private void TriggerAfterChangeStateAction()
        {
            foreach (var action in AfterChangeStateActionList)
            {
                action(this);
            }
        }
        #endregion
    }
}

