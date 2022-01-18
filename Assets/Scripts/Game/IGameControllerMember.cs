using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameControllerMember
{
    //��ʼ������
    public void Init();
    //֡���·���
    public void Update();
    //��ͣʱ�ķ���
    public void Pause();
    //�ָ�ʱ�ķ���
    public void Resume();
    //����ʱ�ķ���
    public void Destory();
}
