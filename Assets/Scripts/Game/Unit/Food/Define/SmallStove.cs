using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 小火炉
/// </summary>
public class SmallStove : FoodUnit
{
    private int timer;
    private FloatModifier floatModifier = new FloatModifier(0);
    private int fireCount; // 火苗数
    private float lastAttack; // 上一帧生产效率

    // 生产类卡片，没有攻击能力，在技能里不填写相关信息即可无法攻击
    public override void MInit()
    {
        fireCount = 0;
        timer = 0;
        lastAttack = 0;
        base.MInit(); 
        // 对于生产卡而言，攻击力即生产效率，1.0代表100%的生产效率
        NumericBox.Attack.SetBase(1.0f);
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        // 如果原来的存在，要先移除原来的
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        if (mShape > 1)
            fireCount = 2;
        else
            fireCount = 1;
        // 根据星级计算出新的  算法为 生产效率（攻击力）*火苗数*44/间隔（秒）/60帧
        floatModifier.Value = mCurrentAttack * (float)(fireCount * 44) / attr.valueList[mLevel] / 60;
        // 加回去
        GameController.Instance.AddCostResourceModifier("Fire", floatModifier);
        lastAttack = mCurrentAttack;
    }

    

    // 死亡前就清除帧回效果
    public override void BeforeDeath()
    {
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        base.BeforeDeath();
    }

    /// <summary>
    /// 再移除一次，以防止强制移除效果不触发产能清空判定bug
    /// </summary>
    public override void AfterDeath()
    {
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
    }

    public override void MUpdate()
    {
        if (mCurrentAttack != lastAttack)
        {
            UpdateAttributeByLevel();
        }
        base.MUpdate();
    }

    public override void OnIdleState()
    {
        // 第60帧时回复火苗数*44火
        if (timer == 60)
        {
            float replyCount = mCurrentAttack * fireCount * 44;
            CreateAddFireEffect(transform.position, replyCount);
        }
        timer++;
        base.OnIdleState();
    }

    /// <summary>
    /// 创建一个增加火苗的特效
    /// </summary>
    public static void CreateAddFireEffect(Vector2 position, float replyCount)
    {
        // 显示回复火苗数特效
        BaseEffect e = BaseEffect.GetInstance("Emp_AddFireEffect");
        GameController.Instance.AddEffect(e);
        e.transform.SetParent(GameManager.Instance.GetUICanvas().transform);
        e.transform.localScale = Vector3.one;
        e.transform.position = position;
        e.transform.Find("Img_AddFireEffect").Find("Text").GetComponent<Text>().text = "+" + (int)replyCount;
        // 实际回复
        GameController.Instance.AddFireResource(replyCount);
    }
}
