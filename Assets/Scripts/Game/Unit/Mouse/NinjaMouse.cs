using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>
/// ����������
/// </summary>
public class NinjaMouse : MouseUnit
{
    private Dictionary<Vector2, BaseUnit> retinueUnitDict; // ��ӱ�
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private DashSkillAbility dashSkillAbility;
    private RectifySkillAbility rectifySkillAbility;
    private SummonRetinueSkillAbility summonRetinueSkillAbility;
    private float tempMoveSpeed; // ��ʱ��Ϊ�˱��ֲ���һ�µ���������

    public override void Awake()
    {
        base.Awake();
        retinueUnitDict = new Dictionary<Vector2, BaseUnit>();
    }

    public override void MInit()
    {
        base.MInit();
        retinueUnitDict.Clear();
        retinueUnitDict.Add(Vector2.left, null);
        retinueUnitDict.Add(Vector2.right, null);
        retinueUnitDict.Add(Vector2.up, null);
        retinueUnitDict.Add(Vector2.down, null);
    }

    /// <summary>
    /// Ĭ���ٻ��¼�
    /// </summary>
    public void DefaultSummonEvent()
    {
        for (int i = 0; i < retinueUnitDict.Count; i++)
        {
            KeyValuePair<Vector2, BaseUnit> item = retinueUnitDict.ElementAt(i);
            Vector2 pos = item.Key;
            BaseUnit unit = item.Value;
            if ((unit == null || !unit.IsAlive()) && !(GetRowIndex()==0 && pos.Equals(Vector2.up)) && !(GetRowIndex() == MapController.yRow-1 && pos.Equals(Vector2.down)))
            {
                Vector2 position = transform.position + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight, 0);
                NinjaRetinueMouse m = (NinjaRetinueMouse)GameController.Instance.CreateMouseUnit(GetRowIndex(), new BaseEnemyGroup.EnemyInfo { type = ((int)MouseNameTypeMap.NinjaRetinueMouse), shape = mShape });
                m.transform.position = position;
                m.UpdateRenderLayer(GameController.Instance.GetSpecificRowEnemyList(m.GetRowIndex()).Count-1); // ����һ��ͼ��
                m.SetActionState(new TransitionState(m));
                retinueUnitDict[pos] = m;
            }
        }
    }

    /// <summary>
    /// �����ٻ��¼�
    /// </summary>
    /// <param name="action"></param>
    public void SetSummonEvent(Action action)
    {
        summonRetinueSkillAbility.SetSummonEvent(action);
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

        // ��漼��
        if (infoList.Count > 1)
        {
            dashSkillAbility = new DashSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(dashSkillAbility);
        }

        // ���ټ���
        if(infoList.Count > 2)
        {
            rectifySkillAbility = new RectifySkillAbility(this, infoList[2]);
            skillAbilityManager.AddSkillAbility(rectifySkillAbility);
        }

        // �ٻ�����
        if(infoList.Count > 3)
        {
            summonRetinueSkillAbility = new SummonRetinueSkillAbility(this, infoList[3]);
            skillAbilityManager.AddSkillAbility(summonRetinueSkillAbility);
            SetSummonEvent(DefaultSummonEvent);
        }
    }

    public override void OnMoveStateEnter()
    {
        if (dashSkillAbility.IsDashed())
            animatorController.Play("Move", true);
        else
            animatorController.Play("Dash", true);
    }

    public override void OnMoveState()
    {
        // ������������������ż��ߵ�ս��������������Ǿ����ܱ��ֲ���һ��
        // ����Ὣ�ƶ��ٶ�ͬ��Ϊ�ƶ��ٶ���������ӣ�ͬʱ����ӹ���ʱ�������ԭ�صȴ�ֱ����Ӿ������ڹ���״̬
        // ���ң�������ʱ����ǿ��ǣ����ӻص����ûص���λ�ã���������ˣ������Ϊ����ͨ��������
        tempMoveSpeed = this.mCurrentMoveSpeed;
        bool flag = false; // �Ƿ�Ҫ��ԭ�صȴ�״̬��flag
        foreach (var item in retinueUnitDict)
        {
            Vector2 pos = item.Key;
            BaseUnit unit = item.Value;
            if (unit != null && unit.IsAlive())
            {
                if (unit.mCurrentMoveSpeed < tempMoveSpeed)
                {
                    tempMoveSpeed = unit.mCurrentMoveSpeed;
                }

                if (tempMoveSpeed <= 0)
                    flag = true;

                if (unit.mCurrentActionState != null && unit.mCurrentActionState is AttackState)
                {
                    flag = true;
                }
            }
        }
        base.OnMoveState();
        // �л�Ϊԭ�صȴ�״̬
        if (flag)
            SetActionState(new IdleState(this));
    }

    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle", true);
    }

    public override void OnIdleState()
    {
        // �ȴ�״̬�£��������д������Ƿ���ս������ս��ص��ƶ�״̬
        bool flag = true;
        foreach (var item in retinueUnitDict)
        {
            Vector2 pos = item.Key;
            BaseUnit unit = item.Value;
            if (unit != null && unit.IsAlive())
            {
                if (unit.mCurrentMoveSpeed <= 0 || (unit as MouseUnit).IsHasTarget())
                {
                    flag = false;
                }
            }
        }
        
        if(flag)
            SetActionState(new MoveState(this));
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;
        if(AnimatorManager.GetNormalizedTime(animator) > 1.0f)
        {
            summonRetinueSkillAbility.EndActivate();
        }
        else if (AnimatorManager.GetNormalizedTime(animator) > 0.6f && mAttackFlag)
        {
            mAttackFlag = false;
            summonRetinueSkillAbility.SetSummonEnable();
        }
    }

    public override void OnCastStateExit()
    {
        mAttackFlag = true;
    }

    /// <summary>
    /// ���·�����
    /// </summary>
    public override void MUpdate()
    {
        base.MUpdate();
        // ����Ƿ���Ҫ�ٻ��µ����
        if (summonRetinueSkillAbility.IsEnergyEnough())
        {
            foreach (var item in retinueUnitDict)
            {
                BaseUnit unit = item.Value;
                Vector2 pos = item.Key;
                if ((unit == null || !unit.IsAlive()) && !(GetRowIndex() == 0 && pos.Equals(Vector2.up)) && !(GetRowIndex() == MapController.yRow - 1 && pos.Equals(Vector2.down)))
                {
                    summonRetinueSkillAbility.OpenSummon();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// ͨ��ǿ����д��ȡ���ٵķ�ʽ��ǿ�Ʊ��ֲ���һ��
    /// </summary>
    public override float GetMoveSpeed()
    {
        return tempMoveSpeed;
    }

    /// <summary>
    /// ������ȡ����
    /// </summary>
    public void PlayRectifyClip()
    {
        animatorController.Play("Rectify");
    }

    /// <summary>
    /// ��ǰ�����Ƿ񲥷���
    /// </summary>
    /// <returns></returns>
    public bool IsCurrentClipEnd()
    {
        return AnimatorManager.GetNormalizedTime(animator) > 1.0f;
    }

    /// <summary>
    /// ��ȡ���
    /// </summary>
    public void PullRetinue()
    {
        foreach (var item in retinueUnitDict)
        {
            Vector3 pos = item.Key;
            BaseUnit unit = item.Value;
            if (unit != null && unit.IsAlive())
            {
                int maxTime = 60;
                int currentTime = 0;
                Vector2 startPosition = unit.transform.position;
                Vector2 endPosition = transform.position + new Vector3(pos.x*MapManager.gridWidth, pos.y*MapManager.gridHeight, 0);
                BaseActionState s = new BaseActionState(unit);
                if(unit is NinjaRetinueMouse)
                {
                    NinjaRetinueMouse n = unit as NinjaRetinueMouse;
                    s.SetEnterAction(delegate { n.PlayRectifyClip(); });
                }
                s.SetUpdateAction(delegate 
                {
                    currentTime++;
                    unit.SetPosition(Vector3.Lerp(startPosition, endPosition, Mathf.Sin((float)currentTime/maxTime*Mathf.PI/2)));
                    if (currentTime >= maxTime)
                        unit.SetActionState(new MoveState(unit));
                });
                unit.SetActionState(s);
            }
        }
    }
}
