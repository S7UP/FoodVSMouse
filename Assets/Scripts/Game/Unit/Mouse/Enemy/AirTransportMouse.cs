using S7P.Numeric;

using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ����������
/// </summary>
public class AirTransportMouse : MouseUnit, IFlyUnit
{
    private AirTransportSummonSkillAbility airTransportSummonSkillAbility;

    public override void MInit()
    {
        base.MInit();
        mHeight = 1;
        // ͼ��Ȩ��-1
        typeAndShapeValue = -1;
        // ����������ֵ����33%ʱ������
        {
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                return GetHeathPercent() < 0.33f;
            });
            task.AddOnExitAction(delegate {
                // ��������
                SetMoveRoate(Vector2.right);
            });
            taskController.AddTask(task);
        }
    }

    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(2.04f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(4.58f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    private CustomizationTask GetSummonsTask(MouseUnit m, Vector2 start, Vector2 end, int totalTime)
    {
        Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false;  };

        CustomizationTask task = new CustomizationTask();
        task.AddTimeTaskFunc(totalTime, delegate
        {
            // ���ɱ����С����ɱ�ѡΪ����Ŀ�꣬�����赲
            m.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
            m.AddCanHitFunc(noHitFunc);
            m.AddCanBlockFunc(noBeSelectedAsTargetFunc);
            m.transform.position = start;
            m.SetAlpha(0);
            m.DisableMove(true);
        },
        (leftTime, totalTime) =>
        {
            float rate = 1 - (float)leftTime / totalTime;
            m.SetAlpha(rate);
            m.transform.position = Vector2.Lerp(start, end, rate);
        },
        delegate
        {
            // ���ɱ����С����ɱ�ѡΪ����Ŀ�꣬�����赲
            m.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
            m.RemoveCanHitFunc(noHitFunc);
            m.RemoveCanBlockFunc(noBeSelectedAsTargetFunc);
            m.SetAlpha(1);
            m.transform.position = end;
            m.DisableMove(false);
        });
        return task;
    }

    /// <summary>
    /// Ĭ���ٻ�����Ͷ��
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
        // �ٻ��ֲ�Ϊ���Ƿ�������
        for (int i = startIndex; i <= endIndex; i++)
        {
            MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), i,
                new BaseEnemyGroup.EnemyInfo() { type = ((int)MouseNameTypeMap.AerialBombardmentMouse), shape = 0 });
            CustomizationTask task = GetSummonsTask(m, new Vector2(transform.position.x + 2 * MapManager.gridWidth, transform.position.y), new Vector2(transform.position.x + 2 * MapManager.gridWidth, transform.position.y + MapManager.gridHeight * (rowIndex - i)), totalTime);
            m.taskController.AddTask(task);
        }
        // ����رղ���
        {
            CustomizationTask task = new CustomizationTask();
            task.AddTimeTaskFunc(totalTime);
            task.AddOnExitAction(delegate {
                airTransportSummonSkillAbility.SetFinishCast();
            });
            taskController.AddTask(task);
        }

        //GameController.Instance.AddTasker(
        //    // Init
        //    delegate 
        //    {
        //        for (int i = startIndex; i <= endIndex; i++)
        //        {
        //            MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), i, 
        //                new BaseEnemyGroup.EnemyInfo() { type=((int)MouseNameTypeMap.AerialBombardmentMouse), shape = 0});
        //            mouseList.Add(m);
        //            // ȷ����ʼ�������������
        //            startV3.Add(new Vector3(transform.position.x + 2*MapManager.gridWidth, transform.position.y, m.transform.position.z));
        //            endV3.Add(new Vector3(transform.position.x + 2*MapManager.gridWidth, transform.position.y + MapManager.gridHeight*(rowIndex-i), m.transform.position.z));
        //            m.transform.position = startV3[startV3.Count-1]; // �Գ�ʼ������н�һ������
        //            m.CloseCollision(); // �ر��ж�
        //            m.SetAlpha(0); // 0͸����
        //        }
        //    },
        //    // Update
        //    delegate 
        //    {
        //        float t = (float)currentTime / totalTime;
        //        // ����ʵ�ֵ��ǽ������������ƶ���Ч��
        //        for (int i = 0; i < mouseList.Count; i++)
        //        {
        //            MouseUnit m = mouseList[i];
        //            m.SetPosition(Vector3.Lerp(startV3[i], endV3[i], t));
        //            m.SetAlpha(t);
        //        }
        //        currentTime++;
        //    }, 
        //    // ExitCondition
        //    delegate 
        //    { 

        //        return currentTime > totalTime; 
        //    },
        //    // EndEvent
        //    delegate
        //    {
        //        // ���������ٻ�����
        //        if (this.IsAlive())
        //        {
        //            airTransportSummonSkillAbility.SetFinishCast();
        //            // ��������
        //            SetMoveRoate(Vector2.right);
        //        }
        //        // �����ж�
        //        foreach (var m in mouseList)
        //        {
        //            m.OpenCollision();
        //            //m.UpdateRenderLayer();
        //        }
        //    });
    }

    /// <summary>
    /// �����ٻ��¼�
    /// </summary>
    /// <param name="action"></param>
    public void SetSummonEvent(Action action)
    {
        airTransportSummonSkillAbility.SetSummonActon(action);
    }

    /// <summary>
    /// ���ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // �ٻ��ּ���
        if (infoList.Count > 0)
        {
            airTransportSummonSkillAbility = new AirTransportSummonSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(airTransportSummonSkillAbility);
            SetSummonEvent(DefaultSummonAction); // ����Ĭ�ϵ��ٻ�����

            // �뼼�����ʹҹ�
            {
                FloatModifier skillSpeedMod = new FloatModifier((NumericBox.SkillSpeed.TotalValue - 1) * 100);
                airTransportSummonSkillAbility.energyRegeneration.AddPctAddModifier(skillSpeedMod);

                NumericBox.SkillSpeed.AddAfterValueChangeAction((val) => {
                    airTransportSummonSkillAbility.energyRegeneration.RemovePctAddModifier(skillSpeedMod);
                    skillSpeedMod.Value = (NumericBox.SkillSpeed.TotalValue - 1) * 100;
                    airTransportSummonSkillAbility.energyRegeneration.AddPctAddModifier(skillSpeedMod);
                });
            }
        }
    }

    public override void OnCastStateEnter()
    {
        int state = airTransportSummonSkillAbility.GetCastState();
        if (state == 0)
        {
            animatorController.Play("PreCast");
        }else if(state == 1)
        {
            animatorController.Play("Cast", true);
        }else if(state == 2)
        {
            animatorController.Play("PostCast");
        }
    }

    public override void OnCastState()
    {
        if (currentStateTimer == 0)
            return;

        int state = airTransportSummonSkillAbility.GetCastState();
        if (state == 0 || state == 2)
        {
            // �ڿ����Ż��߹ز��Ž׶�ʱ����⶯�����ų̶����ƽ���һ�׶�
            if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                if (state == 0)
                    airTransportSummonSkillAbility.SetFinishPreCast();
                else
                    airTransportSummonSkillAbility.SetFinishPostCast();
            }
        }
    }

    public void ExecuteDrop()
    {
        // Ȼ����ĸ���ܻ�׹2333
    }
}
