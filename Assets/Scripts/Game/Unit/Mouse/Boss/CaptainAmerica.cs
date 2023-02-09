using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ������
/// </summary>
public class CaptainAmerica : BossUnit
{
    // ������
    private CustomizationSkillAbility TeleportSkill; // ˲���ƶ�
    private CustomizationSkillAbility BoomerangShield; // ����Բ��
    private CustomizationSkillAbility ShieldStrike; // Բ���Ļ�
    private CustomizationSkillAbility SummonSoldiers; // �ٻ�ʿ��

    // �����ӵ�
    private EnemyBullet ShieldBullet;
    private static RuntimeAnimatorController ShieldBulletAnimaotrController;

    // ����״̬
    private bool isHasShield; // �Ƿ���ж���
    private static int[] DropWaitTimeArray = new int[] { 300, 180, 60 }; // ������ǰ������ʱ�䣨֡��
    private int DropWaitTimeLeft = 0; // ����ʣ��ʱ��
    private static int[] ShieldMovementTimeArray = new int[] { 480, 360, 240 }; // �����˶�ʱ��
    private int ShieldMovementTimeLeft = 0; // ����ʣ���˶�ʱ��
    private static int[] AfterBoomerangShieldWaitTimeArray = new int[] { 240, 120, 0 }; // �ӵ����ƺ�վ׮ͣ��ʱ��
    private bool isPreDrop; // �Ƿ�������������ʱ��

    private List<Vector2> pathList = new List<Vector2>() // ���Ƶ��˶��켣��
    {
        new Vector2(MapManager.GetColumnX(7), MapManager.GetRowY(6)),
        new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(6)),
        new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(0)),
        new Vector2(MapManager.GetColumnX(7), MapManager.GetRowY(0)),
    }; 

    private static int[] StrikeCountArray = new int[] { 2, 3, 4 }; // Բ���Ļ�����
    private static int[] StrikeWaitTimeArray = new int[] { 180, 90, 0 }; // Բ���Ļ�������ͣ��ʱ��

    private static int[] StandTimeArray = new int[] { 360, 240, 120 }; // �����վ��ʱ��
    private int StandTimeLeft = 0; // ����վ��ʣ��ʱ��

    public override void Awake()
    {
        if (ShieldBulletAnimaotrController == null)
            ShieldBulletAnimaotrController = GameManager.Instance.GetRuntimeAnimatorController("Boss/20/ShieldBullet");
        base.Awake();
        ShieldBullet = transform.Find("ShieldBullet").GetComponent<EnemyBullet>();
    }

    public override void MInit()
    {
        isHasShield = true;
        isPreDrop = false;
        DropWaitTimeLeft = 0;
        ShieldMovementTimeLeft = 0;
        ShieldBullet.gameObject.SetActive(false);
        ShieldBullet.isnKillSelf = true;
        ShieldBullet.isAffectCharacter = false; // ��Ӱ����
        base.MInit();
    }

    /// <summary>
    /// ���ؼ���
    /// </summary>
    public override void LoadSkillAbility()
    {
        //skillAbilityManager.Initialize();
        TeleportInit();
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();

        
        list.Add(ShieldStrikeInit(infoList[2]));
        list.Add(BoomerangShieldInit(infoList[1]));
        list.Add(new IdleSkillAbility(this, AfterBoomerangShieldWaitTimeArray[mHertIndex])); // ���ն��ƺ��վ��ʱ��
        list.Add(ShieldStrikeInit(infoList[2]));
        list.Add(SummonSoldiersInit(infoList[3]));

        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }


    public override void MUpdate()
    {
        if(ShieldMovementTimeLeft>0)
            ShieldMovementTimeLeft--;
        base.MUpdate();
    }

    public override void OnIdleStateEnter()
    {

    }

    public override void OnMoveStateEnter()
    {
        
    }

    /// <summary>
    /// ׼���Ӷ���ʱ�Ķ���
    /// </summary>
    public override void OnCastStateEnter()
    {
        animatorController.Play("PreDrop");
        DropWaitTimeLeft = DropWaitTimeArray[mHertIndex];
    }

    /// <summary>
    /// ��ʩ������ʱ����CD����
    /// </summary>
    public override void OnCastState()
    {
        if(DropWaitTimeLeft>0)
            DropWaitTimeLeft--;
    }

    /// <summary>
    /// ����
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        animatorController.Play("PreStand");
    }

    public override void OnTransitionState()
    {
        if (StandTimeLeft > 0)
            StandTimeLeft--;
    }

    /// <summary>
    /// ����������������״̬�±�ը�������ᱻ����3s
    /// </summary>
    /// <param name="dmg"></param>
    public override void OnBombBurnDamage(float dmg)
    {
        if (isPreDrop)
            AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(this, 180, true));
        base.OnBombBurnDamage(dmg);
    }

    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0.49f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    /// <summary>
    /// ����Բ���ӵ�
    /// </summary>
    public override void BeforeDeath()
    {
        RecycleShieldBullet();
        base.BeforeDeath();
    }

    /// <summary>
    /// ����Բ���ӵ�
    /// </summary>
    public override void ExecuteRecycle()
    {
        RecycleShieldBullet();
        base.ExecuteRecycle();
    }

    //////////////////////////////////////////////����ΪBOSS���ܶ���////////////////////////////////////////////////////////////
    /// <summary>
    /// ˲���ƶ�
    /// </summary>
    private CustomizationSkillAbility TeleportInit()
    {
        ////
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        TeleportSkill = c;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate 
        {
            if (isHasShield)
                animatorController.Play("PreMove0");
            else
                animatorController.Play("PreMove1");
            CloseCollision();
        };
        {
            // ˲��ǰҡ
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // ������ƶ�ǰҡ������������˲�Ʊ��λ��Ȼ�������ƶ���ҡ
                    Vector2 v = GetNextGridIndex();
                    transform.position = MapManager.GetGridLocalPosition(v.x, v.y);
                    if (isHasShield)
                        animatorController.Play("PostMove0");
                    else
                        animatorController.Play("PostMove1");
                    return true;
                }
                return false;
            });
            // ǰҡת��ҡ
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    return true;
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate 
        {
            if (isHasShield)
                animatorController.Play("Idle0", true);
            else
                animatorController.Play("Idle1", true);
            OpenCollision();
        };
        return c;
    }

    /// <summary>
    /// ����Բ��
    /// </summary>
    /// <param name="info"></param>
    private CustomizationSkillAbility BoomerangShieldInit(SkillAbility.SkillAbilityInfo info)
    {
        // ��ǰ״̬ 0 ˲�� 1 ǰҡ 2 ���� 3 ��ҡ 4 ˲�� 5 ͣ�� 6 ����
        ////
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        BoomerangShield = c;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            SetNextGridIndex(8, 6); // ȥ7·
            c.ActivateChildAbility(TeleportSkill);
        };
        {
            // ˲��תǰҡ
            c.AddSpellingFunc(delegate {
                if (true || !TeleportSkill.isSpelling)
                {
                    SetActionState(new CastState(this));
                    return true;
                }
                return false;
            });
            // ǰҡת��������
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    isPreDrop = true;
                    animatorController.Play("Drop", true);
                    return true;
                }
                return false;
            });
            // ��������ת��ҡ
            c.AddSpellingFunc(delegate {
                if (DropWaitTimeLeft <= 0)
                {
                    isPreDrop = false;
                    isHasShield = false;
                    animatorController.Play("PostDrop");
                    return true;
                }
                return false;
            });
            // ���䵯��֡
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>=0.5f)
                {
                    DropShieldBullet();
                    return true;
                }
                return false;
            });
            // ��ҡ������ת˲��
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    ShieldMovementTimeLeft = ShieldMovementTimeArray[mHertIndex];
                    SetNextGridIndex(8, 0); // ȥ1·
                    c.ActivateChildAbility(TeleportSkill);
                    return true;
                }
                return false;
            });
            // ˲�Ƶ�һ·��תͣ�ͣ� ׼�����ն���
            c.AddSpellingFunc(delegate {
                if (ShieldMovementTimeLeft<=0)
                {
                    animatorController.Play("Recieve");
                    return true;
                }
                return false;
            });
            // ��������ƺ󲥷�ͣ�Ͷ���ͬʱ�˳��ü���
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    isHasShield = true;
                    if (isHasShield)
                        animatorController.Play("Idle0", true);
                    else
                        animatorController.Play("Idle1", true);
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate{ };
        return c;
    }

    /// <summary>
    /// Բ���Ļ����ܶ���
    /// </summary>
    private CustomizationSkillAbility ShieldStrikeInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        ShieldStrike = c;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate {  };
        {
            for (int i = 0; i < StrikeCountArray[mHertIndex]; i++)
            {
                // ��˲��
                c.AddSpellingFunc(delegate {
                    SetNextGridIndexByRandom(2, 8, 0, 6);
                    c.ActivateChildAbility(TeleportSkill);
                    return true;
                });
                // ˲��ת����
                c.AddSpellingFunc(delegate {
                    if (!TeleportSkill.isSpelling)
                    {
                        animatorController.Play("Attack");
                        return true;
                    }
                    return false;
                });
                // �����ж�����ͬʱ���ж�
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>0.5f)
                    {
                        // ��Ч
                        BaseEffect effect = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Boss/20/Strike"), null, "StrikeEffect", null, false);
                        effect.transform.position = transform.position;
                        GameController.Instance.AddEffect(effect);
                        // ��ǰ������Ƭ���900���˺�
                        DamageAreaEffectExecution dmgEffect = DamageAreaEffectExecution.GetInstance();
                        dmgEffect.Init(this, CombatAction.ActionType.CauseDamage, 900, GetRowIndex(), 2, 1, -1.5f, 0, true, false);
                        dmgEffect.transform.position = transform.position;
                        GameController.Instance.AddAreaEffectExecution(dmgEffect);
                        return true;
                    }
                    return false;
                });
                // ����������ȴ�
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        c.ActivateChildAbility(new IdleSkillAbility(this, StrikeWaitTimeArray[mHertIndex]));
                        if (isHasShield)
                            animatorController.Play("Idle0", true);
                        else
                            animatorController.Play("Idle1", true);
                        return true;
                    }
                    return false;
                });
                // ������������ȴ���
                c.AddSpellingFunc(delegate {
                    return true;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// �ٻ�ʿ��
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility SummonSoldiersInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        SummonSoldiers = c;
        // ʵ��
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate 
        {
            SetNextGridIndex(7, 3);
            c.ActivateChildAbility(TeleportSkill);
        };
        {
            // ˲��ת����ǰҡ
            c.AddSpellingFunc(delegate {
                if (!TeleportSkill.isSpelling)
                {
                    StandTimeLeft = StandTimeArray[mHertIndex];
                    SetActionState(new TransitionState(this));
                    return true;
                }
                return false;
            });
            // ����ǰҡת����վ׮
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Stand", true);
                    return true;
                }
                return false;
            });
            // ����ٻ�ʿ��
            {
                int max = 5;
                int i = 0;
                int interval = 12;
                int time = 0;
                c.AddSpellingFunc(delegate {
                    time++;
                    if (time == interval)
                    {
                        time = 0;
                        i++;
                        CreateEnemySpawner(6, i);
                    }
                    if (i == max)
                    {
                        // �ٶ����ٻ�������ĸ
                        for (int i = 0; i < 2; i++)
                        {
                            GameController.Instance.CreateMouseUnit(1+4*i, new BaseEnemyGroup.EnemyInfo() { type = (int)MouseNameTypeMap.AirTransportMouse, shape = 0 });
                        }
                        return true;
                    }
                    return false;
                });
            }
            

            // ����վ׮ת�����ҡ
            c.AddSpellingFunc(delegate {
                if (StandTimeLeft <= 0)
                {
                    animatorController.Play("PostStand");
                    return true;
                }
                return false;
            });
            // ��ҡת�˳�
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
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
    /// �ӳ�Բ��
    /// </summary>
    private void DropShieldBullet()
    {
        float s = 0;
        for (int i = 1; i < pathList.Count; i++)
        {
            s += (pathList[i] - pathList[i - 1]).magnitude;
        }
        float v = s/ShieldMovementTimeArray[mHertIndex];
        // �����ÿ�εķ�����ʱ��
        List<Vector2> rotList = new List<Vector2>();
        List<int> timeList = new List<int>();
        for (int i = 1; i < pathList.Count; i++)
        {
            rotList.Add((pathList[i] - pathList[i - 1]).normalized);
            timeList.Add(Mathf.CeilToInt((pathList[i] - pathList[i - 1]).magnitude/v));
        }
        // ����һ���������Լ�����
        CustomizationTask task = new CustomizationTask();
        task.OnEnterFunc = delegate 
        {
            // Բ���ӵ�����
            CreateShieldBullet();
            // ����λ��Ϊ����ǰ��һ��
            ShieldBullet.transform.localPosition = MapManager.gridWidth * Vector2.left;
            // 
            ShieldBullet.transform.SetParent(GameController.Instance.transform);
            GameController.Instance.AddBullet(ShieldBullet);
        };
        for (int i = 0; i < rotList.Count; i++)
        {
            Vector3 rot = rotList[i];
            int time = timeList[i];
            int t = 0;
            task.AddTaskFunc(delegate 
            {
                ShieldBullet.transform.position += v * rot;
                t++;
                if (t >= time)
                {
                    return true;
                }
                return false;
            });
        }
        task.OnExitFunc = delegate 
        {
            RecycleShieldBullet();
        };
        AddTask(task);
    }

    /// <summary>
    /// ����Բ���ӵ�
    /// </summary>
    private void CreateShieldBullet()
    {
        ShieldBullet.gameObject.SetActive(true);
        ShieldBullet.MInit();
        ShieldBullet.SetRuntimeAnimatorController(ShieldBulletAnimaotrController);
        ShieldBullet.isAffectCharacter = false; // ��Ӱ����
        ShieldBullet.isnKillSelf = true;
        ShieldBullet.SetDamage(900);
        ShieldBullet.animatorController.Play("Fly", true);
    }

    /// <summary>
    /// ����Բ���ӵ�
    /// </summary>
    private void RecycleShieldBullet()
    {
        ShieldBullet.gameObject.SetActive(false);
        ShieldBullet.transform.SetParent(transform);
        ShieldBullet.transform.localPosition = Vector3.zero;
        GameController.Instance.RemoveBullet(ShieldBullet);
    }

    /// <summary>
    /// ����һ������������
    /// </summary>
    private void CreateEnemySpawner(int xIndex, int yIndex)
    {
        CustomizationItem item = CustomizationItem.GetInstance(MapManager.GetGridLocalPosition(xIndex, yIndex), GameManager.Instance.GetRuntimeAnimatorController("Boss/20/RainBow"));
        CustomizationTask task = new CustomizationTask();
        task.OnEnterFunc = delegate 
        {
            item.animatorController.Play("RainBow");
        };
        task.AddTaskFunc(delegate 
        {
            // ���ɹ�
            if (item.animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.8f)
            {
                GameController.Instance.CreateMouseUnit(xIndex, yIndex, new BaseEnemyGroup.EnemyInfo() { type=(int)MouseNameTypeMap.PandaMouse, shape = 1 });
                return true;
            }
            return false;
        });
        task.AddTaskFunc(delegate
        {
            // �ȴ���������
            if (item.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                return true;
            }
            return false;
        });
        task.OnExitFunc = delegate 
        {
            item.ExecuteRecycle();
        };
        item.AddTask(task);
        GameController.Instance.AddItem(item);
    }
}
