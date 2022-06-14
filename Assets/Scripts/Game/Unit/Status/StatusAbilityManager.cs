using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

/// <summary>
/// ʱЧ״̬��BUFF���������������ڵ�λ�ϵģ�����ÿ����λ������ִ��BUFFЧ���Ķ���
/// </summary>
public sealed class StatusAbilityManager
{
    public List<StatusAbility> statusAbilityList = new List<StatusAbility>();
    public List<StatusAbility> removeList = new List<StatusAbility>();
    // Ψһ��buff�������ж��ʩ��Դ����Ч����һ����ͬ��������ʩ��Դ��ʧʱbuff�Ż�ʧЧ��
    public class UniqueStatusAbility
    {
        public StatusAbility status;
        public int count; // ��ǰʩ�Ӵ�״̬buff��

        public void SetStatus(StatusAbility s) 
        {
            status = s;
        }
        public void AddCount()
        {
            count++;
        }
        public void DecCount()
        {
            count--;
        }
    }
    public Dictionary<string, UniqueStatusAbility> uniqueStatusAbilityDict = new Dictionary<string, UniqueStatusAbility>(); 
    // ��������Ψһ��buff
    public Dictionary<string, StatusAbility> noCountUniqueStatusAbilityDict = new Dictionary<string, StatusAbility>();

    public void Initialize()
    {
        statusAbilityList.Clear();
        removeList.Clear();
        noCountUniqueStatusAbilityDict.Clear();
    }


    /// <summary>
    /// ��BUFF����ʱ������ζ�ż�����
    /// </summary>
    /// <param name="statusAbility"></param>
    public void AddStatusAbility(StatusAbility statusAbility)
    {
        statusAbility.statusAbilityManager = this;
        statusAbility.ActivateAbility();
        statusAbilityList.Add(statusAbility);
    }

    public void RemoveStatusAbility(StatusAbility statusAbility)
    {
        removeList.Add(statusAbility);
        //statusAbilityList.Remove(statusAbility);
    }

    public StatusAbility GetUniqueStatus(string statusName) 
    {
        if (!uniqueStatusAbilityDict.ContainsKey(statusName))
        {
            return null;
        }
        return uniqueStatusAbilityDict[statusName].status;
    }

    /// <summary>
    /// ���һ��Ψһ��buff
    /// </summary>
    /// <param name="statusName"></param>
    /// <param name="statusAbility"></param>
    public void AddUniqueStatusAbility(string statusName, StatusAbility statusAbility)
    {
        if (!uniqueStatusAbilityDict.ContainsKey(statusName))
        {
            // �����һ��ʩ�Ӵ�Ч��
            uniqueStatusAbilityDict.Add(statusName, new UniqueStatusAbility() { status= statusAbility , count=0});
            statusAbility.statusAbilityManager = this;
            // ����
            statusAbility.ActivateAbility();
            // ���������������ִ��
            statusAbilityList.Add(statusAbility); 
        }
        // ��������²���+1���ø�ɶ��ɶ
        uniqueStatusAbilityDict[statusName].AddCount();
        //Debug.Log("statusName="+ statusName+", count="+ uniqueStatusAbilityDict[statusName].count);
    }

    /// <summary>
    /// �Ƴ�һ��Ψһ��buff
    /// </summary>
    /// <param name="statusName"></param>
    public void RemoveUniqueStatusAbility(string statusName)
    {
        if (uniqueStatusAbilityDict.ContainsKey(statusName))
        {
            // ����-1
            uniqueStatusAbilityDict[statusName].DecCount();
            // ������0�㣬��ֱ���Ƴ���Ч��
            if (uniqueStatusAbilityDict[statusName].count <= 0)
            {
                StatusAbility statusAbility = uniqueStatusAbilityDict[statusName].status;
                //RemoveStatusAbility(statusAbility);
                statusAbility.EndActivate(); // �ֶ�����
                uniqueStatusAbilityDict.Remove(statusName);
            }
        }
    }

    /// <summary>
    /// ͨ��key����ȡ��������Ψһ��״̬ʵ��
    /// </summary>
    /// <param name="statusName"></param>
    /// <returns></returns>
    public StatusAbility GetNoCountUniqueStatus(string statusName)
    {
        if (!noCountUniqueStatusAbilityDict.ContainsKey(statusName))
        {
            return null;
        }
        return noCountUniqueStatusAbilityDict[statusName];
    }

    /// <summary>
    /// ��Ӳ�������Ψһ��buff
    /// </summary>
    /// <param name="statusName"></param>
    /// <param name="statusAbility"></param>
    public void AddNoCountUniqueStatusAbility(string statusName, StatusAbility statusAbility)
    {
        if (!noCountUniqueStatusAbilityDict.ContainsKey(statusName))
        {
            // �����һ��ʩ�Ӵ�Ч��
            noCountUniqueStatusAbilityDict.Add(statusName, statusAbility);
            statusAbility.statusAbilityManager = this;
            // ����
            statusAbility.ActivateAbility();
            // ���������������ִ��
            statusAbilityList.Add(statusAbility);
        }
        else
        {
            // ����ִ���串�Ƿ���
            statusAbility.OnCover();
        }
    }


    /// <summary>
    /// ֱ�Ӵӱ����Ƴ���������Ψһ��BUFF���Ǵӱ����Ƴ�������ִ�н���������
    /// </summary>
    /// <param name="statusName"></param>
    public void RemoveNoCountUniqueStatusAbility(string statusName)
    {
        if (noCountUniqueStatusAbilityDict.ContainsKey(statusName))
        {
            noCountUniqueStatusAbilityDict.Remove(statusName);
        }
    }

    /// <summary>
    /// �ֶ�������������Ψһ��BUFF����ִ��������ķ�����
    /// </summary>
    public void EndNoCountUniqueStatusAbility(string statusName)
    {
        StatusAbility s = GetNoCountUniqueStatus(statusName);
        if(s!=null)
            s.EndActivate();
    }

    public void TryActivateStatusAbility(int index)
    {
        statusAbilityList[index].TryActivateAbility();
    }

    public void TryActivateStatusAbility(string name)
    {
        foreach (var item in statusAbilityList)
        {
            if (item.name.Equals(name))
            {
                item.TryActivateAbility();
                return;
            }
        }
    }

    public void TryEndStatusAbility(int index)
    {
        statusAbilityList[index].TryEndActivate();
    }

    public void TryEndStatusAbility(string name)
    {
        foreach (var item in statusAbilityList)
        {
            if (item.name.Equals(name))
            {
                item.TryEndActivate();
                return;
            }
        }
    }

    public void TryEndAllStatusAbility()
    {
        foreach (var item in statusAbilityList)
        {
            item.TryEndActivate();
        }
    }

    public void Update()
    {
        foreach (var item in statusAbilityList)
        {
            item.Update();
        }
        foreach (var item in removeList)
        {
            statusAbilityList.Remove(item);
        }
        removeList.Clear();
    }
}
