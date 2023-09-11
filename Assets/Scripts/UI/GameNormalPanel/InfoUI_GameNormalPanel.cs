using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 信息显示面板
/// </summary>
public class InfoUI_GameNormalPanel : MonoBehaviour
{
    private Image Img_Head;
    private Text Tex_Name;
    private EventTrigger EventTrigger_Name;
    private Text Tex_Attack;
    private EventTrigger EventTrigger_Attack;
    private Text Tex_AttackSpeed;
    private EventTrigger EventTrigger_AttackSpeed;
    private Text Tex_Defence;
    private EventTrigger EventTrigger_Defence;
    private Text Tex_MoveSpeed;
    private EventTrigger EventTrigger_MoveSpeed;
    private Transform Trans_HpBar1;
    private Text Tex_Hp;

    private BaseUnit targetUnit;

    public void Awake()
    {
        Img_Head = transform.Find("Head").Find("Image").GetComponent<Image>();
        Tex_Name = transform.Find("Tex_Name").GetComponent<Text>();
        Tex_Attack = transform.Find("Img_Attack").Find("Text").GetComponent<Text>();
        EventTrigger_Attack = transform.Find("Img_Attack").GetComponent<EventTrigger>();
        Tex_AttackSpeed = transform.Find("Img_AttackSpeed").Find("Text").GetComponent<Text>();
        EventTrigger_AttackSpeed = transform.Find("Img_AttackSpeed").GetComponent<EventTrigger>();
        Tex_Defence = transform.Find("Img_Defence").Find("Text").GetComponent<Text>();
        EventTrigger_Defence = transform.Find("Img_Defence").GetComponent<EventTrigger>();
        Tex_MoveSpeed = transform.Find("Img_MoveSpeed").Find("Text").GetComponent<Text>();
        EventTrigger_MoveSpeed = transform.Find("Img_MoveSpeed").GetComponent<EventTrigger>();
        Trans_HpBar1 = transform.Find("Img_HpBar0").Find("Img_HpBar1");
        Tex_Hp = transform.Find("Img_HpBar0").Find("Text").GetComponent<Text>();
        AddEventTriggerOnAwake();
    }

    private void AddEventTriggerOnAwake()
    {
        // 攻击力图标
        {
            EventTrigger.TriggerEvent tr = new EventTrigger.TriggerEvent();
            tr.AddListener(delegate { OnPointerEnterIcon(EventTrigger_Attack.GetComponent<RectTransform>(), "攻击力：目标普通攻击的伤害值，可以影响目标部分技能的数值。"); });
            EventTrigger_Attack.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = tr });
        }
        
        // 攻击速度图标
        {
            EventTrigger.TriggerEvent tr = new EventTrigger.TriggerEvent();
            tr.AddListener(delegate { OnPointerEnterIcon(EventTrigger_AttackSpeed.GetComponent<RectTransform>(), "攻击速度：目标每秒可以攻击的次数。|| 技能速率：目标技能填充速率，值越大技能间隔越小。"); });
            EventTrigger_AttackSpeed.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = tr });
        }

        // 伤害减免图标
        {
            EventTrigger.TriggerEvent tr = new EventTrigger.TriggerEvent();
            tr.AddListener(delegate { OnPointerEnterIcon(EventTrigger_Defence.GetComponent<RectTransform>(), "伤害减免率：目标在受到普通伤害时可以减少的伤害比率，若为负数则为增加的伤害比率。"); });
            EventTrigger_Defence.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = tr });
        }

        // 移动速度图标
        {
            EventTrigger.TriggerEvent tr = new EventTrigger.TriggerEvent();
            tr.AddListener(delegate { OnPointerEnterIcon(EventTrigger_MoveSpeed.GetComponent<RectTransform>(), "移动速度：目标的移动速度标准值，每1点移动速度代表每6秒走完一格的速度。"); });
            EventTrigger_MoveSpeed.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = tr });
        }
    }

    /// <summary>
    /// 由GameNormalPanel调用
    /// </summary>
    public void MInit()
    {
        targetUnit = null;
    }

    /// <summary>
    /// 由GameNormalPanel调用
    /// </summary>
    public void MUpdate()
    {
        if(targetUnit != null && targetUnit.IsAlive())
        {
            UpdateHpDisplay();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 更新血量信息
    /// </summary>
    public void UpdateHpDisplay()
    {
        if (targetUnit != null)
        {
            Tex_Hp.text = (int)targetUnit.mCurrentHp + "/" + (int)targetUnit.mMaxHp;
            Trans_HpBar1.transform.localScale = new Vector2(Mathf.Min(1, Mathf.Max(0, targetUnit.GetHeathPercent())), 1);
            Tex_Attack.text = ((int)targetUnit.mCurrentAttack).ToString();
            Tex_AttackSpeed.text = targetUnit.mCurrentAttackSpeed.ToString("#0.00")+" | "+targetUnit.mCurrentSkillSpeed.ToString("#0.00");
            float dmgRate = targetUnit.GetFinalDamageRate();
            if (dmgRate <= 1)
                dmgRate = (1 - dmgRate) * 100;
            else
                dmgRate = -(dmgRate - 1) * 100;
            Tex_Defence.text = ((int)dmgRate).ToString() + "%";
            if (targetUnit is MouseUnit)
                Tex_MoveSpeed.text = (TransManager.TranToStandardVelocity(targetUnit.mCurrentMoveSpeed)).ToString("#0.00");
            else
                Tex_MoveSpeed.text = "--";
        }
    }

    /// <summary>
    /// 显示美食信息
    /// </summary>
    /// <param name="unit"></param>
    public void DisplayFoodInfo(BaseUnit unit)
    {
        if(unit.mType >= 0)
        {
            gameObject.SetActive(true);
            targetUnit = unit;
            FoodUnit.Attribute attr = GameManager.Instance.attributeManager.GetFoodUnitAttribute(unit.mType, unit.mShape);
            Img_Head.sprite = GameManager.Instance.GetSprite("Food/"+unit.mType + "/" + unit.mShape +"/display");
            Tex_Name.text = attr.baseAttrbute.name;
            UpdateHpDisplay();
        }
    }

    /// <summary>
    /// 显示老鼠信息
    /// </summary>
    /// <param name="unit"></param>
    public void DisplayMouseInfo(BaseUnit unit)
    {

        if(unit is BossUnit)
        {
            if (unit.mType >= 0)
            {
                gameObject.SetActive(true);
                targetUnit = unit;
                Img_Head.sprite = GameManager.Instance.GetSprite("Boss/" + unit.mType + "/" + unit.mShape + "/icon");
                Tex_Name.text = BossManager.GetBossName(unit.mType, unit.mShape);
                UpdateHpDisplay();
            }
        }
        else
        {
            if (unit.mType >= 0)
            {
                gameObject.SetActive(true);
                targetUnit = unit;
                MouseManager.MouseAttribute attr = MouseManager.GetAttribute(unit.mType, unit.mShape);
                Img_Head.sprite = GameManager.Instance.GetSprite("Mouse/" + unit.mType + "/" + unit.mShape + "/icon");
                Tex_Name.text = attr.name;
                UpdateHpDisplay();
            }
        }
    }

    /// <summary>
    /// 显示人物信息
    /// </summary>
    /// <param name="unit"></param>
    public void DisplayCharacterInfo(BaseUnit unit)
    {
        if (unit.mType >= 0)
        {
            gameObject.SetActive(true);
            targetUnit = unit;
            Img_Head.sprite = GameManager.Instance.GetSprite("Character/" + unit.mType + "/icon");
            Tex_Name.text = "玩家";
            UpdateHpDisplay();
        }
    }

    /// <summary>
    /// 当自身界面图标被鼠标滑动经过时，显示文字信息
    /// </summary>
    public void OnPointerEnterIcon(RectTransform rect, string text)
    {
        TextArea.Instance.SetText(text);
        TextArea.Instance.SetLocalPosition(rect.transform, new Vector2(rect.sizeDelta.x/2, 0), new Vector2(1, -1));
    }

    /// <summary>
    /// 当自身界面图标被鼠标滑动移出时，取消文字信息（由外部添加）
    /// </summary>
    public void OnPointerExitIcon()
    {
        TextArea.ExecuteRecycle();
    }
}
