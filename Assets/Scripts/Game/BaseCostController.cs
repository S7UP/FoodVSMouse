
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class BaseCostController : MonoBehaviour, IBaseCostController, IGameControllerMember
{
    // 引用
    protected GameObject mImg_FireDisplayer;
    protected Text mFireText;
    protected Text mAddFireText;

    // 资源字典
    protected Dictionary<string, float> mCostDict = new Dictionary<string, float>();
    protected Dictionary<string, FloatNumeric> mAddCostDict = new Dictionary<string, FloatNumeric>(); // 资源系统变化字典
    protected Dictionary<string, BoolNumeric> mShieldGettingCostDict = new Dictionary<string, BoolNumeric>(); // 屏蔽获取资源系统字典

    public void Awake()
    {
        mImg_FireDisplayer = transform.Find("Img_FireDisplayer").gameObject;
        mFireText = mImg_FireDisplayer.transform.Find("Tex_Fire").GetComponent<Text>();
        mAddFireText = mImg_FireDisplayer.transform.Find("Tex_AddFire").GetComponent<Text>();
    }

    public void MInit()
    {
        mCostDict.Clear();
        mAddCostDict.Clear();
        mShieldGettingCostDict.Clear();

        AddCostType("Fire", GameController.Instance.mCurrentStage.mStageInfo.startCost);
        // 自然回复
        AddCostResourceModifier("Fire", new FloatModifier((float)25/7/60));

        // 初始化时就要更新一次UI显示
        UpdateCostDisplayer();
        UpdateAddFireDisplayer();
    }

    /// <summary>
    /// 添加一种类型的资源
    /// </summary>
    public void AddCostType(string costTypeName, float initialValue)
    {
        mCostDict.Add(costTypeName, initialValue); // 基础的火苗系统
        mAddCostDict.Add(costTypeName, new FloatNumeric());
        mAddCostDict[costTypeName].Initialize();
        mShieldGettingCostDict.Add(costTypeName, new BoolNumeric());
        mShieldGettingCostDict[costTypeName].Initialize();
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

    public void AddCostResourceModifier(string name, FloatModifier floatModifier)
    {
        mAddCostDict[name].AddAddModifier(floatModifier);
        UpdateAddFireDisplayer();
    }

    public void RemoveCostResourceModifier(string name, FloatModifier floatModifier)
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
        
    }

    public void MResume()
    {
        
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }

    public void MPauseUpdate()
    {
        
    }
}
