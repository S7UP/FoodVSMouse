using System.Collections.Generic;

using UnityEngine;

namespace ResourceLoading.Mouse
{
    /// <summary>
    /// 老鼠单位的加载方式
    /// </summary>
    public abstract class IMouse
    {
        private List<int> loadShapeList = new List<int>();

        public void Load(int shape)
        {
            if (!loadShapeList.Contains(shape))
            {
                O_Load(shape);
                loadShapeList.Add(shape);
            }
            else
                Debug.LogError("相关资源已重复加载！");
        }
        public void UnLoad(int shape)
        {
            if (loadShapeList.Contains(shape))
            {
                O_UnLoad(shape);
                loadShapeList.Remove(shape);
            }
            else
                Debug.LogError("相关资源未加载就调用卸载！");
        }

        protected abstract void O_Load(int shape);

        protected abstract void O_UnLoad(int shape);
    }
}

