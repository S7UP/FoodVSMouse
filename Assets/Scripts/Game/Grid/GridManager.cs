using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// ���ӹ���������ž�̬������
/// </summary>
public class GridManager
{
    /// <summary>
    /// ��һ�����ӱ���ȡ�������������
    /// </summary>
    /// <param name="list">���ӱ�</param>
    /// <param name="count">ȡ���������</param>
    /// <returns></returns>
    public static List<BaseGrid> GetRandomUnitList(List<BaseGrid> list, int count)
    {
        List<BaseGrid> randList = new List<BaseGrid>();
        if (count <= 0 || list == null)
            return randList;

        // ����һ��List������ԭ����List����Ӱ��
        List<BaseGrid> cp_list = new List<BaseGrid>();
        foreach (var item in list)
        {
            cp_list.Add(item);
        }

        count = Mathf.Min(count, cp_list.Count);

        for (int i = 0; i < count; i++)
        {
            int index = GameController.Instance.GetRandomInt(0, cp_list.Count);
            randList.Add(cp_list[index]);
            cp_list.Remove(cp_list[index]);
        }
        return randList;
    }

    /// <summary>
    /// ��ȡ�ض���Χ�ڵ����и���
    /// </summary>
    /// <returns></returns>
    public static List<BaseGrid> GetSpecificAreaGridList(List<BaseGrid> gridList, float left, float right, float up, float bottom)
    {
        List<BaseGrid> l = new List<BaseGrid>();
        foreach (var g in gridList)
        {
            if (g.transform.position.x >= left && g.transform.position.x <= right && g.transform.position.y <= up && g.transform.position.y >= bottom)
                l.Add(g);
        }
        return l;
    }

    /// <summary>
    /// ��ȡ����ض����� ֵ��С �ĸ�����
    /// </summary>
    /// <param name="ConditionFunc"></param>
    /// <returns></returns>
    public static List<BaseGrid> GetGridListWhichHasMinCondition(List<BaseGrid> gridList, Func<BaseGrid, float> ConditionFunc)
    {
        List<BaseGrid> l = new List<BaseGrid>();
        float min = float.MaxValue;
        foreach (var g in gridList)
        {
            float c = ConditionFunc(g);
            if (c < min)
            {
                min = c;
                l.Clear();
                l.Add(g);
            }
            else if(c == min)
            {
                l.Add(g);
            }
        }
        return l;
    }


    /// <summary>
    /// ��ȡ����ض����� ֵ��� �ĸ�����
    /// </summary>
    /// <param name="ConditionFunc"></param>
    /// <returns></returns>
    public static List<BaseGrid> GetGridListWhichHasMaxCondition(List<BaseGrid> gridList, Func<BaseGrid, float> ConditionFunc)
    {
        List<BaseGrid> l = new List<BaseGrid>();
        float max = float.MinValue;
        foreach (var g in gridList)
        {
            float c = ConditionFunc(g);
            if (c > max)
            {
                max = c;
                l.Clear();
                l.Add(g);
            }
            else if (c == max)
            {
                l.Add(g);
            }
        }
        return l;
    }
}
