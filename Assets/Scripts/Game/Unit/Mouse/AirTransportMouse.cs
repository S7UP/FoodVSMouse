using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 空中运输型
/// </summary>
public class AirTransportMouse : MouseUnit
{
    private AirTransportSummonSkillAbility airTransportSummonSkillAbility;

    public override void MInit()
    {
        base.MInit();
        // 防爆
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
    }

    /// <summary>
    /// 默认召唤三个投弹
    /// </summary>
    public void DefaultSummonAction()
    {
        int totalTime = 90;
        int currentTime = 0;
        int rowIndex = GetRowIndex();
        int startIndex = Mathf.Max(0, rowIndex-1);
        int endIndex = Mathf.Min(MapController.yRow - 1, rowIndex+1);
        List<MouseUnit> mouseList = new List<MouseUnit>();
        List<Vector3> startV3 = new List<Vector3>();
        List<Vector3> endV3 = new List<Vector3>();
        GameController.Instance.AddTasker(
            // Init
            delegate 
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), i, 
                        new BaseEnemyGroup.EnemyInfo() { type=((int)MouseNameTypeMap.AerialBombardmentMouse), shape = 0});
                    mouseList.Add(m);
                    // 确定初始坐标和最终坐标
                    startV3.Add(new Vector3(transform.position.x + 2*MapManager.gridWidth, transform.position.y, m.transform.position.z));
                    endV3.Add(new Vector3(transform.position.x + 2*MapManager.gridWidth, transform.position.y + MapManager.gridHeight*(rowIndex-i), m.transform.position.z));
                    m.transform.position = startV3[startV3.Count-1]; // 对初始坐标进行进一步修正
                    m.CloseCollision(); // 关闭判定
                    m.SetAlpha(0); // 0透明度
                }
            },
            // Update
            delegate 
            {
                float t = (float)currentTime / totalTime;
                // 这里实现的是渐出并且上下移动的效果
                for (int i = 0; i < mouseList.Count; i++)
                {
                    MouseUnit m = mouseList[i];
                    m.SetPosition(Vector3.Lerp(startV3[i], endV3[i], t));
                    m.SetAlpha(t);
                }
                currentTime++;
            }, 
            // ExitCondition
            delegate 
            { 

                return currentTime > totalTime; 
            },
            // EndEvent
            delegate
            {
                // 标记已完成召唤动作
                if (this.IsAlive())
                {
                    airTransportSummonSkillAbility.SetFinishCast();
                    // 润了润了
                    SetMoveRoate(Vector2.right);
                }
                // 启用判定
                foreach (var m in mouseList)
                {
                    m.OpenCollision();
                    //m.UpdateRenderLayer();
                }
            });
    }

    /// <summary>
    /// 设置召唤事件
    /// </summary>
    /// <param name="action"></param>
    public void SetSummonEvent(Action action)
    {
        airTransportSummonSkillAbility.SetSummonActon(action);
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // 召唤怪技能
        if (infoList.Count > 0)
        {
            airTransportSummonSkillAbility = new AirTransportSummonSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(airTransportSummonSkillAbility);
            SetSummonEvent(DefaultSummonAction); // 设置默认的召唤技能
        }
    }

    public override void OnCastStateEnter()
    {
        int state = airTransportSummonSkillAbility.GetCastState();
        if (state == 0)
        {
            animator.Play("PreCast");
        }else if(state == 1)
        {
            animator.Play("Cast");
        }else if(state == 2)
        {
            animator.Play("PostCast");
        }
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;

        int state = airTransportSummonSkillAbility.GetCastState();
        if (state == 0 || state == 2)
        {
            // 在开舱门或者关舱门阶段时，检测动画播放程度来推进下一阶段
            if(AnimatorManager.GetNormalizedTime(animator) >= 1.0f)
            {
                if (state == 0)
                    airTransportSummonSkillAbility.SetFinishPreCast();
                else
                    airTransportSummonSkillAbility.SetFinishPostCast();
            }
        }
    }
}
