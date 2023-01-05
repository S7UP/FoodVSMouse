using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 列车二阶
/// </summary>
public class RatTrain1 : BaseRatTrain
{
    private static RuntimeAnimatorController FogCreator_RuntimeAnimatorController;
    private static RuntimeAnimatorController LaserAttacker_RuntimeAnimatorController;
    private static RuntimeAnimatorController LaserEffect_RuntimeAnimatorController;
    private static RuntimeAnimatorController FireBulletAttacker_RuntimeAnimatorController;
    private static RuntimeAnimatorController Head_RuntimeAnimatorController;
    private static RuntimeAnimatorController Body_RuntimeAnimatorController;
    private static RuntimeAnimatorController FireBullet_RuntimeAnimatorController;
    private static RuntimeAnimatorController Bomb_RuntimeAnimatorController;


    private static string LaserEffectKey = "RatTrain1_LaserEffectKey";

    private FloatModifier moveSpeedModifier = new FloatModifier(0); // 移速提升标签
    private List<BaseUnit> retinueList = new List<BaseUnit>(); // 自身随从表（在死亡时同时销毁所有随从）
    private Dictionary<int, List<BaseUnit>> laserAttackerPairDict = new Dictionary<int, List<BaseUnit>>(); // 激光发射器对字典
    

    private int attackRound; // 当前攻击轮数（一次技能循环视为一轮）

    public override void Awake()
    {
        if(FogCreator_RuntimeAnimatorController == null)
        {
            FogCreator_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/FogCreator");
            LaserAttacker_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/LaserAttacker");
            LaserEffect_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/LaserEffect");
            FireBulletAttacker_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/FireBulletAttacker");
            Head_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Head");
            Body_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Body");
            FireBullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/FireBullet");
            Bomb_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Bomb");
        }
        base.Awake();
    }

    public override void MInit()
    {
        attackRound = 0;
        retinueList.Clear();
        laserAttackerPairDict.Clear();
        base.MInit();
        // 创建13节车厢
        CreateHeadAndBody(13);
        // 设置车头、车身、车尾的伤害倍率
        GetHead().SetDmgRate(GetParamValue("head_normal", mHertIndex), GetParamValue("head_burn", mHertIndex));
        foreach (var item in GetBodyList())
        {
            item.SetDmgRate(GetParamValue("body_normal", mHertIndex), GetParamValue("body_burn", mHertIndex));
        }
        GetBody(GetBodyList().Count - 1).SetDmgRate(GetParamValue("tail_normal", mHertIndex), GetParamValue("tail_burn", mHertIndex));
        // 默认速度为2.4纵格/每秒
        NumericBox.MoveSpeed.SetBase(2.4f*MapManager.gridHeight/60);
        SetAllComponentNoBeSelectedAsTargetAndHited();
        SetActionState(new MoveState(this));
    }

    public override void MUpdate()
    {
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var u in retinueList)
        {
            if (!u.IsAlive())
                delList.Add(u);
        }
        foreach (var u in delList)
        {
            retinueList.Remove(u);
        }

        base.MUpdate();
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    public override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.667f, 0.333f });
        // 移动速度
        AddParamArray("speed_rate", new float[] { 1.0f, 1.25f, 1.5f });
        // 车头、车身、车尾的受伤倍率
        AddParamArray("head_normal", new float[] { 1.0f });
        AddParamArray("head_burn", new float[] { 1.0f });
        AddParamArray("body_normal", new float[] { 1.0f });
        AddParamArray("body_burn", new float[] { 1.0f });
        AddParamArray("tail_normal", new float[] { 1.0f });
        AddParamArray("tail_burn", new float[] { 10.0f }); // 车尾10倍爆破伤害
        // 侧路对流攻击式停靠时间
        AddParamArray("wait0", new float[] { 9, 7, 6});
        // 中线迷雾袭击式停靠时间
        AddParamArray("wait1", new float[] { 9, 7, 6});

        // 指令-雾袭
        AddParamArray("t0_0", new float[] { 3, 3, 2 });
        AddParamArray("fog0_0", new float[] { 3, 4, 5 });
        AddParamArray("soldier_type", new float[] { 0, 0, 20 });
        AddParamArray("soldier_shape", new float[] { 7, 8, 0 });
        AddParamArray("stun0_0", new float[] { 1, 2, 3 });
        AddParamArray("fog_hp0_0", new float[] { 900 });

        // 指令-对流激光
        AddParamArray("t1_0", new float[] { 5, 3, 2 });
        AddParamArray("dmg1_0", new float[] { 900 });
        AddParamArray("laser_hp", new float[] { 2700 });

        // 指令-火力支援
        AddParamArray("t2_0", new float[] { 1.5f, 1, 0 });
        AddParamArray("num2_0", new float[] { 1, 2, 2 });
        AddParamArray("dmg2_0", new float[] { 900 });
        AddParamArray("dmg2_1", new float[] { 900 });
        AddParamArray("t2_1", new float[] { 0, 0, 0 });
        AddParamArray("soldier_type2_0", new float[] { 19, 19, 19 });
        AddParamArray("soldier_shape2_0", new float[] { 1, 1, 1 });
        AddParamArray("stun2_0", new float[] { 5, 5, 5 });
        AddParamArray("fog_hp2_0", new float[] { 3600 });
        AddParamArray("fog2_0", new float[] { 5, 5, 5 });
    }

    /// <summary>
    /// 切换阶段、加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        // 设置获取技能组的方法
        mSkillQueueAbilityManager.SetGetNextSkillIndexQueueFunc(GetNextAttackList);

        if(mHertIndex == 0)
        {
            LoadP1SkillAbility();
        }
        else if (mHertIndex == 1)
        {
            LoadP2SkillAbility();
        }
        else
        {
            LoadP3SkillAbility();
        }
    }

    private void LoadP1SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float fog_hp0_0 = GetParamValue("fog_hp0_0", mHertIndex);

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        float laser_hp = GetParamValue("laser_hp", mHertIndex);
        
        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex) * 60);
        int num2_0 = Mathf.FloorToInt(GetParamValue("num2_0", mHertIndex));
        float dmg2_0 = GetParamValue("dmg2_0", mHertIndex);
        float dmg2_1 = GetParamValue("dmg2_1", mHertIndex);

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(dmg1_0, t1_0, laser_hp));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }, t2_0, num2_0, dmg2_0
            ));
        list.Add(Movement2(t2_0, num2_0, dmg2_0, dmg2_1));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP2SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float fog_hp0_0 = GetParamValue("fog_hp0_0", mHertIndex);

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        float laser_hp = GetParamValue("laser_hp", mHertIndex);

        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex) * 60);
        int num2_0 = Mathf.FloorToInt(GetParamValue("num2_0", mHertIndex));
        float dmg2_0 = GetParamValue("dmg2_0", mHertIndex);
        float dmg2_1 = GetParamValue("dmg2_1", mHertIndex);

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(dmg1_0, t1_0, laser_hp));
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }, t2_0, num2_0, dmg2_0
            ));
        list.Add(Movement2(t2_0, num2_0, dmg2_0, dmg2_1));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    private void LoadP3SkillAbility()
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60);
        int fog0_0 = Mathf.FloorToInt(GetParamValue("fog0_0", mHertIndex) * 60);
        int soldier_type = Mathf.FloorToInt(GetParamValue("soldier_type", mHertIndex));
        int soldier_shape = Mathf.FloorToInt(GetParamValue("soldier_shape", mHertIndex));
        int stun0_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float fog_hp0_0 = GetParamValue("fog_hp0_0", mHertIndex);

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60);
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        float laser_hp = GetParamValue("laser_hp", mHertIndex);

        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex) * 60);
        int num2_0 = Mathf.FloorToInt(GetParamValue("num2_0", mHertIndex));
        float dmg2_0 = GetParamValue("dmg2_0", mHertIndex);
        float dmg2_1 = GetParamValue("dmg2_1", mHertIndex);

        List<SkillAbility> list = new List<SkillAbility>();
        list.Add(Movement0(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateLaserAttackerAction(dmg1_0, t1_0, laser_hp));
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }
            ));
        list.Add(Movement1(
            delegate
            {
                List<Action<int, int>> actionList = new List<Action<int, int>>();
                actionList.Add(CreateFogCreatorAction(t0_0, soldier_type, soldier_shape, stun0_0, fog0_0, fog_hp0_0));
                return actionList;
            }, t2_0, num2_0, dmg2_0
            ));
        list.Add(Movement2(t2_0, num2_0, dmg2_0, dmg2_1));
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 获取下一组攻击技能顺序
    /// </summary>
    /// <returns></returns>
    private List<int> GetNextAttackList()
    {
        List<int> list = new List<int>();
        // 检测有没有用编辑器自定义攻击顺序
        if(!IsParamValueInValidOrOutOfBound("attack_order", attackRound))
        {
            int val = Mathf.FloorToInt(GetParamValue("attack_order", attackRound));
            while(val > 0)
            {
                int i = val % 10;
                i = Mathf.Max(0, i - 1);
                list.Insert(0, i);
                val /= 10;
            }
        }
        else
        {
            // 如果没有的话由BOSS的当前种子自行生成
            List<int> l = new List<int>() { 0,1,2 };
            while (l.Count > 0)
            {
                int i = GetRandomNext(0, l.Count);
                list.Add(l[i]);
                l.Remove(l[i]);
            }
        }
        attackRound++;
        return list;
    }

    public override void BeforeDeath()
    {
        foreach (var u in retinueList)
        {
            u.TaskList.Clear();
            u.ExecuteDeath();
        }
        retinueList.Clear();
        base.BeforeDeath();
    }

    public override void BeforeBurn()
    {
        foreach (var u in retinueList)
        {
            u.TaskList.Clear();
            u.ExecuteBurn();
        }
        retinueList.Clear();
        base.BeforeBurn();
    }

    public override void BeforeDrop()
    {
        foreach (var u in retinueList)
        {
            u.TaskList.Clear();
            u.ExecuteDrop();
        }
        retinueList.Clear();
        base.BeforeDrop();
    }

    /// <summary>
    /// 创建一个激光发射器
    /// </summary>
    private BaseUnit CreateLaserAttacker(RatTrainComponent master, bool isLeft, float laser_hp)
    {
        MouseModel m = MouseModel.GetInstance(LaserAttacker_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true)); // 免疫灰烬秒杀
            m.SetBaseAttribute(laser_hp, 1, 1f, 0, 0, 0, 0);
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = -master.moveRotate;
            m.transform.localScale = new Vector2(1, (isLeft ? -1 : 1) * Mathf.Sign(master.moveRotate.x));
            // m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // 不可作为选取的目标
            m.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            // m.AddCanHitFunc(delegate { return false; }); // 不可被子弹击中
            m.mBoxCollider2D.offset = new Vector2(0, 1.0f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            // 受到一次灰烬伤害后固定损失50%最大生命值真实伤害
            Action<CombatAction> OnBombDamage = (action) => {
                if (action is DamageAction)
                {
                    var damageAction = action as DamageAction;
                    if(damageAction is BombDamageAction)
                    {
                        damageAction.DamageValue = Mathf.Max(damageAction.DamageValue, 0.5f * m.mMaxHp); // 伤害值转为50%最大生命值伤害与原伤害较小者
                    }
                }
            };
            m.AddActionPointListener(ActionPointType.PreReceiveDamage, OnBombDamage);
            m.AddActionPointListener(ActionPointType.PreReceiveReboundDamage, OnBombDamage);
            // 收纳为自身随从
            retinueList.Add(m);
        }
        GameController.Instance.AddMouseUnit(m);
        m.UpdateRenderLayer(master.spriteRenderer.sortingOrder -1); // 图层-1
        return m;
    }

    /// <summary>
    /// 生成激光发射器并发布执行它们逻辑的任务
    /// </summary>
    /// <param name="waitTime"></param>
    /// <param name="dmg"></param>
    private void LinkLaserAttackers(int waitTime, float dmg, float laser_hp)
    {
        laserAttackerPairDict.Clear();
        // 生成激光发射器并且串联同一列的激光发射器
        foreach (var body in GetBodyList())
        {
            if (body != null && !body.IsHide() && body.transform.position.x >= MapManager.GetColumnX(-1) && body.transform.position.x <= MapManager.GetColumnX(9))
            {
                int columnIndex = int.MinValue;
                BaseUnit u = null;
                if (body.transform.position.y > MapManager.GetRowY(3f))
                {
                    u = CreateLaserAttacker(body, false, laser_hp);
                    columnIndex = body.GetColumnIndex();
                }
                else
                {
                    u = CreateLaserAttacker(body, false, laser_hp);
                    columnIndex = body.GetColumnIndex();
                }

                if(columnIndex != int.MinValue)
                {
                    if (!laserAttackerPairDict.ContainsKey(columnIndex))
                    {
                        laserAttackerPairDict.Add(columnIndex, new List<BaseUnit>());
                    }
                    if(u!=null)
                        laserAttackerPairDict[columnIndex].Add(u);
                }
            }
        }
        // 为自身添加一个任务，每帧检测激光发射器的存活情况并剔除不存活的激光发射器，在等待时间结束后释放激光
        CustomizationTask t = new CustomizationTask();
        t.OnEnterFunc = delegate
        {
            foreach (var u in GetAllLaserAttacker())
            {
                u.animatorController.Play("PreAttack");
            }
        };
        t.AddTaskFunc(delegate
        {
            UpdateLaserAttackerPairDict();
            List<BaseUnit> l = GetAllLaserAttacker();
            if (l.Count <= 0)
                return true;
            if (l[0].animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                foreach (var u in l)
                {
                    u.animatorController.Play("Attack", true);
                }
                // 生成激光特效
                foreach (var keyValuePair in laserAttackerPairDict)
                {
                    List<BaseUnit> list = keyValuePair.Value;
                    if (list.Count >= 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            BaseUnit u = list[i];
                            Vector2 rot = u.transform.position.y > MapManager.GetRowY(3f) ? Vector2.down : Vector2.up;
                            BaseEffect e = BaseEffect.CreateInstance(LaserEffect_RuntimeAnimatorController, null, "PreAttack", "Disappear", true);
                            if (rot.Equals(Vector2.up))
                                e.transform.localScale = new Vector2(1, -1);
                            GameController.Instance.AddEffect(e);
                            u.AddEffectToDict(LaserEffectKey, e, 1f*Vector2.up*MapManager.gridHeight);
                        }
                    }
                }
                return true;
            }
            return false;
        });
        t.AddTaskFunc(delegate
        {
            if (waitTime > 0)
            {
                waitTime--;
                UpdateLaserAttackerPairDict();
                return false;
            }
            {
                List<BaseUnit> laserList = GetAllLaserAttacker();
                foreach (var u in laserList)
                {
                    u.animatorController.Play("PostAttack");
                    u.RemoveEffectFromDict(LaserEffectKey);
                    u.CloseCollision();
                }
                // 发射激光
                foreach (var keyValuePair in laserAttackerPairDict)
                {
                    List<BaseUnit> list = keyValuePair.Value;
                    if(list.Count >= 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            BaseUnit u = list[i];
                            Vector2 rot = u.transform.localScale.y > 0 ? Vector2.up : Vector2.down;
                            BaseEffect e = BaseEffect.CreateInstance(LaserEffect_RuntimeAnimatorController, null, "PostAttack", null, false);
                            if (rot.Equals(Vector2.up))
                                e.transform.localScale = new Vector2(1, -1);
                            GameController.Instance.AddEffect(e);
                            u.AddEffectToDict(LaserEffectKey, e, 1.5f*Vector2.up * MapManager.gridHeight);
                            // 产生真正的激光判定
                            BombAreaEffectExecution r = BombAreaEffectExecution.GetInstance(this, dmg, new Vector2(u.transform.position.x, MapManager.GetRowY(3)), 0.5f, 5);
                            r.isAffectFood = true;
                            r.isAffectMouse = true;
                            r.isAffectCharacter = false;
                            foreach (var laserUnit in retinueList)
                            {
                                if(laserUnit is MouseUnit)
                                {
                                    MouseUnit m = laserUnit as MouseUnit;
                                    r.AddExcludeMouseUnit(m); // 不能毁掉自身的组件
                                }
                            }
                            GameController.Instance.AddAreaEffectExecution(r);
                        }
                    }
                }
                return true;
            }
        });
        t.AddTaskFunc(delegate
        {
            UpdateLaserAttackerPairDict();
            List<BaseUnit> l = GetAllLaserAttacker();
            if (l.Count <= 0)
                return true;
            if (l[0].animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                return true;
            }
            return false;
        });
        t.OnExitFunc = delegate {
            foreach (var u in GetAllLaserAttacker())
            {
                u.ExecuteRecycle(); // 直接回收，不附带死亡动画也不触发亡语（如果有）
            }
        };
        AddTask(t);
    }

    /// <summary>
    /// 更新激光发射器字典里的东西，剔除不必要的子项
    /// </summary>
    private void UpdateLaserAttackerPairDict()
    {
        foreach (var keyValuePair in laserAttackerPairDict)
        {
            List<BaseUnit> delList = new List<BaseUnit>();
            foreach (var u in keyValuePair.Value)
            {
                if (!u.IsAlive())
                    delList.Add(u);
            }
            foreach (var u in delList)
            {
                keyValuePair.Value.Remove(u);
            }
        }
    }

    /// <summary>
    /// 获取所有激光发射器
    /// </summary>
    /// <returns></returns>
    private List<BaseUnit> GetAllLaserAttacker()
    {
        List<BaseUnit> list = new List<BaseUnit>();
        foreach (var keyValuePair in laserAttackerPairDict)
        {
            foreach (var u in keyValuePair.Value)
            {
                list.Add(u);
            }
        }
        return list;
    }

    /// <summary>
    /// 召唤士兵
    /// </summary>
    private void SpawnEnemy(Vector2 startV2, Vector2 endV2, Vector2 rot, int type, int shape, int stun_time)
    {
        MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), 0,
                        new BaseEnemyGroup.EnemyInfo() { type = type, shape = shape });
        m.moveRotate = rot;
        if (m.moveRotate.x > 0)
            m.transform.localScale = new Vector2(-1, 1);
        // 一些初始出现动画不能被击中的效果
        Func<BaseUnit, BaseUnit, bool> noSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        CustomizationTask task = new CustomizationTask();
        int totalTime = 90;
        int currentTime = 0;
        task.OnEnterFunc = delegate
        {
            m.transform.position = startV2; // 对初始坐标进行进一步修正
            m.AddCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc); // 不可作为选取的目标
            m.AddCanBlockFunc(noBlockFunc); // 不可被阻挡
            m.AddCanHitFunc(noHitFunc); // 不可被子弹击中
            m.SetAlpha(0); // 0透明度
        };
        task.AddTaskFunc(delegate
        {
            if (currentTime <= totalTime)
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
            m.RemoveCanBeSelectedAsTargetFunc(noSelectedAsTargetFunc);
            m.RemoveCanBlockFunc(noBlockFunc);
            m.RemoveCanHitFunc(noHitFunc);
            // 自我晕眩
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, stun_time, false));
        };
        m.AddTask(task);
    }

    /// <summary>
    /// 创建一个喷雾器
    /// </summary>
    private BaseUnit CreateFogCreator(BaseUnit master, float hp, bool isLeft, int waitTime, int type, int shape, int stun_time, int fog_time)
    {
        MouseModel m = MouseModel.GetInstance(FogCreator_RuntimeAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true)); // 免疫灰烬秒杀
            m.SetBaseAttribute(hp, 1, 1f, 0, 0, 0, 0);
            m.transform.position = master.transform.position;
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.transform.right = -master.moveRotate;
            m.transform.localScale = new Vector2(1, (isLeft?-1:1)*Mathf.Sign(master.moveRotate.x));
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // 不可作为选取的目标
            m.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            m.AddCanHitFunc(delegate { return false; }); // 不可被子弹击中
            m.mBoxCollider2D.offset = new Vector2(0, 1f * MapManager.gridHeight);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            m.isBoss = true;
            // 收纳为自身随从
            retinueList.Add(m);

            // 动作
            {
                CustomizationTask t = new CustomizationTask();
                t.OnEnterFunc = delegate
                {
                    m.animatorController.Play("PreAttack");
                };
                t.AddTaskFunc(delegate
                {
                    if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        m.animatorController.Play("Attack", true);
                        return true;
                    }
                    return false;
                });
                t.AddTaskFunc(delegate
                {
                    if (waitTime > 0)
                    {
                        waitTime--;
                        return false;
                    }
                    {
                        m.animatorController.Play("PostAttack");
                        // 召唤一只小怪并释放迷雾
                        if (m.IsAlive())
                        {
                            Vector2 rot = m.transform.localScale.y > 0 ? Vector2.up : Vector2.down;
                            Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                            FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(pos + MapManager.gridHeight*rot);
                            e.SetOpen();
                            CustomizationTask t = new CustomizationTask();
                            t.AddTaskFunc(delegate {
                                if (fog_time > 0)
                                    fog_time--;
                                else
                                    return true;
                                return false;
                            });
                            t.OnExitFunc = delegate {
                                e.SetDisappear();
                            };
                            e.AddTask(t);
                            GameController.Instance.AddAreaEffectExecution(e);
                            SpawnEnemy(pos + 0.5f * rot * MapManager.gridWidth, pos + 1.0f * rot * MapManager.gridWidth, Vector2.left, type, shape, stun_time);
                            m.CloseCollision();
                        }
                        return true;
                    }
                });
                t.AddTaskFunc(delegate
                {
                    if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
                t.OnExitFunc = delegate {
                    m.ExecuteRecycle(); // 直接回收，不附带死亡动画也不触发亡语（如果有）
                };
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.UpdateRenderLayer(master.GetSpriteRenderer().sortingOrder - 2); // 图层-2
        }
        return m;
    }


    /// <summary>
    /// 发射一枚火炮
    /// </summary>
    /// <returns></returns>
    private EnemyBullet CreateFireBullet(BaseUnit master, Vector2 pos, Vector2 rotate, float dmg)
    {
        EnemyBullet b = EnemyBullet.GetInstance(FireBullet_RuntimeAnimatorController, master, 0);
        b.isAffectFood = true;
        b.isAffectCharacter = false;
        b.SetStandardVelocity(18);
        b.transform.position = pos;
        b.SetRotate(rotate);
        b.mCircleCollider2D.radius = 0.25f*MapManager.gridWidth;
        b.AddHitAction((b, u)=> {
            BombAreaEffectExecution r = BombAreaEffectExecution.GetInstance(master, dmg, u.transform.position, 0.75f, 0.75f);
            r.isAffectFood = true;
            r.isAffectCharacter = false;
            r.isAffectMouse = true;
            GameController.Instance.AddAreaEffectExecution(r);
        });
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// 创建一个投影
    /// </summary>
    private MouseModel CreateProjection(RuntimeAnimatorController runtimeAnimatorController, bool isFireAttacker)
    {
        MouseModel m = MouseModel.GetInstance(runtimeAnimatorController);
        {
            BossUnit.AddBossIgnoreDebuffEffect(m);
            m.SetBaseAttribute(mCurrentHp, 1, 1f, 0, 0, 0, 0);
            m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
            m.mBoxCollider2D.offset = new Vector2(0, 0);
            m.mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
            // 伤害转移给本体的方法
            Action<CombatAction> transToMaster = (action) => {
                if(action is DamageAction)
                {
                    var damageAction = action as DamageAction;
                    float dmg = damageAction.DamageValue*((isFireAttacker && damageAction is BombDamageAction) ? GetParamValue("tail_burn", mHertIndex) : GetParamValue("tail_normal", mHertIndex));
                    new DamageAction(action.mActionType, action.Creator, this, dmg).ApplyAction();
                }
            };
            m.AddActionPointListener(ActionPointType.PreReceiveDamage, transToMaster);
            m.AddActionPointListener(ActionPointType.PreReceiveReboundDamage, transToMaster);
            // 收纳为自身随从
            retinueList.Add(m);
            // 不触发猫，进家也不判输
            m.canTriggerCat = false;
            m.canTriggerLoseWhenEnterLoseLine = false;
            m.isBoss = true;

            // 动作
            {
                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate
                {
                    m.mCurrentHp = mCurrentHp; // 生命值与BOSS本体同步
                    return false;
                });
                m.AddTask(t);
            }
            GameController.Instance.AddMouseUnit(m);
            m.UpdateRenderLayer(GetBody(0).spriteRenderer.sortingOrder - 1); // 图层-1
        }
        return m;
    }

    /// <summary>
    /// 生成带炮台的列车
    /// </summary>
    private BaseUnit CreateFireBulletAttackerTrain(int t2_0, int totalAttackCount, float dmg, float boom_dmg)
    {
        List<BaseUnit> compList = new List<BaseUnit>();
        // 生成车头投影
        BaseUnit head = CreateProjection(Head_RuntimeAnimatorController, false);
        compList.Add(head);
        // 生成车身与炮台投影
        compList.Add(CreateProjection(Body_RuntimeAnimatorController, false));
        BaseUnit fireBulletAttacker = CreateProjection(FireBulletAttacker_RuntimeAnimatorController, true);
        compList.Add(fireBulletAttacker);
        compList.Add(CreateProjection(Body_RuntimeAnimatorController, false));

        Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        // 以下为其行为控制
        foreach (var u in compList)
        {
            u.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc); // 不可作为选取的目标
            u.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            u.AddCanHitFunc(noHitFunc); // 不可被子弹击中
            u.CloseCollision();
            u.moveRotate = Vector2.left;
            u.transform.position = MapManager.GetGridLocalPosition(8, -3); // 直接藏在屏幕外边
        }
        float maxDist = GetHeadToBodyLength();
        float distLeft = GetHeadToBodyLength();
        int count = 1; // 出现的车组件数量（头和炮台也算的）
        int timeLeft = t2_0;
        int currentAttackCount = 0;
        Vector2 start = MapManager.GetGridLocalPosition(9, 3);
        Vector2 stop = MapManager.GetGridLocalPosition(5, 3);
        head.transform.position = start;
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            distLeft -= mCurrentMoveSpeed;
            for (int i = 0; i < count; i++)
            {
                BaseUnit u = compList[i];
                u.transform.position += (Vector3)u.moveRotate * mCurrentMoveSpeed;
                if (i == count - 1)
                {
                    u.animatorController.Play("Appear", false, 0.75f*(1-distLeft/maxDist));
                }
                else
                {
                    u.animatorController.Play("Move", true);
                }
            }
            if(distLeft <= 0)
            {
                if(count < compList.Count)
                {
                    compList[count].transform.position = start - compList[count].moveRotate * distLeft;
                    compList[count].OpenCollision();
                }
                count++;
                maxDist = GetBodyLength();
                distLeft += maxDist;
            }
            return count > compList.Count;
        });
        t.AddTaskFunc(delegate {
            // 继续前进
            foreach (var u in compList)
            {
                u.transform.position += (Vector3)u.moveRotate * mCurrentMoveSpeed;
            }
            if (fireBulletAttacker.transform.position.x < stop.x)
            {
                head.moveRotate = Vector2.left;
                compList[1].moveRotate = Vector2.left;
                compList[3].moveRotate = Vector2.right;
                foreach (var u in compList)
                {
                    if (u != fireBulletAttacker)
                    {
                        u.animatorController.Play("Disappear");
                    } 
                }
                return true;
            }
            return false;
        });
        // 停靠时分类讨论：车头和第一节车厢继续前进并消失，车尾倒退消失
        t.AddTaskFunc(delegate {
            // 继续前进
            bool flag = false;
            foreach (var u in compList)
            {
                if (u != fireBulletAttacker)
                {
                    u.transform.position += (Vector3)u.moveRotate * mCurrentMoveSpeed;
                    if (u.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                // 炮台架起，变得可被攻击
                fireBulletAttacker.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                fireBulletAttacker.RemoveCanHitFunc(noHitFunc);
                fireBulletAttacker.animatorController.Play("PreAttack");
                // 清理掉其他车厢
                foreach (var u in compList)
                {
                    if (u != fireBulletAttacker)
                        u.ExecuteRecycle();
                }
                return true;
            }
            return false;
        });
        // 如果是第三阶段，则追加迷雾攻击
        if(mHertIndex >= 2)
        {
            t.AddTaskFunc(delegate {
                CreateFogAreaToFireBulletAttacker(fireBulletAttacker, fireBulletAttacker.GetSpriteRenderer().sortingOrder - 1);
                return true;
            });
        }

        // 打开炮台
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = t2_0;
                fireBulletAttacker.animatorController.Play("AttackWait0");
                return true;
            }
            return false;
        });
        // 等一会儿
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rot = new Vector2(Mathf.Cos(90*i*Mathf.PI/180), Mathf.Sin(90 * i * Mathf.PI / 180));
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f*MapManager.gridWidth*rot, rot, dmg);
                }
                fireBulletAttacker.animatorController.Play("Attack0");
                return true;
            }
        });
        // 连续攻击
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                currentAttackCount++;
                if (currentAttackCount < totalAttackCount)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos(90 * i * Mathf.PI / 180), Mathf.Sin(90 * i * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
                    }
                    fireBulletAttacker.animatorController.Play("Attack0", false, 0);
                }
                else
                {
                    // 转一下
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("TurnAttack");
                    return true;
                }
            }
            return false;
        });
        // 在转了
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = t2_0;
                fireBulletAttacker.animatorController.Play("AttackWait1");
                return true;
            }
            return false;
        });
        // 再等一会儿
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
                }
                fireBulletAttacker.animatorController.Play("Attack1");
                return true;
            }
        });
        // 连续攻击
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                currentAttackCount++;
                if (currentAttackCount < totalAttackCount)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
                    }
                    fireBulletAttacker.animatorController.Play("Attack1", false, 0);
                }
                else
                {
                    // 转一下
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("PostAttack");
                    return true;
                }
            }
            return false;
        });
        // 前二阶段收起来然后润，第三阶段开始原地自爆
        if(mHertIndex < 2)
        {
            // 收起来,然后润了
            t.AddTaskFunc(delegate {
                if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    fireBulletAttacker.animatorController.Play("Disappear");
                    fireBulletAttacker.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                    fireBulletAttacker.AddCanHitFunc(noHitFunc);
                    return true;
                }
                return false;
            });
            // 动画播放完清理掉自己
            t.AddTaskFunc(delegate {
                if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    fireBulletAttacker.ExecuteRecycle();
                    return true;
                }
                else
                {
                    fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
                }
                return false;
            });
        }
        else
        {
            // 自爆！
            t.AddTaskFunc(delegate {
                if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    BaseEffect e = BaseEffect.CreateInstance(Bomb_RuntimeAnimatorController, null, "BoomDie", null, false);
                    e.transform.position = fireBulletAttacker.transform.position;
                    GameController.Instance.AddEffect(e);
                    fireBulletAttacker.ExecuteRecycle();
                    // 产生3*3爆破效果
                    BombAreaEffectExecution r = BombAreaEffectExecution.GetInstance(this, boom_dmg, fireBulletAttacker.transform.position, 3, 3);
                    r.isAffectFood = true;
                    r.isAffectMouse = true;
                    GameController.Instance.AddAreaEffectExecution(r);
                    return true;
                }
                return false;
            });
        }

        AddTask(t);
        return fireBulletAttacker;
    }

    /// <summary>
    /// 生成雾域给P3的炮台
    /// </summary>
    private void CreateFogAreaToFireBulletAttacker(BaseUnit master, int sortingOrder)
    {
        int t2_1 = Mathf.FloorToInt(GetParamValue("t2_1", mHertIndex) * 60);
        int fog2_0 = Mathf.FloorToInt(GetParamValue("fog2_0", mHertIndex) * 60);
        int soldier_type2_0 = Mathf.FloorToInt(GetParamValue("soldier_type2_0", mHertIndex));
        int soldier_shape2_0 = Mathf.FloorToInt(GetParamValue("soldier_shape2_0", mHertIndex));
        int stun2_0 = Mathf.FloorToInt(GetParamValue("stun0_0", mHertIndex) * 60);
        float fog_hp2_0 = GetParamValue("fog_hp2_0", mHertIndex);
        BaseUnit[] arr = new BaseUnit[2];
        arr[0] = CreateFogCreator(master, fog_hp2_0, true, t2_1, soldier_type2_0, soldier_shape2_0, stun2_0, fog2_0);
        arr[0].GetSpriteRenderer().sortingOrder = sortingOrder;
        arr[1] = CreateFogCreator(master, fog_hp2_0, false, t2_1, soldier_type2_0, soldier_shape2_0, stun2_0, fog2_0);
        arr[1].GetSpriteRenderer().sortingOrder = sortingOrder;
        int timeLeft = 67 + t2_1;
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
                timeLeft--;
            else
            {
                Vector2[] v2List = new Vector2[4] {
                    new Vector2(MapManager.gridWidth, MapManager.gridHeight),
                    new Vector2(MapManager.gridWidth, 0),
                    new Vector2(-MapManager.gridWidth, 0),
                    new Vector2(-MapManager.gridWidth, MapManager.gridHeight)
                };
                // 填充其他地方的雾和老鼠
                foreach (var m in arr)
                {
                    if (m.IsAlive())
                    {
                        int r = m.transform.localScale.y > 0 ? 1 : -1;
                        Vector2 pos = new Vector2(m.transform.position.x, MapManager.GetRowY(m.GetRowIndex()));
                        foreach (var v2 in v2List)
                        {
                            FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(pos + new Vector2(v2.x, v2.y*r));
                            e.SetOpen();
                            CustomizationTask t = new CustomizationTask();
                            int fog_time = fog2_0;
                            t.AddTaskFunc(delegate {
                                if (fog_time > 0)
                                    fog_time--;
                                else
                                    return true;
                                return false;
                            });
                            t.OnExitFunc = delegate {
                                e.SetDisappear();
                            };
                            e.AddTask(t);
                            GameController.Instance.AddAreaEffectExecution(e);
                            SpawnEnemy(pos + 0.5f * new Vector2(v2.x, v2.y * r), pos + 1.0f * new Vector2(v2.x, v2.y * r), Vector2.left, soldier_type2_0, soldier_shape2_0, stun2_0);
                        }
                    }
                }
                return true;
            }
            return false;
        });
        AddTask(t);
    }

    /// <summary>
    /// 只生成一个带炮台的车尾（用于P3的雾攻击）
    /// </summary>
    private BaseUnit CreateFireBulletAttackerTail(Vector2 start, int t2_0, int totalAttackCount, float dmg)
    {
        BaseUnit fireBulletAttacker = CreateProjection(FireBulletAttacker_RuntimeAnimatorController, true);

        Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTargetFunc = delegate { return false; };
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };

        fireBulletAttacker.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc); // 不可作为选取的目标
        fireBulletAttacker.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
        fireBulletAttacker.AddCanHitFunc(noHitFunc); // 不可被子弹击中
        fireBulletAttacker.moveRotate = Vector2.left;
        fireBulletAttacker.transform.position = start;
        float maxDist = GetBodyLength();
        float distLeft = GetBodyLength();
        int timeLeft = t2_0;
        int currentAttackCount = 0;
        Vector2 stop = MapManager.GetGridLocalPosition(5, 3);
        
        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            distLeft -= mCurrentMoveSpeed;
            fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
            if (distLeft <= 0)
            {
                fireBulletAttacker.animatorController.Play("Move");
                return true;
            }
            else
            {
                fireBulletAttacker.animatorController.Play("Appear", false, 0.75f * (1 - distLeft / maxDist));
                return false;
            }
        });
        t.AddTaskFunc(delegate {
            // 继续前进
            fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
            if (fireBulletAttacker.transform.position.x < stop.x)
            {
                // 炮台架起，变得可被攻击
                fireBulletAttacker.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                fireBulletAttacker.RemoveCanHitFunc(noHitFunc);
                fireBulletAttacker.animatorController.Play("PreAttack");
                return true;
            }
            return false;
        });
        // 打开炮台
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = t2_0;
                fireBulletAttacker.animatorController.Play("AttackWait0");
                return true;
            }
            return false;
        });
        // 等一会儿
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rot = new Vector2(Mathf.Cos(90 * i * Mathf.PI / 180), Mathf.Sin(90 * i * Mathf.PI / 180));
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
                }
                fireBulletAttacker.animatorController.Play("Attack0");
                return true;
            }
        });
        // 连续攻击
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                currentAttackCount++;
                if (currentAttackCount < totalAttackCount)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos(90 * i * Mathf.PI / 180), Mathf.Sin(90 * i * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
                    }
                    fireBulletAttacker.animatorController.Play("Attack0", false, 0);
                }
                else
                {
                    // 转一下
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("TurnAttack");
                    return true;
                }
            }
            return false;
        });
        // 在转了
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                timeLeft = t2_0;
                fireBulletAttacker.animatorController.Play("AttackWait1");
                return true;
            }
            return false;
        });
        // 再等一会儿
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
            {
                timeLeft--;
                return false;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                    CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
                }
                fireBulletAttacker.animatorController.Play("Attack1");
                return true;
            }
        });
        // 连续攻击
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                currentAttackCount++;
                if (currentAttackCount < totalAttackCount)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rot = new Vector2(Mathf.Cos((45 + 90 * i) * Mathf.PI / 180), Mathf.Sin((45 + 90 * i) * Mathf.PI / 180));
                        CreateFireBullet(fireBulletAttacker, (Vector2)fireBulletAttacker.transform.position + 0.9f * MapManager.gridWidth * rot, rot, dmg);
                    }
                    fireBulletAttacker.animatorController.Play("Attack1", false, 0);
                }
                else
                {
                    // 转一下
                    currentAttackCount = 0;
                    fireBulletAttacker.animatorController.Play("PostAttack");
                    return true;
                }
            }
            return false;
        });
        // 收起来,然后润了
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                fireBulletAttacker.animatorController.Play("Disappear");
                fireBulletAttacker.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTargetFunc);
                fireBulletAttacker.AddCanHitFunc(noHitFunc);
                return true;
            }
            return false;
        });
        // 动画播放完清理掉自己
        t.AddTaskFunc(delegate {
            if (fireBulletAttacker.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                fireBulletAttacker.ExecuteRecycle();
                return true;
            }
            else
            {
                fireBulletAttacker.transform.position += (Vector3)fireBulletAttacker.moveRotate * mCurrentMoveSpeed;
            }
            return false;
        });
        AddTask(t);
        return fireBulletAttacker;
    }

    /// <summary>
    /// 指令-对流激光
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateLaserAttackerAction(float dmg, int wait_time, float laser_hp)
    {
        Action<int, int> action = (timeLeft, totalTime) => {
            if (timeLeft == totalTime - 30)
            {
                LinkLaserAttackers(wait_time, dmg, laser_hp);
            }
        };
        return action;
    }

    /// <summary>
    /// 指令-雾袭
    /// </summary>
    /// <returns></returns>
    private Action<int, int> CreateFogCreatorAction(int waitTime, int type, int shape, int stun_time, int fog0_0, float fog_hp0_0)
    {
        Action<int, int> action = (timeLeft, totalTime) => {
            if (timeLeft == totalTime - 30)
            {
                foreach (var body in GetBodyList())
                {
                    if (body != null && !body.IsHide() && body.transform.position.y >= MapManager.GetRowY(6.5f) && body.transform.position.y <= MapManager.GetRowY(-0.5f))
                    {
                        if(body.transform.position.y > MapManager.GetRowY(2.5f))
                        {
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, type, shape, stun_time, fog0_0);
                        }else if(body.transform.position.y < MapManager.GetRowY(3.5f))
                        {
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, type, shape, stun_time, fog0_0);
                        }
                        else
                        {
                            CreateFogCreator(body, fog_hp0_0, true, waitTime, type, shape, stun_time, fog0_0);
                            CreateFogCreator(body, fog_hp0_0, false, waitTime, type, shape, stun_time, fog0_0);
                        }
                    }
                }
            }
        };
        return action;
    }

    /// <summary>
    /// 产生一个移动相关的技能（给下面的方法用）
    /// </summary>
    /// <param name="pointList">起始点,结束点 对</param>
    /// <param name="waitDist">停靠前需要移动的距离</param>
    /// <param name="wait">停靠时间</param>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility CreateMovementFunc(List<Vector2[]> pointList, float waitDist, int wait, Func<List<Action<int, int>>> createActionListFunc)
    {
        // 变量初始化
        float dist = 0;
        int timeLeft = 0;
        bool flag = false;
        List<Action<int, int>> WaitTaskActionList = null;
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        c.IsMeetSkillConditionFunc = delegate {
            return true;
        };
        c.BeforeSpellFunc = delegate
        {
            // 移速更新
            NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
            moveSpeedModifier.Value = (GetParamValue("speed_rate", mHertIndex) - 1) * 100;
            NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);

            isMoveToDestination = false;
            timeLeft = 0;
            List<Vector2[]> list = new List<Vector2[]>();
            foreach (var p in pointList)
            {
                Vector2 start = p[0];
                Vector2 end = p[1];
                list.Add(new Vector2[] { new Vector2(start.x, start.y), new Vector2(end.x + 0.01f, end.y) });
            }
            dist = waitDist;
            WaitTaskActionList = createActionListFunc();
            flag = false;
            AddRouteListByGridIndex(list);
            SetActionState(new MoveState(this));
        };
        {
            // 是否抵达停靠点
            c.AddSpellingFunc(delegate {
                // if (GetHead().mCurrentActionState is MoveState || (GetHead().mCurrentActionState is TransitionState && GetHead().IsDisappear()))
                if(GetRouteQueue().Count <= 0)
                {
                    if(flag)
                        dist -= GetHead().DeltaPosition.magnitude;
                    else
                        flag = true;
                }
                    
                if (dist <= 0)
                {
                    CancelSetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new IdleState(this));
                    timeLeft = wait;
                    return true;
                }
                return false;
            });
            // 等待
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    foreach (var action in WaitTaskActionList)
                    {
                        action(timeLeft, wait); // 执行指令
                    }
                    timeLeft--;
                    return false;
                }
                else
                {
                    SetAllComponentNoBeSelectedAsTargetAndHited();
                    SetActionState(new MoveState(this));
                    return true;
                }
            });
            // 是否抵达终点
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
    /// 侧路对流攻击式停靠
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement0(Func<List<Action<int, int>>> createActionListFunc)
    {
        float x;
        if (mHertIndex == 0)
            x = 0.1f;
        else if (mHertIndex == 1)
            x = 0.9f;
        else
            x = 1.5f;
        return CreateMovementFunc(
            new List<Vector2[]>() { 
                new Vector2[2]{ new Vector2(8f, 0f), new Vector2(x, 0f) }, 
                new Vector2[2] { new Vector2(0f, 6f), new Vector2(9f+29f, 6f) }
            }, // 起始点与终点
            MapManager.GetColumnX(7) - MapManager.GetColumnX(0) + GetHeadToBodyLength(), // 计算从第七行出现到第一节车厢停靠
            Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
            createActionListFunc // 产生（停靠时执行的指令集合）的方法
            ); 
    }

    /// <summary>
    /// 中线迷雾袭击式停靠
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement1(Func<List<Action<int, int>>> createActionListFunc, int wait_time, int totalAttackCount, float dmg)
    {
        float x;
        if (mHertIndex == 0)
            x = 0;
        else if (mHertIndex == 1)
            x = 0.55f;
        else
            x = 0.9f;

        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            c.ActivateChildAbility(CreateMovementFunc(
                new List<Vector2[]>() {
                    new Vector2[2]{ new Vector2(9-x, 0f), new Vector2(x, 0f) },
                    new Vector2[2] { new Vector2(x, 6f), new Vector2(7.875f-x, 6f) },
                    new Vector2[2] { new Vector2(8f, 3f), new Vector2(0, 3f) },
                }, // 起始点与终点
                MapManager.GetColumnX(8f) - MapManager.GetColumnX(3) + GetHeadToBodyLength(), // 计算使第一节车厢停靠在奇数行所需要走的路程
                Mathf.FloorToInt(GetParamValue("wait0", mHertIndex) * 60), // 获取停靠时间
                createActionListFunc // 产生（停靠时执行的指令集合）的方法
                ));
        };
        {
            c.AddSpellingFunc(delegate
            {
                return true;
            });
            c.AddSpellingFunc(delegate
            {
                isMoveToDestination = false;
                AddRouteListByGridIndex(new List<Vector2[]>() {
                    new Vector2[2] { new Vector2(0f, -4f), new Vector2(26f, -4f) },
                });
                SetActionState(new MoveState(this));
                return true;
            });
            // 三阶段之后会在车尾抵达中线右一列左侧一个车厢距离时再生成一个炮台（视觉上就是多了一节车厢）
            if(mHertIndex >= 2)
            {
                BaseUnit u = null;
                c.AddSpellingFunc(delegate
                {
                    BaseUnit tail = GetLastBody();
                    if (MapManager.GetYIndex(tail.transform.position.y) == 3 && tail.transform.position.x <= MapManager.GetColumnX(8) - GetBodyLength())
                    {
                        u = CreateFireBulletAttackerTail((Vector2)tail.transform.position - moveRotate* GetBodyLength(), wait_time, totalAttackCount, dmg);
                        return true;
                    }
                    return false;
                });
                // 判断炮台是否消失，消失则为该技能结束的标志
                c.AddSpellingFunc(delegate {
                    return u == null || !u.IsAlive();
                });
            }
            c.AddSpellingFunc(delegate
            {
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
    /// 轻便型全息投影停靠
    /// </summary>
    /// <param name="createActionListFunc">产生（停靠期间要执行的指令集合）的方法，其中每个停靠要执行的指令中：第一个int为停靠剩余时间，第二个int为停靠总时间</param>
    /// <returns></returns>
    private CustomizationSkillAbility Movement2(int wait_time, int totalAttackCount, float dmg, float boom_dmg)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        BaseUnit u = null;
        // 实现
        c.IsMeetSkillConditionFunc = delegate {
            // 移速更新
            NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
            moveSpeedModifier.Value = (GetParamValue("speed_rate", mHertIndex) - 1) * 100;
            NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
            return true;
        };
        c.BeforeSpellFunc = delegate{
            u = CreateFireBulletAttackerTrain(wait_time, totalAttackCount, dmg, boom_dmg);
        };
        {
            // 判断炮台是否消失，消失则为该技能结束的标志
            c.AddSpellingFunc(delegate {
                return u==null || !u.IsAlive();
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
}
