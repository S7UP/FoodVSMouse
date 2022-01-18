using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameControllerMember
{
    //初始化方法
    public void Init();
    //帧更新方法
    public void Update();
    //暂停时的方法
    public void Pause();
    //恢复时的方法
    public void Resume();
    //销毁时的方法
    public void Destory();
}
