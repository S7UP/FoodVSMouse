using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ��ʳ��λ�ķ��ࣨ������ְҵϵͳ��
/// </summary>
public enum FoodType
{
    Producer, // �����ࣨ�����������Դ��
    Shooter, // �����ֱࣨ�߹����ͣ�
    Aoe, // Ⱥ���ࣨ�ڹ�����Χ�����ежԵ�λ�ܵ��ȶ�����˺���
    Pitcher, // Ͷ���ࣨͶ���͹�����
    Tracker, // ׷����
    Bomb, // ը����
    Support, // �����ࣨ�����������ܣ�
    Vehicle, // �ؾ��ࣨ��������γ��ؿ�Ƭ�Ŀ�Ƭ��
    Protect, // �����ࣨ��Ҫ����ռ��һ�����ӵķ����Ϳ�Ƭ��
    Barrier // �����ࣨ������������๲����һ�����ӵķ����Ϳ�Ƭ��
}
