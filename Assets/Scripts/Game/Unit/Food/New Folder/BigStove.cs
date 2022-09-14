using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 大火炉
/// </summary>
public class BigStove : FoodUnit
{
    private int timer;
    private FloatModifier floatModifier;
    private int fireCount; // 火苗数

    // 生产类卡片，没有攻击能力，在技能里不填写相关信息即可无法攻击
    public override void MInit()
    {
        floatModifier = null;
        fireCount = 0;
        timer = 0;
        base.MInit();
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        // 如果原来的存在，要先移除原来的
        if (floatModifier != null)
            GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        // 根据星级计算出新的  算法为 火苗数*44/间隔（秒）/60帧
        if (mShape > 0)
            fireCount = 3;
        else
            fireCount = 2;
        floatModifier = new FloatModifier((float)(fireCount*44)/attr.valueList[mLevel]/60);
        // 加回去
        GameController.Instance.AddCostResourceModifier("Fire", floatModifier);
    }

    // 死亡前就清除帧回效果
    public override void AfterDeath()
    {
        base.AfterDeath();
        if (floatModifier != null)
            GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
    }


    public override void MUpdate()
    {
        base.MUpdate();
        // 第60帧时回复火苗数*44火
        if (timer == 60)
        {
            int replyCount = fireCount * 44;
            // 显示回复火苗数特效
            BaseEffect e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Effect/Emp_AddFireEffect").GetComponent<BaseEffect>();
            GameController.Instance.AddEffect(e);
            e.transform.SetParent(GameManager.Instance.GetUICanvas().transform);
            e.transform.localScale = Vector3.one;
            e.transform.position = transform.position;
            e.transform.Find("Img_AddFireEffect").Find("Text").GetComponent<Text>().text = "+" + replyCount;
            // 实际回复
            GameController.Instance.AddFireResource(replyCount);
        }
        timer++;
    }
}
