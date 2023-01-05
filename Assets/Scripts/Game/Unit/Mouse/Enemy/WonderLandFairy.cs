using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �澳����
/// </summary>
public class WonderLandFairy : MouseUnit
{
    private List<BaseUnit> LuminescentMoldList = new List<BaseUnit>(); // ����ײ�ķ���ù����
    private bool isConceal; // �Ƿ�������״̬
    private static FloatModifier speedModifier = new FloatModifier(300);

    public override void MInit()
    {
        LuminescentMoldList.Clear();
        base.MInit();
        isConceal = true;
        EnterFogMode();
    }

    /// <summary>
    /// ������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return false;
    }

    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return !isConceal && base.CanBeSelectedAsTarget(otherUnit);
    }

    public override bool CanHit(BaseBullet bullet)
    {
        return !isConceal && base.CanHit(bullet);
    }

    /// <summary>
    /// �����ѷ���λ����ʳ���������������ײ�ж�ʱ
    /// </summary>
    public override void OnAllyCollision(BaseUnit unit)
    {
        // ���Ŀ���Ƿ���ù�����߱���Ⱦ��Ƭ
        if (!LuminescentMoldList.Contains(unit)
            && (unit.mType == (int)FoodNameTypeMap.LuminescentMold || unit.NumericBox.GetBoolNumericValue(StringManager.BacterialInfection)))
        {
            LuminescentMoldList.Add(unit);
        }
    }

    /// <summary>
    /// ���ѷ���λ�뿪ʱ
    /// </summary>
    /// <param name="collision"></param>
    public override void OnAllyTriggerExit(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            if (LuminescentMoldList.Contains(unit))
            {
                LuminescentMoldList.Remove(unit);
            }
        }
        base.OnAllyTriggerExit(collision);
    }

    public override void MUpdate()
    {
        foreach (var unit in LuminescentMoldList)
        {
            if (unit.mType != (int)FoodNameTypeMap.LuminescentMold && !unit.NumericBox.GetBoolNumericValue(StringManager.BacterialInfection))
                LuminescentMoldList.Remove(unit);
        }
        if (LuminescentMoldList.Count > 0)
        {
            if (isConceal)
            {
                ExitFogMode();
                isConceal = false;
            }
        }
        else
        {
            if (!isConceal)
            {
                isConceal = true;
                EnterFogMode();
            }
        }
        base.MUpdate();
    }

    private void EnterFogMode()
    {
        if (!IsContainEffect(EffectType.Conceal))
        {
            BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Mouse/35/Fog"), "Appear", "Idle", "Disappear", true);
            e.SetSpriteRendererSorting("Effect", 1);
            GameController.Instance.AddEffect(e);
            AddEffectToDict(EffectType.Conceal, e, new Vector2(0, 0.5f*MapManager.gridWidth));
        }
        NumericBox.MoveSpeed.RemovePctAddModifier(speedModifier);
        NumericBox.MoveSpeed.AddPctAddModifier(speedModifier);
    }

    private void ExitFogMode()
    {
        if (IsContainEffect(EffectType.Conceal))
        {
            RemoveEffectFromDict(EffectType.Conceal);
        }
        NumericBox.MoveSpeed.RemovePctAddModifier(speedModifier);
    }
}
