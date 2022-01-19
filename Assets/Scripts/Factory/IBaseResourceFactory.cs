using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 其他种类资源工厂的接口，每种工厂获取的资源都不同，所以我们用泛型接口
public interface IBaseResourceFactory<T>
{
    T GetSingleResources(string resourcePath);
}
