using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCostController : MonoBehaviour, IBaseCostController, IGameControllerMember
{
    protected Dictionary<string, float> mCostDict;
    protected GameObject mImg_FireDisplayer;
    protected Text mFireText;

    public void Awake()
    {

    }

    public void MInit()
    {
        mCostDict = new Dictionary<string, float>();
        mCostDict.Add("Fire", 50.0f); // 基础的火苗系统
        mImg_FireDisplayer = transform.Find("Img_FireDisplayer").gameObject;
        mFireText = mImg_FireDisplayer.transform.Find("Tex_Fire").GetComponent<Text>();

        // 初始化时就要更新一次UI显示
        UpdateCostDisplayer();
    }

    /// <summary>
    /// 更新资源系统UI显示
    /// </summary>
    protected virtual void UpdateCostDisplayer()
    {
        mFireText.text = Mathf.Floor(GetCost("Fire")).ToString();
    }

    public void AddCost(string name, float val)
    {
        if (mCostDict.ContainsKey(name))
        {
            mCostDict[name] += val;
            UpdateCostDisplayer();
        }
        else
        {
            Debug.LogError("键值为 "+name+" 的资源无法被找到！");
        }
    }

    public float GetCost(string name)
    {
        return mCostDict[name];
    }

    public void SetCost(string name, float val)
    {
        if (mCostDict.ContainsKey(name))
        {
            mCostDict[name] = val;
            UpdateCostDisplayer();
        }
        else
        {
            Debug.LogError("键值为 " + name + " 的资源无法被找到！");
        }
    }

    public void MUpdate()
    {
        AddCost("Fire", 1/5f);
    }

    public void MPause()
    {
        throw new System.NotImplementedException();
    }

    public void MResume()
    {
        throw new System.NotImplementedException();
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }
}
