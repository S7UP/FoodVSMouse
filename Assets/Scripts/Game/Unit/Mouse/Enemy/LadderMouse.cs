using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ������
/// </summary>
public class LadderMouse : MouseUnit
{
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private PutLadderSkillAbility putLadderSkillAbility;
    private FloatModifier velocityBuffModifier; // ����buff
    private BaseGrid targetGrid;
    private float old_P2_HpRate; // �׶ε�2ԭʼѪ������

    public override void MInit()
    {
        base.MInit();
        // ����֮���������
        if (mShape == 1)
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        old_P2_HpRate = (float)mHertRateList[1];
        velocityBuffModifier = new FloatModifier(100);
        NumericBox.MoveSpeed.AddPctAddModifier(velocityBuffModifier); // ��ʼ���100%����

    }

    /// <summary>
    /// Ĭ�Ϸ�����Ʒ�ķ���
    /// </summary>
    public void DefaultPutEvent()
    {
        // �����һ����⣬�����ǰ����һ��û�з����Ϳ�Ƭ����ֱ�����η�����
        bool flag = false;
        List<FoodUnit> list = targetGrid.GetFoodUnitList();
        foreach (var unit in list)
        {
            FoodNameTypeMap f_type = (FoodNameTypeMap)unit.mType;
            if (FoodManager.DenfenceCard.Contains(f_type))
            {
                flag = true;
                break;
            }
        }

        if (flag)
        {
            BaseLadder l = (BaseLadder)GameController.Instance.CreateItem(targetGrid.GetColumnIndex(), targetGrid.GetRowIndex(), (int)ItemNameTypeMap.Ladder, mShape);
            // ������ͼ����ƫ��
            l.AddSpriteOffsetX(new FloatModifier(0.5f * MapManager.gridWidth));
            if(mShape == 0)
            {
                l.SetMoveDistanceAndMaxHight(MapManager.gridWidth * 1.5f, 0.7f*MapManager.gridHeight);
            }
            else
            {
                l.SetMoveDistanceAndMaxHight(MapManager.gridWidth * 3, 1.2f);
            }
            GameManager.Instance.audioSourceManager.PlayEffectMusic("Ladder");
        }
    }

    /// <summary>
    /// ���ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // ��ͨ����
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        }

        // �����Ӽ���
        if (infoList.Count > 1)
        {
            putLadderSkillAbility = new PutLadderSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(putLadderSkillAbility);
            putLadderSkillAbility.SetEvent(DefaultPutEvent);
        }
    }


    public override void OnMoveStateEnter()
    {
        animatorController.Play("Move", true);
    }

    public override void OnAttackStateEnter()
    {
        // ���빥��״̬ʱ���һ�������Ƿ���ˣ�
        // ��û�ţ����⵱ǰ����Ŀ�����ڸ��Ƿ��а��������Ϳ�Ƭ������У�ȡ�����ι�������Ч��ת��Ϊ�����Ӽ��ܣ�����ޣ�ͬ��
        // ���ѷţ������������������
        if (!putLadderSkillAbility.IsSkilled())
        {
            BaseUnit u = GetCurrentTarget();
            if (u!=null)
            {
                BaseGrid g = u.GetGrid();
                if (g != null)
                {
                    bool flag = false;
                    List<FoodUnit> list = g.GetFoodUnitList();
                    foreach (var unit in list)
                    {
                        FoodNameTypeMap f_type = (FoodNameTypeMap)unit.mType;
                        if (FoodManager.DenfenceCard.Contains(f_type))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                    {
                        targetGrid = g;
                        generalAttackSkillAbility.EndActivate(); // ȡ��ƽA
                        putLadderSkillAbility.TriggerSkill(); // �������Ӽ���
                        return; // �ʹ˴�ס
                    }
                }
            }
        }
        base.OnAttackStateEnter();
    }

    // CastStateΪ���ö���
    public override void OnCastStateEnter()
    {
        animatorController.Play("Put");
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            putLadderSkillAbility.TriggerEvent(); // ����ʵ���¼�
            putLadderSkillAbility.SetEndSkill();
        }
    }

    public override void OnCastStateExit()
    {
        // ��Ѫ�����ڽ׶ε�2����ǿ��Ѫ��Ϊ�׶ε�2��Ѫ��
        mCurrentHp = Mathf.Min(mCurrentHp, mMaxHp * old_P2_HpRate-1);
        putLadderSkillAbility.SetSkilled();
        UpdateHertMap();
    }

    /// <summary>
    /// ����ͼ����ʱҪ������
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        // 0 Я���������ɳ��
        // 1 Я�����𵯻ɳ��
        // 2 �޵�������
        // 3 �޵��ɸ���
        if (mHertIndex > 0)
        {
            mHertRateList[0] = double.MaxValue;
        }
        if (mHertIndex > 1)
        {
            mHertRateList[0] = double.MaxValue;
            mHertRateList[1] = double.MaxValue;
            // �����û�з����Ӿͱ������Ѫ�����£���ô����һ����������Ķ���ͬʱ���ö�Ӧ����Ϊ��ʩ�ţ����˺������ظ��ͷţ�
            if (!putLadderSkillAbility.IsSkilled())
            {
                animatorController.Play("Drop");
                putLadderSkillAbility.SetSkilled();
            }
            // �Ƴ�����buff
            NumericBox.MoveSpeed.RemovePctAddModifier(velocityBuffModifier);
        }
    }

}
