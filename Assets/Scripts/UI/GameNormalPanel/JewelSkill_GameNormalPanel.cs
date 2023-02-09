using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 宝石技能窗口
/// </summary>
public class JewelSkill_GameNormalPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform RectTrans;
    private Image Img_Jewel;
    private Button Btn;
    private GameObject UnableMask; // 不可使用时的遮罩
    private Transform Trans_CDMask; // 冷却时间遮罩
    private Text Tex_CDLeft; // 冷却时间

    private int type = -1; // 对应的宝石种类
    private BaseJewelSkill skill;

    public void Awake()
    {
        RectTrans = GetComponent<RectTransform>();
        Img_Jewel = transform.GetComponent<Image>();
        Btn = transform.GetComponent<Button>();
        UnableMask = transform.Find("UnableMask").gameObject;
        Trans_CDMask = transform.Find("CDMask").transform;
        Tex_CDLeft = transform.Find("Tex_CDLeft").GetComponent<Text>();
    }

    public void MInit(int type, BaseJewelSkill skill)
    {
        this.type = type;
        this.skill = skill;
        Btn.onClick.RemoveAllListeners();
        if(type == -1)
        {
            Img_Jewel.sprite = GameManager.Instance.GetSprite("UI/GameNormalPanel/JewelSkillUI/lockedSkill");
            Btn.interactable = false;
            UnableMask.gameObject.SetActive(true);
            Trans_CDMask.gameObject.SetActive(false);
            Tex_CDLeft.gameObject.SetActive(false);
        }
        else
        {
            Btn.onClick.AddListener(OnClick);
            Img_Jewel.sprite = GameManager.Instance.GetSprite("Jewel/" + type + "/SkillIcon");
            Btn.interactable = true;
            UnableMask.gameObject.SetActive(true);
            Trans_CDMask.gameObject.SetActive(true);
            Tex_CDLeft.gameObject.SetActive(true);
        }
    }

    public void MUpdate()
    {
        if(type > -1)
        {
            if (skill.IsEnoughEnergy())
            {
                UnableMask.gameObject.SetActive(false);
                Trans_CDMask.gameObject.SetActive(false);
                Tex_CDLeft.gameObject.SetActive(false);
            }
            else
            {
                UnableMask.gameObject.SetActive(true);
                Trans_CDMask.gameObject.SetActive(true);
                Trans_CDMask.localScale = new Vector2(1, 1-skill.currentEnergy/skill.maxEnergy);
                Tex_CDLeft.gameObject.SetActive(true);
                Tex_CDLeft.text = (skill.maxEnergy - skill.currentEnergy).ToString("#0.0");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (type != -1)
        {
            TextArea.Instance.SetText(JewelManager.GetName(type) + "\n" + JewelManager.GetInfo(type));
            TextArea.Instance.SetLocalPosition(transform, new Vector2(RectTrans.sizeDelta.x / 2, 0), new Vector2(1, -1));
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (type != -1)
            TextArea.ExecuteRecycle();
    }

    private void OnClick()
    {
        skill.TryExecute();
    }
}
