using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �г��ռ�
/// </summary>
public class RatTrain2 : BaseRatTrain
{
    private CustomizationSkillAbility SummonSoldiers; // �ٻ�ʿ��
    private CustomizationSkillAbility OrbitalMovement; // �����ƶ�
    private CustomizationSkillAbility TimeBomb; // ��ʱը��

    private int leftBombCount; // ʣ��ը����������������ñ�ĳ��᲻�����¹��봫��״̬ʱ���������䲢ת��Ϊ��ʱը����
    private float moveSpeedIncrease; // ��������ֵ
    private FloatModifier moveSpeedModifier; // ����������ǩ
    private FloatModifier BlockedMoveSpeedModifier; // ���׶α��赲ʱ���ٸı��ǩ

    // ����״̬
    private static int[] SpawnWaitTimeArray = new int[] { 300, 240, 180, 120 }; // �¹ֺ�ĵȴ�վ׮ʱ��

    private float oilDistLeft = 0; // �뵹�ͻ�����پ���

    public override void MInit()
    {
        moveSpeedIncrease = 0;
        moveSpeedModifier = null;
        leftBombCount = 0;
        oilDistLeft = 0;
        base.MInit();
        // ����8�ڳ���
        CreateHeadAndBody(8);
        // ���ó�ͷ1���˺����ݣ�����0.5���˺����ݣ���β1����ͨ�˺���10���ҽ��˺�����
        GetHead().SetDmgRate(1.0f, 1.0f);
        foreach (var item in GetBodyList())
        {
            item.SetDmgRate(0.5f, 0.5f);
        }
        GetBody(GetBodyList().Count - 1).SetDmgRate(1.0f, 10f);
        // ��������
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(9f));
        BlockedMoveSpeedModifier = new FloatModifier(100*(TransManager.TranToVelocity(3f)/ NumericBox.MoveSpeed.Value)-100);
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// ���ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        if (mHertIndex < 4)
        {
            // ��֮ǰ
            SummonSoldiersInit(infoList[1]);
            OrbitalMovementInit(infoList[3]);
            List<SkillAbility> list = new List<SkillAbility>();
            list.Add(OrbitalMovement);
            mSkillQueueAbilityManager.ClearAndAddSkillList(list);
        }
        else if (mHertIndex == 4)
        {
            // ��ʼ��
            mSkillQueueAbilityManager.SetCurrentSkill(EnterRampageInit(SkillAbility.GetDefaultSkillInfoInstance("����")));
            mSkillQueueAbilityManager.SetNextSkillIndex(0);
            List<SkillAbility> list = new List<SkillAbility>();
            list.Add(SupersonicRocketHead(SkillAbility.GetDefaultSkillInfoInstance("�����ٻ��ͷ")));
            mSkillQueueAbilityManager.ClearAndAddSkillList(list);
        }
    }

    public override void UpdateRuntimeAnimatorController()
    {
        base.UpdateRuntimeAnimatorController();
        // ������ֵÿ����20%���<׼������>���һ�ڳ���,�����16.7%�ƶ��ٶ�����
        if (mHertIndex >= 1 && mHertIndex < 4)
        {
            if (GetBodyList().Count - leftBombCount>0)
            {
                SetPctAddMoveSpeed(moveSpeedIncrease + 16.7f);
                leftBombCount++;
            }
        }else if(mHertIndex == 4)
        {
            // �ƶ��ٶ�+100%
            SetPctAddMoveSpeed(100f);
            // ʣ�೵��ȫ��ת��Ϊը��
            int c = GetBodyList().Count;
            for (int i = 0; i < c; i++)
            {
                TimeBombAction();
            }
            leftBombCount = 0;
        }
    }

    /// <summary>
    /// �����ƶ��ٶ����ӵİٷֱ�ֵ
    /// </summary>
    private void SetPctAddMoveSpeed(float percent)
    {
        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
        moveSpeedIncrease = percent;
        moveSpeedModifier = new FloatModifier(moveSpeedIncrease);
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
        // �޸Ĵ�Խ�Ĳ����ٶ�
        foreach (var item in GetAllRatTrainComponent())
        {
            item.animatorController.SetSpeed("Appear", 1.0f + (float)moveSpeedIncrease / 100);
            item.animatorController.SetSpeed("Disappear", 1.0f + (float)moveSpeedIncrease / 100);
        }
    }

    /// <summary>
    /// ���ƶ�״̬ʱ�᲻�ϼ��ը�������״̬�������������ĳ�������
    /// </summary>
    public override void OnMoveState()
    {
        base.OnMoveState();
        // ��<׼������>�ĳ���������0�����������ƶ�ʱ���ϼ�����һ�ڳ�����������������������䲢ת��Ϊ��ʱը��
        while(leftBombCount > 0)
        {
            // ֻҪ���һ�ڳ���δ����������ֹͣ����ǰ��ĳ��ᣬ��Ŀ���Ƿ�ֹżȻ�����ǰ��ĳ���������������Ӷ��γɶ���
            RatTrainBody b = GetBody(GetBodyList().Count - 1);
            if(!(b.mCurrentActionState is TransitionState))
            {
                TimeBombAction();
                // ע��ִ������TimeBombAction()�ķ���ʱ��ʹleftBombCount--��
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// ��վ�³����ٹ֣�
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility SummonSoldiersInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        SummonSoldiers = c;
        // ��ر���
        int WaitTime = SpawnWaitTimeArray[mHertIndex];
        int waitTimeLeft = 0; // ʣ��ȴ�ʱ��
        List<RatTrainBody> bodyList = new List<RatTrainBody>();
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            waitTimeLeft = WaitTime;
            SetActionState(new IdleState(this));
            animatorController.Play("PreSpawn2");
            // �����ڳ���ִ���¹�
            bodyList.Clear();
            int start = Mathf.Max(0, GetBodyList().Count - 1);
            int end = Mathf.Max(0, GetBodyList().Count - 5);
            for (int i = start; i >= end; i--)
            {
                if(GetBodyList()[i].mCurrentActionState is MoveState)
                {
                    GetBodyList()[i].animatorController.Play("PreSpawn2");
                    bodyList.Add(GetBodyList()[i]);
                }
            }
        };
        {
            // �¹�ǰҡ
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Spawn2", true);
                    foreach (var item in bodyList)
                    {
                        if (item.IsAlive())
                        {
                            item.animatorController.Play("Spawn2", true);
                            SummonAction(item.transform.position, 2);
                            // �������ͼ��ٴ�
                            for (int i = -1; i <= 1; i+=2)
                            {
                                for (int j = -1; j <= 1; j+=2)
                                {
                                    CreateOil(item.transform.position + MapManager.gridHeight * i * Vector3.up + 0.5f*MapManager.gridWidth * j * Vector3.right);
                                }
                                
                            }
                        }
                    }
                    return true;
                }
                return false;
            });
            // �¹ֵȴ�
            c.AddSpellingFunc(delegate {
                if (waitTimeLeft < 0)
                {
                    animatorController.Play("PostSpawn2");
                    foreach (var item in bodyList)
                    {
                        if (item.IsAlive())
                            item.animatorController.Play("PostSpawn2", true);
                    }
                    return true;
                }
                waitTimeLeft--;
                return false;
            });
            // �¹ֺ�ҡ
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // ֱ����
                    SetActionState(new MoveState(this));
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// �����ƶ�
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility OrbitalMovementInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        OrbitalMovement = c;
        // ��ر���

        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isMoveToDestination = false;
            List<Vector2[]> list = new List<Vector2[]>()
            {
                new Vector2[]{ new Vector2(10, 1f), new Vector2(-1, 1f) },
            };
            AddRouteListByGridIndex(list);
            SetActionState(new MoveState(this));
        };
        {
            // �Ƿ�ִ��յ�
            c.AddSpellingFunc(delegate {
                if (isMoveToDestination)
                {
                    c.ActivateChildAbility(SummonSoldiers);
                    return true;
                }
                return false;
            });
            // �ٻ���ʿ�������ǰ��
            c.AddSpellingFunc(delegate {
                if (!SummonSoldiers.isSpelling)
                {
                    List<Vector2[]> list = new List<Vector2[]>()
                    {
                        new Vector2[]{ new Vector2(-1, 3), new Vector2(10, 3) },
                        new Vector2[]{ new Vector2(10, 5f), new Vector2(-1, 5f) },
                    };
                    isMoveToDestination = false;
                    AddRouteListByGridIndex(list);
                    SetActionState(new MoveState(this));
                    return true;
                }
                return false;
            });
            // �Ƿ�ִ��յ�
            c.AddSpellingFunc(delegate {
                if (isMoveToDestination)
                {
                    isMoveToDestination = false;
                    c.ActivateChildAbility(SummonSoldiers);
                    return true;
                }
                return false;
            });
            // �ٻ���ʿ�������ǰ��
            c.AddSpellingFunc(delegate {
                if (!SummonSoldiers.isSpelling)
                {
                    List<Vector2[]> list = new List<Vector2[]>()
                    {
                        new Vector2[]{ new Vector2(-1, 3), new Vector2(10, 3) },
                    };
                    isMoveToDestination = false;
                    AddRouteListByGridIndex(list);
                    SetActionState(new MoveState(this));
                    return true;
                }
                return false;
            });
            // �Ƿ�ִ��յ㣬�ִ���˳���������һ��ѭ��
            c.AddSpellingFunc(delegate {
                if (isMoveToDestination)
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// ��ʱը������
    /// </summary>
    private void TimeBombAction()
    {
        leftBombCount--;
        int count = GetBodyList().Count;
        if (count > 0)
        {
            RatTrainBody b = GetBody(count - 1);
            CustomizationTask task = new CustomizationTask();
            RemoveTheEndOfBody();
            // �����µĳ�β1���˺���10���ҽ��˺�
            if(GetBodyList().Count >= 1)
                GetBody(GetBodyList().Count - 1).SetDmgRate(1.0f, 10f);
            b.SetMaster(null);
            // ����г����ڴ���״̬����ת��Ϊը��������ֱ����ʧ
            if (!(b.mCurrentActionState is TransitionState))
            {
                task.OnEnterFunc = delegate
                {
                    // �����ĳ�������
                    b.animatorController.Play("Bomb");
                };
                task.AddTaskFunc(delegate
                {
                    if (b.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // �Ա��������ţ�ͬʱ����3*3������ʳ������λ��Ȼ��ȡ�������ж�
                        b.animatorController.Play("BoomDie");
                        b.CloseCollision();
                        // ����һ����ըЧ��
                        BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
                        bombEffect.Init(b, 3600, b.GetRowIndex(), 3, 3, 0, 0, true, true);
                        bombEffect.transform.position = b.transform.position;
                        bombEffect.SetOnEnemyEnterAction((m)=> m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 360, false)));
                        GameController.Instance.AddAreaEffectExecution(bombEffect);
                        return true;
                    }
                    return false;
                });
                // ����������Ϳ�����ʧ��
                task.AddTaskFunc(delegate
                {
                    if (b.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
            }
            else
            {
                task.OnEnterFunc = delegate
                {

                };
                // ����������Ϳ�����ʧ��
                task.AddTaskFunc(delegate
                {
                    if (b.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
            }

            // ����
            task.OnExitFunc = delegate {
                b.SetMaster(null);
                // �Ա������������ֱ�ӻ���
                b.DeathEvent();
            };
            b.AddTask(task);
        }
    }

    /// <summary>
    /// �����״̬
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility EnterRampageInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ��ر���

        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            ClearRouteList(); // ��յ�ǰ·����
            ClearTransferQueue(); // ��յ�ǰת�ƶ���
            isMoveToDestination = false;
            // ԭ����ʧ
            AddRoute(new BaseRatTrain.RoutePoints()
            {
                start = GetHead().transform.position,
                end = GetHead().transform.position + (Vector3)moveRotate*0.01f,
            });
            SetActionState(new MoveState(this));
        };
        {
            // �Ƿ�ִ��յ㣬�ִ���˳���������һ��ѭ��
            c.AddSpellingFunc(delegate {
                if (isMoveToDestination)
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// �����ٻ��ͷ
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility SupersonicRocketHead(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // ��ر���
        bool lastBlock = false;
        // �������
        int cd = 30;
        int cdLeft = cd;
        // �¹ּ��
        int spawnCd = 120;
        int spawnCdLeft = spawnCd;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isMoveToDestination = false;
            List<Vector2[]> list = new List<Vector2[]>()
                    {
                        new Vector2[]{ new Vector2(8, 6), new Vector2(-2, 6) }, // 7·���ҵ���
                        new Vector2[]{ new Vector2(0, 4), new Vector2(10, 4) }, // 5·������
                        new Vector2[]{ new Vector2(8, 2), new Vector2(-2, 2) }, // 3·���ҵ���
                        new Vector2[]{ new Vector2(0, 0), new Vector2(10, 0) }, // 1·������
                        new Vector2[]{ new Vector2(8, 1), new Vector2(-2, 1) }, // 2·���ҵ���
                        new Vector2[]{ new Vector2(0, 3), new Vector2(10, 3) }, // 4·������
                        new Vector2[]{ new Vector2(8, 5), new Vector2(-2, 5) }, // 6·���ҵ���
                        // �������Ǿ����
                        new Vector2[]{ new Vector2(0, 6), new Vector2(10, 6) }, // 7·������
                        new Vector2[]{ new Vector2(8, 4), new Vector2(-2, 4) }, // 5·���ҵ���
                        new Vector2[]{ new Vector2(0, 2), new Vector2(10, 2) }, // 3·������
                        new Vector2[]{ new Vector2(8, 0), new Vector2(-2, 0) }, // 1·���ҵ���
                        new Vector2[]{ new Vector2(0, 1), new Vector2(10, 1) }, // 2·������
                        new Vector2[]{ new Vector2(8, 3), new Vector2(-2, 3) }, // 4·���ҵ���
                        new Vector2[]{ new Vector2(0, 5), new Vector2(10, 5) }, // 6·������
                    };
            AddRouteListByGridIndex(list);
            SetActionState(new MoveState(this));
            GetHead().animatorController.Play("Dash", true); // ��ͷֱ�ӳ��
        };
        {
            // �Ƿ�ִ��յ㣬�ִ���˳���������һ��ѭ��
            c.AddSpellingFunc(delegate {
                // ������赲���赲Ŀ�겻�������ʹ�г����ٽ����� 0.5��/s������ÿ30֡��0.5�룩���赲����� 20���˺�
                cdLeft--;
                bool currentBlock = GetHead().IsHasTarget() && !(GetHead().GetCurrentTarget() is CharacterUnit);
                if (currentBlock)
                {
                    if (cdLeft <= 0)
                    {
                        new DamageAction(CombatAction.ActionType.CauseDamage, this, GetHead().GetCurrentTarget(), 20.0f).ApplyAction();
                        cdLeft = cd;
                    }
                        
                    if (!lastBlock)
                    {
                        // ���ٱ�ǩ���ɱ��赲��
                        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
                        NumericBox.MoveSpeed.RemovePctAddModifier(BlockedMoveSpeedModifier);
                        NumericBox.MoveSpeed.AddPctAddModifier(BlockedMoveSpeedModifier);
                    }
                }
                else
                {
                    if (lastBlock)
                    {
                        // ���ٱ�ǩ����δ�赲��
                        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
                        NumericBox.MoveSpeed.RemovePctAddModifier(BlockedMoveSpeedModifier);
                        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
                    }
                }
                lastBlock = currentBlock;
                
                if(GetHead().mCurrentActionState is MoveState)
                {
                    GetHead().animatorController.Play("Dash", true); // ��ͷֱ�ӳ��
                    // ÿ��һ����ں�����һ������
                    oilDistLeft -= GetMoveSpeed();
                    if(oilDistLeft <= 0)
                    {
                        CreateOil(GetHead().transform.position - (MapManager.gridWidth-oilDistLeft)*(Vector3)GetHead().moveRotate);
                        oilDistLeft += MapManager.gridWidth;
                    }
                    // �¹�
                    spawnCdLeft--;
                    if (spawnCdLeft <= 0)
                    {
                        if (GetHead().transform.position.x > MapManager.GetColumnX(1) && GetHead().transform.position.x < MapManager.GetColumnX(9))
                        {
                            SummonAction(GetHead().transform.position, 1);
                            spawnCdLeft = spawnCd;
                        }
                    }
                }   

                if (isMoveToDestination)
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// �ٻ�ʿ��ʵ��
    /// </summary>
    private void SummonAction(Vector2 center, int count)
    {
        for (int i = -(count-1); i <= count-1; i+=count)
        {
            int j = i;
            MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), 0,
                new BaseEnemyGroup.EnemyInfo() { type = ((int)MouseNameTypeMap.NormalMouse), shape = 8 });
            // ȷ����ʼ�������������
            Vector2 startV2 = new Vector2(center.x, center.y);
            Vector2 endV2 = new Vector2(center.x, center.y + MapManager.gridHeight * j);

            CustomizationTask task = new CustomizationTask();
            int totalTime = 90;
            int currentTime = 0;
            task.OnEnterFunc = delegate
            {
                m.transform.position = startV2; // �Գ�ʼ������н�һ������
                m.CloseCollision(); // �ر��ж�
                m.SetAlpha(0); // 0͸����
            };
            task.AddTaskFunc(delegate
            {
                if(currentTime <= totalTime)
                {
                    currentTime++;
                    float t = (float)currentTime / totalTime;
                    m.SetPosition(Vector2.Lerp(startV2, endV2, t));
                    m.SetAlpha(t);
                    return false;
                }
                return true;
            });
            task.OnExitFunc = delegate
            {
                // �����ж�
                m.OpenCollision();
            };
            m.AddTask(task);
        }
    }

    /// <summary>
    /// ������ʹ����λ���ٵĻ���
    /// </summary>
    private void CreateOil(Vector2 pos)
    {
        // ��������ʵ��
        TimelinessShiftZone t = (TimelinessShiftZone)GameController.Instance.CreateItem(pos, (int)ItemNameTypeMap.ShiftZone, 0);
        t.SetLeftTime(900); // ����15s
        t.SetChangePercent(100.0f); // ���ӵ�ǰ100%��������
        t.SetSkin(1);// ����Ϊ������ۼ��ٴ�
    }

    /// <summary>
    /// �Ƿ��
    /// </summary>
    /// <returns></returns>
    private bool IsRampage()
    {
        return mHertIndex >= 4;
    }
}
