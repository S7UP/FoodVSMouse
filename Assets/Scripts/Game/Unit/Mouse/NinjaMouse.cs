using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>
/// 忍者鼠首领
/// </summary>
public class NinjaMouse : MouseUnit
{
    private Dictionary<Vector2, BaseUnit> retinueUnitDict; // 随从表
    private GeneralAttackSkillAbility generalAttackSkillAbility;
    private DashSkillAbility dashSkillAbility;
    private RectifySkillAbility rectifySkillAbility;
    private SummonRetinueSkillAbility summonRetinueSkillAbility;
    private float tempMoveSpeed; // 临时的为了保持步调一致的移速属性

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
    /// 默认召唤事件
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
                m.UpdateRenderLayer(GameController.Instance.GetSpecificRowEnemyList(m.GetRowIndex()).Count-1); // 更新一次图层
                m.SetActionState(new TransitionState(m));
                retinueUnitDict[pos] = m;
            }
        }
    }

    /// <summary>
    /// 设置召唤事件
    /// </summary>
    /// <param name="action"></param>
    public void SetSummonEvent(Action action)
    {
        summonRetinueSkillAbility.SetSummonEvent(action);
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // 普通攻击
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
        }

        // 冲锋技能
        if (infoList.Count > 1)
        {
            dashSkillAbility = new DashSkillAbility(this, infoList[1]);
            skillAbilityManager.AddSkillAbility(dashSkillAbility);
        }

        // 整顿技能
        if(infoList.Count > 2)
        {
            rectifySkillAbility = new RectifySkillAbility(this, infoList[2]);
            skillAbilityManager.AddSkillAbility(rectifySkillAbility);
        }

        // 召唤技能
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
        // 忍者鼠首领与随从有着极高的战术素养，因此它们尽可能保持步调一致
        // 首领会将移动速度同步为移动速度最慢的随从，同时当随从攻击时，首领会原地等待直至随从均不处于攻击状态
        // 并且，首领存活时，会强制牵动随从回到本该回到的位置，当首领噶了，随从行为与普通老鼠无异
        tempMoveSpeed = this.mCurrentMoveSpeed;
        bool flag = false; // 是否要切原地等待状态的flag
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
        // 切换为原地等待状态
        if (flag)
            SetActionState(new IdleState(this));
    }

    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle", true);
    }

    public override void OnIdleState()
    {
        // 等待状态下，会检测所有存活随从是否脱战，均脱战则回到移动状态
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
    /// 更新方法：
    /// </summary>
    public override void MUpdate()
    {
        base.MUpdate();
        // 检测是否需要召唤新的随从
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
    /// 通过强制重写获取移速的方式来强制保持步调一致
    /// </summary>
    public override float GetMoveSpeed()
    {
        return tempMoveSpeed;
    }

    /// <summary>
    /// 播放拉取动画
    /// </summary>
    public void PlayRectifyClip()
    {
        animatorController.Play("Rectify");
    }

    /// <summary>
    /// 当前动画是否播放完
    /// </summary>
    /// <returns></returns>
    public bool IsCurrentClipEnd()
    {
        return AnimatorManager.GetNormalizedTime(animator) > 1.0f;
    }

    /// <summary>
    /// 拉取随从
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
