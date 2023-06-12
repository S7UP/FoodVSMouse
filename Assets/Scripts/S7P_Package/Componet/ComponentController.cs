using System.Collections.Generic;
/// <summary>
/// 组件管理器
/// </summary>
namespace S7P.Component
{
    public class ComponentController
    {
        private List<IComponent> componentList = new List<IComponent>();
        private Dictionary<string, IComponent> componentDict = new Dictionary<string, IComponent>();
        private List<IComponent> delList = new List<IComponent>();

        public void Initial()
        {
            componentList.Clear();
            componentDict.Clear();
            delList.Clear();
        }

        public void Update()
        {
            foreach (var c in componentList)
            {
                if (c.IsExit() || c.IsDestory())
                    delList.Add(c);
                else
                    c.Update();
            }
            foreach (var c in delList)
            {
                componentList.Remove(c);
            }
            delList.Clear();
        }

        public void Destory()
        {
            foreach (var c in componentList)
            {
                c.Destory();
            }
            componentList.Clear();
            componentDict.Clear();
            delList.Clear();
        }

        #region 添加或移除组件方法
        public void Add(IComponent component)
        {
            componentList.Add(component);
            component.Initial();
        }

        public void Add(string key, IComponent component)
        {
            componentList.Add(component);
            componentDict.Add(key, component);
            component.Initial();
        }

        public void Remove(IComponent component)
        {
            bool flag = componentList.Remove(component);
            string key = null;
            foreach (var keyValuePair in componentDict)
            {
                if (component == keyValuePair.Value)
                {
                    key = keyValuePair.Key;
                    break;
                }
            }
            if (key != null)
                componentDict.Remove(key);
            if (flag)
            {
                component.Exit();
            }
        }

        public void Remove(string key)
        {
            if (componentDict.ContainsKey(key))
            {
                IComponent c = componentDict[key];
                componentDict.Remove(key);
                if (componentList.Remove(c))
                    c.Exit();
            }
        }
        #endregion

        /// <summary>
        /// 尝试获取一个组件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool TryGet(string key, out IComponent component)
        {
            if (componentDict.ContainsKey(key))
            {
                component = componentDict[key];
                return true;
            }
            else
            {
                component = null;
                return false;
            }
        }
    }
}
