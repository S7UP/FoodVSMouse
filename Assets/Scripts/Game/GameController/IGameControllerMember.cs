using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameControllerMember
{
    //��ʼ������
    public void MInit();
    //֡���·���
    public void MUpdate();
    //��ͣʱ�ķ���
    public void MPause();
    //�ָ�ʱ�ķ���
    public void MResume();
    //����ʱ�ķ���
    public void MDestory();
}
