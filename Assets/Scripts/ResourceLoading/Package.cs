using System;

namespace ResourceLoading
{
    /// <summary>
    /// 一个资源包，里面包含若干方法
    /// </summary>
    public class Package
    {
        public Action LoadAction; // 加载方法
        public Action UnLoadAction; // 卸载方法
    }

}
