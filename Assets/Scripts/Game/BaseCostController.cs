using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCostController : MonoBehaviour, IBaseCostController, IGameControllerMember
{
    protected Dictionary<string, float> mCostDict;
    protected Dictionary<string, FloatNumeric> mAddCostDict; // 资源系统变化字典
    protected Dictionary<string, BoolNumeric> mShieldGettingCostDict; // 屏蔽获取资源系统字典
    protected GameObject mImg_FireDisplayer;
    protected Text mFireText;
    protected Text mAddFireText;

    public void Awake()
    {

    }

    public void MInit()
    {
        mCostDict = new Dictionary<string, float>();
        mCostDict.Add("Fire", 3000.0f); // 基础的火苗系统
        mAddCostDict = new Dictionary<string, FloatNumeric>();
        mAddCostDict.Add("Fire", new FloatNumeric());
        mAddCostDict["Fire"].Initialize();
        mShieldGettingCostDict = new Dictionary<string, BoolNumeric>();
        mShieldGettingCostDict.Add("Fire", new BoolNumeric());
        mShieldGettingCostDict["Fire"].Initialize();

        mImg_FireDisplayer = transform.Find("Img_FireDisplayer").gameObject;
        mFireText = mImg_FireDisplayer.transform.Find("Tex_Fire").GetComponent<Text>();
        mAddFireText = mImg_FireDisplayer.transform.Find("Tex_AddFire").GetComponent<Text>();

        // 初始化时就要更新一次UI显示
        UpdateCostDisplayer();
        UpdateAddFireDisplayer();
    }

    /// <summary>
    /// 更新资源系统UI显示
    /// </summary>
    protected virtual void UpdateCostDisplayer()
    {
        mFireText.text = Mathf.Floor(GetCost("Fire")).ToString();
    }

    public void UpdateAddFireDisplayer()
    {
        if (IsShieldAddResource("Fire"))
        {
            mAddFireText.text = "Disable!";
        }
        else
        {
            mAddFireText.text = "+" + (mAddCostDict["Fire"].Value * 60).ToString("F2") + "/s";
        }
        
    }

    public void AddCost(string name, float val)
    {
        if (mCostDict.ContainsKey(name))
        {
            if (val > 0 && IsShieldAddResource(name))
                return;
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

    public void AddModifier(string name, FloatModifier floatModifier)
    {
        mAddCostDict[name].AddAddModifier(floatModifier);
        UpdateAddFireDisplayer();
    }

    public void RemoveModifier(string name, FloatModifier floatModifier)
    {
        mAddCostDict[name].RemoveAddModifier(floatModifier);
        UpdateAddFireDisplayer();
    }

    /// <summary>
    /// 获取某个资源是否被屏蔽增长了（包括自增长与其他外界的方式增加）
    /// </summary>
    public bool IsShieldAddResource(string name)
    {
        return mShieldGettingCostDict[name].Value;
    }

    /// <summary>
    /// 添加屏蔽某个资源的修饰器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="boolModifier"></param>
    public void AddShieldModifier(string name, BoolModifier boolModifier)
    {
        mShieldGettingCostDict[name].AddDecideModifier(boolModifier);
        UpdateAddFireDisplayer();
    }

    /// <summary>
    /// 移除屏蔽某个资源的修饰器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="boolModifier"></param>
    public void RemoveShieldModifier(string name, BoolModifier boolModifier)
    {
        mShieldGettingCostDict[name].RemoveDecideModifier(boolModifier);
        UpdateAddFireDisplayer();
    }

    public void MUpdate()
    {
        foreach (var item in mAddCostDict)
        {
            string key = item.Key;
            FloatNumeric numeric = item.Value;
            AddCost(key, numeric.Value);
        }
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
