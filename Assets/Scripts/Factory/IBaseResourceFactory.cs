using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ����������Դ�����Ľӿڣ�ÿ�ֹ�����ȡ����Դ����ͬ�����������÷��ͽӿ�
public interface IBaseResourceFactory<T>
{
    T GetSingleResources(string resourcePath);
}
