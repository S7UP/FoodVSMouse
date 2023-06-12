using System.Collections.Generic;
using UnityEngine;
using System;
using S7P.Numeric;
public class DongJun : BossUnit
{
    private static RuntimeAnimatorController Cave_Road_AnimatorController;
    private static RuntimeAnimatorController Cave_Water_AnimatorController;
    private static RuntimeAnimatorController Pipeline_Road_AnimatorController;
    private static RuntimeAnimatorController Pipeline_Water_AnimatorController;

    private const string PipelineKey = "管道";

    public override void Awake()
    {
        if (Cave_Road_AnimatorController == null)
        {
            Cave_Road_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/0/Cave_Road");
            Cave_Water_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/0/Cave_Water");
            Pipeline_Road_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/0/Pipeline_Road");
            Pipeline_Water_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/0/Pipeline_Water");
        }
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();

        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        // 添加出现的技能
        {
            CompoundSkillAbility c = new CompoundSkillAbility(this);
            int timeLeft = 120;
            c.IsMeetSkillConditionFunc = delegate { return true; };
            c.BeforeSpellFunc = delegate
            {
                animatorController.Play("Appear");
            };
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Stun", true);
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                else
                {
                    return true;
                }
            });
            // 强制设制当前技能为这个
            mSkillQueueAbilityManager.SetCurrentSkill(c);
        }

    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.DongJun, 0))
        {
            AddParamArray(keyValuePair.Key, keyValuePair.Value);
        }
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();

        list.Add(BuildPipelinesInit(infoList[0])); // 修筑管道
        list.Add(StampedeAccidentsInit(infoList[1])); // 踩踏事故

        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }


    /// <summary>
    /// 产生一个无底洞
    /// </summary>
    /// <param name="g">产生洞的格子</param>
    /// <param name="timeLeft">洞的存在时间</param>
    private void CreateCave(BaseGrid g, int timeLeft)
    {
        // 如果传入的格子无效 或者 格子已包含高空地块 则不会发生任何事
        if (g == null || g.IsContainGridType(GridType.Sky))
            return;

        // 添加高空地块
        BaseGridType gt = BaseGridType.GetInstance(GridType.Sky, 0);
        g.AddGridType(GridType.Sky, gt);
        // 禁止放卡
        Func<BaseGrid, FoodInGridType, bool> noBuildFunc = delegate { return false; };
        g.AddCanBuildFuncListener(noBuildFunc);
        // 可以试着强制移除上面的载具
        g.KillFoodUnit(FoodInGridType.WaterVehicle);
        g.KillFoodUnit(FoodInGridType.LavaVehicle);

        // 添加若干时间后移除的任务
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            // 洞出现
            if(g.IsContainGridType(GridType.Water))
                gt.animator.runtimeAnimatorController = Cave_Water_AnimatorController;
            else
                gt.animator.runtimeAnimatorController = Cave_Road_AnimatorController;
            gt.animatorController.Play("Appear");
        });
        t.AddTaskFunc(delegate
        {
            timeLeft--;
            if (gt.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                gt.animatorController.Play("Idle", true);
                return true;
            }
            return false;
        });
        
        t.AddTaskFunc(delegate {
            if (timeLeft > 0)
                timeLeft--;
            else
            {
                gt.animatorController.Play("Disappear");
                return true;
            }
            return false;
        });

        t.AddTaskFunc(delegate
        {
            if (gt.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                g.RemoveCanBuildFuncListener(noBuildFunc);
                g.RemoveGridType(GridType.Sky);
                return true;
            }
            return false;
        });
        g.AddTask(t);
    }

    /// <summary>
    /// 创建一个管道
    /// </summary>
    /// <param name="g"></param>
    private MouseModel CreatePipeline(BaseGrid g, float hp)
    {
        if (g == null)
            return null;

        if (g.GetUnitFromDict(PipelineKey) != null)
        {
            g.GetUnitFromDict(PipelineKey).ExecuteDeath();
            g.RemoveUnitFromDict(PipelineKey);
        }

        MouseModel m;
        if (g.IsContainGridType(GridType.Water))
            m = MouseModel.GetInstance(Pipeline_Water_AnimatorController);
        else
            m = MouseModel.GetInstance(Pipeline_Road_AnimatorController);
        BoolModifier boolModifier = new BoolModifier(true);
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier); // 免疫冻结
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, boolModifier); // 免疫晕眩
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, boolModifier); // 免疫冰冻减速
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreDropFromSky, boolModifier); // 免疫坠落
        m.NumericBox.AddDecideModifierToBoolDict(PipelineKey, boolModifier); // 标记自己是洞，不能被别的洞传送
        WaterGridType.AddNoAffectByWater(m, boolModifier); // 标记免疫水蚀
        m.SetBaseAttribute(hp, 0, 1, 0, 100, 0, 0);
        m.transform.position = g.transform.position;
        m.currentYIndex = MapManager.GetYIndex(g.transform.position.y);
        m.CanBlockFuncList.Add(delegate { return false; }); // 管道不可阻挡所以也不会攻击
        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        m.CanHitFuncList.Add(noHitFunc); // 管道也不会被子弹击中（转移子弹是用范围效果，下述）
        m.CanBeSelectedAsTargetFuncList.Add(delegate { return false; }); // 不可作为目标被选取
        // 禁止放卡
        Func<BaseGrid, FoodInGridType, bool> noBuildFunc = delegate { return false; };
        g.AddCanBuildFuncListener(noBuildFunc);
        GameController.Instance.AddMouseUnit(m);
        g.AddUnitToDict(PipelineKey, m); // 绑定在格子上

        // 添加动画
        {
            CustomizationTask t = new CustomizationTask();
            t.AddOnEnterAction(delegate {
                m.SetActionState(new IdleState(m));
                m.animatorController.Play("Appear");
            });
            t.AddTaskFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    m.RemoveCanHitFunc(noHitFunc);
                    animatorController.Play("Idle", true);
                    return true;
                }
                return false;
            });
        }
        // 当管道被摧毁时会留下无底洞并移除因为管道造成的不可放置
        m.AddBeforeDeathEvent(delegate {
            g.RemoveCanBuildFuncListener(noBuildFunc);
            CreateCave(g, Mathf.FloorToInt(GetParamValue("CaveAliveTime", mHertIndex) * 60));
        });
        return m;
    }

    /// <summary>
    /// 连接两个管道
    /// </summary>
    /// <param name="p1">朝左的管道</param>
    /// <param name="p2">朝右的管道</param>
    private void LinkDoublePipeline(MouseModel p1, MouseModel p2)
    {
        if(p1 == null || p2 == null)
        {
            if (p1 != null)
                p1.ExecuteDeath();
            if (p2 != null)
                p2.ExecuteDeath();
            return;
        }

        RetangleAreaEffectExecution r1 = null;
        RetangleAreaEffectExecution r1_bullet = null;
        RetangleAreaEffectExecution r2 = null;
        RetangleAreaEffectExecution r2_bullet = null;

        // 为左向管道添加转移老鼠的区域 并将区域与 管道绑定
        {
            Vector3 offset = 0*Vector3.left * 0.375f * MapManager.gridWidth; // 跟随偏移量
            r1 = RetangleAreaEffectExecution.GetInstance(p1.transform.position + offset, 0.25f, 1, "ItemCollideEnemy");
            r1.isAffectMouse = true;
            // 转移敌怪
            r1.SetOnEnemyEnterAction((m) => {
                // BOSS与管道本身不能通过
                if (m.IsBoss() || m.NumericBox.GetBoolNumericValue(PipelineKey))
                    return;

                if (m.moveRotate.x > 0)
                {
                    m.transform.position = r2.transform.position;
                }
            });
            r1.AddExcludeMouseUnit(p1); // 不能把自己传走呀
            r1.SetAffectHeight(0);
            GameController.Instance.AddAreaEffectExecution(r1);

            // 添加绑定任务
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate{
                if (p1.IsAlive())
                {
                    r1.transform.position = p1.transform.position + offset;
                    return false;
                }
                else
                {
                    r1.MDestory();
                    return true;
                }
            });
            r1.AddTask(t);
        }
        // 为左向管道添加转移子弹的区域 并将区域与 管道绑定
        {
            Vector3 offset = 0 * Vector3.left * 0.375f * MapManager.gridWidth; // 跟随偏移量
            r1_bullet = RetangleAreaEffectExecution.GetInstance(p1.transform.position + offset, 0.25f, 1, "Enemy");
            r1_bullet.isAffectBullet = true;
            // 转移子弹
            r1_bullet.SetOnBulletEnterAction((b) => {
                if (b.GetRotate().x > 0)
                {
                    b.transform.position = p2.transform.position;
                    r1_bullet.AddExcludeBullet(b); // 不再二次传送这个子弹
                }
            });
            r1_bullet.SetAffectHeight(0);
            GameController.Instance.AddAreaEffectExecution(r1_bullet);

            // 添加绑定任务
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (p1.IsAlive())
                {
                    r1_bullet.transform.position = p1.transform.position + offset;
                    return false;
                }
                else
                {
                    r1_bullet.MDestory();
                    return true;
                }
            });
            r1_bullet.AddTask(t);
        }



        // 为右向管道添加转移老鼠的区域 并将区域与 管道绑定
        {
            Vector3 offset = 0*Vector3.right * 0.375f * MapManager.gridWidth; // 跟随偏移量
            r2 = RetangleAreaEffectExecution.GetInstance(p2.transform.position + offset, 0.25f, 1, "ItemCollideEnemy");
            r2.isAffectMouse = true;
            r2.isAffectBullet = true;
            // 转移子弹
            r2.SetOnBulletEnterAction((b) => {
                if (b.GetRotate().x < 0)
                {
                    b.transform.position = r1.transform.position;
                }
            });
            // 转移敌怪
            r2.SetOnEnemyEnterAction((m) => {
                // BOSS与管道本身不能通过
                if (m.IsBoss() || m.NumericBox.GetBoolNumericValue(PipelineKey))
                    return;

                if (m.moveRotate.x < 0)
                {
                    m.transform.position = r1.transform.position;
                }
            });
            r2.AddExcludeMouseUnit(p2); // 不能把自己传走呀
            r2.SetAffectHeight(0);
            GameController.Instance.AddAreaEffectExecution(r2);

            // 添加绑定任务
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (p2.IsAlive())
                {
                    r2.transform.position = p2.transform.position + offset;
                    return false;
                }
                else
                {
                    r2.MDestory();
                    return true;
                }
            });
            r2.AddTask(t);
        }
        // 为右向管道添加转移子弹的区域 并将区域与 管道绑定
        {
            Vector3 offset = 0 * Vector3.right * 0.375f * MapManager.gridWidth; // 跟随偏移量
            r2_bullet = RetangleAreaEffectExecution.GetInstance(p2.transform.position + offset, 0.25f, 1, "Enemy");
            r2_bullet.isAffectBullet = true;
            // 转移子弹
            r2_bullet.SetOnBulletEnterAction((b) => {
                if (b.GetRotate().x < 0)
                {
                    b.transform.position = p1.transform.position;
                    r2_bullet.AddExcludeBullet(b); // 不再二次传送这个子弹
                }
            });
            r2_bullet.SetAffectHeight(0);
            GameController.Instance.AddAreaEffectExecution(r2_bullet);

            // 添加绑定任务
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if (p2.IsAlive())
                {
                    r2_bullet.transform.position = p2.transform.position + offset;
                    return false;
                }
                else
                {
                    r2_bullet.MDestory();
                    return true;
                }
            });
            r2_bullet.AddTask(t);
        }

        // 对两管施加死亡绑定（一个死了会连带另一个也死）
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if(!p1.IsAlive())
                {
                    p2.ExecuteDeath();
                    return true;
                }else if (!p2.IsAlive())
                {
                    p1.ExecuteDeath();
                    return true;
                }
                return false;
            });
            p1.AddTask(t);
            p2.AddTask(t);
        }
    }

    ////////////////////////////////////////////////////////////////以下为BOSS的技能定义////////////////////////////////////////////////////////////

    /// <summary>
    /// 挖地移动到某地
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility DigMove(Vector2 pos, int timeLeft, int stunTime)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            // 原地挖土消失
            animatorController.Play("Disappear");
        };
        {
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 原地留下一个洞
                    CreateCave(GameController.Instance.mMapController.GetGrid(GetColumnIndex(), GetRowIndex()), timeLeft);
                    transform.position = pos;
                    animatorController.Play("Appear");
                    return true;
                }
                return false;
            });

            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Stun", true);
                    return true;
                }
                return false;
            });

            c.AddSpellingFunc(delegate {
                if (stunTime > 0)
                {
                    stunTime--;
                    return false;
                }
                else
                    return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 修筑管道
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility BuildPipelinesInit(SkillAbility.SkillAbilityInfo info)
    {
        int CaveAliveTime = Mathf.FloorToInt(GetParamValue("CaveAliveTime", mHertIndex)*60); // 洞的存在时间
        float Pipeline_Hp = GetParamValue("Pipeline_Hp", mHertIndex); // 管道的生命值

        // 修筑管道
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex)*60); // 钻土出来后的晕眩时间
        int t0_1 = Mathf.FloorToInt(GetParamValue("t0_1", mHertIndex)*60); // 2钻土出来后的晕眩时间
        int num0_0 = Mathf.FloorToInt(GetParamValue("num0_0", mHertIndex)); // 跳跃次数

        // 参数
        BaseGrid selectedGrid1 = null; // 被选中打洞的格子
        BaseGrid selectedGrid2 = null;

        MouseModel pipeline_right = null;
        MouseModel pipeline_left = null;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            // 初始化
            selectedGrid1 = null;
            selectedGrid2 = null;
            pipeline_right = null;
            pipeline_left = null;

            // 从右二列中取出所有有效格子，然后随机在一个有效格子后出现
            List<BaseGrid> gridList = new List<BaseGrid>();
            for (int row = 0; row < 7; row++)
            {
                BaseGrid g = GameController.Instance.mMapController.GetGrid(7, row);
                if(g!=null)
                    gridList.Add(g);
            }
            Vector2 pos;
            // 之后开始取BOSS的随机数
            if(gridList.Count > 0)
            {
                int selectedIndex = GetRandomNext(0, gridList.Count);
                selectedGrid1 = gridList[selectedIndex];
                pos = new Vector2(selectedGrid1.transform.position.x + 1f*MapManager.gridWidth, selectedGrid1.transform.position.y);
            }
            else
            {
                // 真要是没有的话就七条线随机取一格吧
                int selectedRow = GetRandomNext(0, 7);
                pos = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectedRow));
            }
            // 原地挖土消失
            c.ActivateChildAbility(DigMove(pos, CaveAliveTime, t0_0));
        };
        {
            // 出土晕眩完后的事——打洞！
            c.AddSpellingFunc(delegate
            {
                animatorController.Play("Dig", false, 0);
                return true;
            });

            // 动画播放完一次后打出洞，然后再播一次
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 在上次被选取的格子中生成洞
                    CreateCave(selectedGrid1, CaveAliveTime);
                    animatorController.Play("Dig", false, 0);
                    return true;
                }
                return false;
            });

            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    SmallJump(MapManager.gridWidth);
                    return true;
                }
                return false;
            });

            // 打完洞就钻进去
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    int col = 6;
                    // 找右四列有卡片的格子
                    List<BaseGrid> gridList = new List<BaseGrid>();
                    for (int row = 0; row < 7; row++)
                    {
                        BaseGrid g = GameController.Instance.mMapController.GetGrid(col, row);
                        if (g != null && g.GetAttackableFoodUnitList().Count > 0)
                        {
                            gridList.Add(g);
                        }   
                    }
                    // 如果找不到这种格子，那就再找有效的格子
                    if(gridList.Count <= 0)
                    {
                        for (int row = 0; row < 7; row++)
                        {
                            BaseGrid g = GameController.Instance.mMapController.GetGrid(col, row);
                            if (g != null)
                            {
                                gridList.Add(g);
                            }
                        }
                    }

                    Vector2 pos;
                    // 随机取一个格子
                    if (gridList.Count > 0)
                    {
                        int selectedIndex = GetRandomNext(0, gridList.Count);
                        selectedGrid2 = gridList[selectedIndex];
                        pos = selectedGrid2.transform.position + MapManager.gridWidth*Vector3.left;
                    }
                    else
                    {
                        // 真要是没有的话就七条线随机取一格吧
                        int selectedRow = GetRandomNext(0, 7);
                        pos = new Vector2(MapManager.GetColumnX(5f), MapManager.GetRowY(selectedRow));
                    }
                    // 原地挖土消失
                    c.ActivateChildAbility(DigMove(pos, CaveAliveTime, t0_1));
                    return true;
                }
                return false;
            });
            // 出现后自身反向，然后再播放打洞动画
            c.AddSpellingFunc(delegate
            {
                transform.localScale = new Vector2(-1, 1);
                animatorController.Play("Dig", false, 0);
                return true;
            });
            // 第一次播放完在最早选定的格子生成管道A，右朝向
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    pipeline_right = CreatePipeline(selectedGrid1, Pipeline_Hp);
                    animatorController.Play("Dig", false, 0);
                    return true;
                }
                return false;
            });

            // 第二次播放完打一个洞出来
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 再生成一个洞
                    CreateCave(selectedGrid2, CaveAliveTime);
                    animatorController.Play("Dig", false, 0);
                    return true;
                }
                return false;
            });

            // 第三次播放完生成管道B，左朝向，然后使两个管道相连接
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    pipeline_left = CreatePipeline(selectedGrid2, Pipeline_Hp);
                    if (pipeline_left != null)
                        pipeline_left.transform.localScale = new Vector2(-1, 1);
                    LinkDoublePipeline(pipeline_left, pipeline_right);
                    transform.localScale = new Vector2(1, 1); // 方向恢复回来
                    return true;
                }
                return false;
            });

            // 准备起跳
            if(num0_0 > 0)
            {
                c.AddSpellingFunc(delegate
                {
                    SmallJump(2 * MapManager.gridWidth);
                    return true;
                });
            }
            // 几次小跳判定
            for (int i = 0; i < num0_0 - 1; i++)
            {
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        SmallJump(2 * MapManager.gridWidth);
                        // 造成一次单格的踩踏伤害
                        DamageCurrentPosition();
                        return true;
                    }
                    return false;
                });
            }

            // 最后一次小跳结束
            if (num0_0 > 0)
            {
                c.AddSpellingFunc(delegate
                {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // 造成一次单格的踩踏伤害
                        DamageCurrentPosition();
                        return true;
                    }
                    return false;
                });
            }

        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// 小跳
    /// </summary>
    private void SmallJump(float Dist)
    {
        int JumpClipTime = Mathf.FloorToInt(animatorController.GetAnimatorStateRecorder("Jump").aniTime) - 10;
        animatorController.Play("Jump", false, 0);
        AddTask(TaskManager.GetParabolaTask(this, Dist / JumpClipTime, 0.5f, transform.position, transform.position + Dist * Vector3.left, false));
    }

    /// <summary>
    /// 大跳
    /// </summary>
    private void BigJump(float Dist)
    {
        int JumpClipTime = Mathf.FloorToInt(animatorController.GetAnimatorStateRecorder("Jump").aniTime) - 10;
        animatorController.Play("Jump", false, 0);
        AddTask(TaskManager.GetParabolaTask(this, Dist / JumpClipTime, 3f, transform.position, transform.position + Dist * Vector3.left, false));
    }

    /// <summary>
    /// 对当前位置造成一格踩踏伤害
    /// </summary>
    private void DamageCurrentPosition()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.5f, 0.5f, "EnemyAllyGrid");
        r.SetInstantaneous();
        r.isAffectMouse = true;
        r.SetOnEnemyEnterAction((u)=>{
            if (u.IsBoss())
                return;
            UnitManager.Execute(this, u);
        });

        r.isAffectGrid = true;
        r.SetOnGridEnterAction((g) => {
            g.TakeAction(this, (u) => { 
                DamageAction action = UnitManager.Execute(this, u);
                new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * GetParamValue("dmg_trans")/100).ApplyAction();
            }, false);
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    /// <summary>
    /// 击晕当前范围内单位
    /// </summary>
    /// <param name="stunTime"></param>
    private void StunCurrentpostion(int stunTime)
    {
        Action<BaseUnit> stunAction = (u) => {
            u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stunTime, false));
        };

        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 3, 3, "BothCollide");
        r.SetInstantaneous();
        r.isAffectFood = true;
        r.isAffectMouse = true;
        r.isAffectCharacter = true;
        r.SetOnFoodEnterAction(stunAction);
        r.SetOnEnemyEnterAction(stunAction);
        r.AddExcludeMouseUnit(this); // 自身被排除在外
        GameController.Instance.AddAreaEffectExecution(r);
    }



    /// <summary>
    /// 踩踏事故
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility StampedeAccidentsInit(SkillAbility.SkillAbilityInfo info)
    {
        int CaveAliveTime = Mathf.FloorToInt(GetParamValue("CaveAliveTime", mHertIndex)*60); // 洞的存在时间
        float Pipeline_Hp = GetParamValue("Pipeline_Hp", mHertIndex); // 管道的生命值

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60); // 挖洞后自身晕眩时间
        int num1_0 = Mathf.FloorToInt(GetParamValue("num1_0", mHertIndex)); // 跳跃次数
        int num1_1 = Mathf.FloorToInt(GetParamValue("num1_1", mHertIndex)); // 大跳次数
        int StunTime1_0 = Mathf.FloorToInt(GetParamValue("StunTime1_0", mHertIndex) * 60); // 大跳对卡片的晕眩时间

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {

            // 从右一列中取出所有有效格子，然后随机在一个有效格子后出现
            List<BaseGrid> gridList = new List<BaseGrid>();
            for (int row = 0; row < 7; row++)
            {
                BaseGrid g = GameController.Instance.mMapController.GetGrid(8, row);
                if (g != null)
                    gridList.Add(g);
            }
            Vector2 pos;
            // 之后开始取BOSS的随机数
            if (gridList.Count > 0)
            {
                int selectedIndex = GetRandomNext(0, gridList.Count);
                BaseGrid g = gridList[selectedIndex];
                pos = new Vector2(g.transform.position.x, g.transform.position.y);
            }
            else
            {
                // 真要是没有的话就七条线随机取一格吧
                int selectedRow = GetRandomNext(0, 7);
                pos = new Vector2(MapManager.GetColumnX(8f), MapManager.GetRowY(selectedRow));
            }
            // 原地挖土消失
            c.ActivateChildAbility(DigMove(pos, CaveAliveTime, t1_0));
        };
        {
            // 准备起跳
            c.AddSpellingFunc(delegate
            {
                SmallJump(2 * MapManager.gridWidth);
                return true;
            });
            // 几次小跳判定
            for (int i = 0; i < num1_0 - 1; i++)
            {
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        SmallJump(2 * MapManager.gridWidth);
                        // 造成一次单格的踩踏伤害
                        DamageCurrentPosition();
                        return true;
                    }
                    return false;
                });
            }

            // 最后一次小跳结束
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 造成一次单格的踩踏伤害
                    DamageCurrentPosition();
                    // 如果有大跳的话，就准备进行大跳
                    if(num1_1 > 0)
                    {
                        BigJump(2 * MapManager.gridWidth);
                    }
                    
                    return true;
                }
                return false;
            });
            // 几次大跳判定
            for (int i = 0; i < num1_1 - 1; i++)
            {
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        BigJump(2 * MapManager.gridWidth);
                        // 造成一次单格的踩踏伤害
                        DamageCurrentPosition();
                        // 造成一次3*3的晕眩效果
                        StunCurrentpostion(StunTime1_0);
                        return true;
                    }
                    return false;
                });
            }
            // 最后一次跳结束
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 最后一次大跳结束（如果是）
                    if (num1_1 > 0)
                    {
                        // 造成一次单格的踩踏伤害
                        DamageCurrentPosition();
                        // 造成一次3*3的晕眩效果
                        StunCurrentpostion(StunTime1_0);
                    }
                    return true;
                }
                return false;
            });

        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
}