using System.Collections.Generic;

using UnityEngine;

namespace ResourceLoading.Mouse
{
    /// <summary>
    /// ����λ�ļ��ط�ʽ
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
                Debug.LogError("�����Դ���ظ����أ�");
        }
        public void UnLoad(int shape)
        {
            if (loadShapeList.Contains(shape))
            {
                O_UnLoad(shape);
                loadShapeList.Remove(shape);
            }
            else
                Debug.LogError("�����Դδ���ؾ͵���ж�أ�");
        }

        protected abstract void O_Load(int shape);

        protected abstract void O_UnLoad(int shape);
    }
}

