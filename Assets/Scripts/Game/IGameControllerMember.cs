using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameControllerMember
{
    //初始化方法
    public void MInit();
    //帧更新方法
    public void MUpdate();
    //暂停时的方法
    public void MPause();
    //恢复时的方法
    public void MResume();
    //销毁时的方法
    public void MDestory();
}
