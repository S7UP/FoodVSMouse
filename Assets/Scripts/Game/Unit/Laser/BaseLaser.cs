using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 能造成伤害的激光
/// </summary>
public class BaseLaser : MonoBehaviour, IGameControllerMember
{
    // 激光渲染器组件，其Update方法由自身控制
    public LaserRenderer laserRenderer;

    // 激光击中特效的Sprite 或 Runtime
    private Sprite hitSprite;
    private RuntimeAnimatorController HitRuntimeAnimatorController;

    // 以下这些全是激光渲染器的属性
    public bool isOpen { get { return laserRenderer.isOpen; } set { laserRenderer.isOpen = value; } } // 是否打开激光
    public float mMaxLength { get { return laserRenderer.mMaxLength; } set { laserRenderer.mMaxLength = value; } } // 激光的最大长度
    public float mCurrentLength { get { return laserRenderer.mCurrentLength; } set { laserRenderer.mCurrentLength = value; } } // 激光的当前长度
    public float mVelocity { get { return laserRenderer.mVelocity; } set { laserRenderer.mVelocity = value; } } // 激光延伸的速度
    public Vector2 mRotate { get { return laserRenderer.mRotate; } set { laserRenderer.mRotate = value; } } // 激光延伸的方向
    public int mAliveTime { get { return laserRenderer.mAliveTime; }} // 存活时间
    public LayerMask mask;

    // 自己的属性
    public bool isPenetration; // 是否穿透
    public bool isShowHitEffect; // 是否出现击中特效
    public int hitEffectInterval; // 击中特效刷新间隔
    private Dictionary<BaseUnit, int> targetNextHitEffectTimeDict = new Dictionary<BaseUnit, int>();
    public List<BaseUnit> currentHitedUnitList = new List<BaseUnit>(); // 当前帧已碰撞的所有目标
    public string laserHitKey; // 特效名（键值）
    public BaseUnit master;
    public float damage;
    public bool isCollide;
    public TaskController taskController = new TaskController();

    public void Awake()
    {

    }

    public void MInit()
    {
        laserRenderer = null;
        SetCollisionLayer("EnemyLaser");
        mask = LayerMask.GetMask("Ally");
        SetTag("Untagged");

        isPenetration = true; // 默认为穿透
        isShowHitEffect = true; // 默认为显示击中特效
        hitEffectInterval = 16; // 默认16帧刷新一次击中特效
        targetNextHitEffectTimeDict.Clear();
        currentHitedUnitList.Clear();

        laserHitKey = "DefaultLaserHitEffect";

        master = null;
        damage = 10f/60;

        isCollide = true; // 是否开启判定

        taskController.Initial();
    }

    public void MUpdate()
    {
        currentHitedUnitList.Clear(); // 先清空当前帧击中单位表
        laserRenderer.MUpdate();
        // 坐标同步
        laserRenderer.transform.position = transform.position;
        if (isOpen && isCollide)
        {
            if (isPenetration)
            {
                // 穿透情况
                RaycastHit2D[] rsList = Physics2D.RaycastAll(transform.position, mRotate, float.MaxValue, mask);
                foreach (var rs in rsList)
                {
                    float dist = rs.distance;
                    // 产生击中判定的条件是激光长度要大于目标到激光源的距离
                    if (mCurrentLength >= dist)
                    {
                        HandleRaycastHit2DByUpdate(rs);
                    }
                }
            }
            else
            {
                // 非穿透情况
                // 发射一条射线去检测目标
                RaycastHit2D rs = Physics2D.Raycast(transform.position, mRotate, float.MaxValue, mask);
                float dist = rs.distance;
                // 产生击中判定的条件是激光长度要大于目标到激光源的距离
                if (mCurrentLength >= dist)
                {
                    HandleRaycastHit2DByUpdate(rs);
                    // 强制同步激光的长度为距离
                    mCurrentLength = dist;
                }
            }
            // 更新击中特效
            UpdateHitEffectDict();
        }

        taskController.Update();
    }

    public void MPause()
    {
        
    }

    public void MPauseUpdate()
    {
        
    }

    public void MResume()
    {
        
    }

    public void MDestory()
    {
        ExecuteRecycle();
    }

    public void SetOpen(bool enable)
    {
        laserRenderer.SetOpen(enable);
    }

    /// <summary>
    /// 放在Update里处理传进来的RaycastHit2D
    /// </summary>
    private void HandleRaycastHit2DByUpdate(RaycastHit2D rs)
    {
        BaseUnit u;
        rs.collider.TryGetComponent<BaseUnit>(out u);
        if (u != null)
        {
            currentHitedUnitList.Add(u);
            if (!targetNextHitEffectTimeDict.ContainsKey(u))
                targetNextHitEffectTimeDict.Add(u, 0);

            // 处理伤害
            TakeDamage(u);

            // 处理击中特效
            if (targetNextHitEffectTimeDict.ContainsKey(u) && targetNextHitEffectTimeDict[u] <= 0 && !u.IsContainEffect(laserHitKey))
            {
                BaseEffect e = BaseEffect.CreateInstance(HitRuntimeAnimatorController, null, "Idle", null, false);
                u.AddEffectToDict(laserHitKey, e, rs.point - (Vector2)u.transform.position);
                targetNextHitEffectTimeDict[u] += hitEffectInterval;
            }
        }
    }

    /// <summary>
    /// 更新击中特效
    /// </summary>
    private void UpdateHitEffectDict()
    {
        List<BaseUnit> delList = new List<BaseUnit>();
        // 先更新已击中字典的单位，剔除没有的目标
        foreach (var keyValuePair in targetNextHitEffectTimeDict)
        {
            BaseUnit unit = keyValuePair.Key;
            if (!currentHitedUnitList.Contains(unit))
            {
                delList.Add(unit);
            }
        }
        foreach (var unit in delList)
        {
            targetNextHitEffectTimeDict.Remove(unit);
        }
        
        // 剩下的计算击中特效
        if (isShowHitEffect)
        {
            List<int> timeLeftList = new List<int>(targetNextHitEffectTimeDict.Values);
            List<BaseUnit> unitList = new List<BaseUnit>(targetNextHitEffectTimeDict.Keys);

            for (int i = 0; i < unitList.Count; i++)
            {
                BaseUnit unit = unitList[i];
                int timeLeft = timeLeftList[i];
                targetNextHitEffectTimeDict[unit]--;
            }
        }
    }


    public void TakeDamage(BaseUnit unit)
    {
        if (unit != null)
            new DamageAction(CombatAction.ActionType.CauseDamage, master, unit, damage).ApplyAction();
    }

    public void SetCollisionLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public void SetTag(string tag)
    {
        this.tag = tag;
    }

    public void ExecuteRecycle()
    {
        laserRenderer.ExecuteRecycle();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Laser/BaseLaser", gameObject);
    }

    /// <summary>
    /// 添加一个任务
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        taskController.AddTask(t);
    }

    /// <summary>
    /// 移除唯一性任务
    /// </summary>
    public void RemoveUniqueTask(string key)
    {
        taskController.RemoveUniqueTask(key);
    }

    /// <summary>
    /// 移除一个任务
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        taskController.RemoveTask(t);
    }

    /// <summary>
    /// 获取某个标记为key的任务
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ITask GetTask(string key)
    {
        return taskController.GetTask(key);
    }

    public static BaseLaser GetInstance(BaseUnit master, float damage, string layerName, LayerMask mask, Vector2 pos, Vector2 rot, Sprite HeadSprite, Sprite BodySprite, Sprite TailSprite, Sprite HitEffectSprite,
        RuntimeAnimatorController HeadRun, RuntimeAnimatorController BodyRun, RuntimeAnimatorController TailRun, RuntimeAnimatorController HitRun)
    {
        BaseLaser l = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Laser/BaseLaser").GetComponent<BaseLaser>();
        l.MInit();
        l.transform.position = pos;
        l.master = master;
        l.damage = damage;
        l.laserRenderer = LaserRenderer.GetInstance(pos, rot, HeadSprite, BodySprite, TailSprite, HeadRun, BodyRun, TailRun);
        l.SetCollisionLayer(layerName);
        l.mask = mask;
        l.hitSprite = HitEffectSprite;
        l.HitRuntimeAnimatorController = HitRun;
        return l;
    }

    public static BaseLaser GetEnemyInstance(BaseUnit master, float damage, Vector2 pos, Vector2 rot, Sprite HeadSprite, Sprite BodySprite, Sprite TailSprite, Sprite HitEffectSprite,
        RuntimeAnimatorController HeadRun, RuntimeAnimatorController BodyRun, RuntimeAnimatorController TailRun, RuntimeAnimatorController HitRun)
    {
        return BaseLaser.GetInstance(master, damage, "EnemyLaser", LayerMask.GetMask("Ally"), pos, rot, HeadSprite, BodySprite, TailSprite, HitEffectSprite, HeadRun, BodyRun, TailRun, HitRun);
    }

    public static BaseLaser GetAllyInstance(BaseUnit master, float damage, Vector2 pos, Vector2 rot, Sprite HeadSprite, Sprite BodySprite, Sprite TailSprite, Sprite HitEffectSprite,
    RuntimeAnimatorController HeadRun, RuntimeAnimatorController BodyRun, RuntimeAnimatorController TailRun, RuntimeAnimatorController HitRun)
    {
        return BaseLaser.GetInstance(master, damage, "AllyLaser", LayerMask.GetMask("Enemy"), pos, rot, HeadSprite, BodySprite, TailSprite, HitEffectSprite, HeadRun, BodyRun, TailRun, HitRun);
    }
}
