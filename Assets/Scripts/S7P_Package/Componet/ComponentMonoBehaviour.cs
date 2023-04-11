using UnityEngine;
namespace S7P.Component
{
    /// <summary>
    /// 用途：有的Component会携带一个对象引用，并且全程由该Componet控制对象的生命周期
    /// 这种被利用的对象会挂上一个引用其Component的脚本（该脚本）
    /// 仅挂在对象上，作为连接Component与对象的桥梁
    /// </summary>
    public class ComponentMonoBehaviour : MonoBehaviour
    {
        public IComponent component;
    }
}

