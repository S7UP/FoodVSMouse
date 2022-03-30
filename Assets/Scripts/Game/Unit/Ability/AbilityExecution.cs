using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// 能力执行体，能力执行体是实际创建执行能力表现，触发应用能力效果的地方
/// 这里可以存一些表现执行相关的临时的状态数据
/// </summary>
public abstract class AbilityExecution
{
    // 开始执行
    public virtual void BeginExecute()
    {

    }

    // 执行过程
    public virtual void Update()
    {

    }

    // 结束执行
    public virtual void EndExecute()
    {
        
    }
}
