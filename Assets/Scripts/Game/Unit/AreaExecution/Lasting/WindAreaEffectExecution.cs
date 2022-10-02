using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 风域
/// </summary>
public class WindAreaEffectExecution : RetangleAreaEffectExecution
{
    private const string TaskName = "WindTask"; // 专属的任务名
    public float velocity; // 风速，默认方向是向右的，负数则表明向左

    // 自身所覆盖云层组
    private List<RetangleAreaEffectExecution> cloudGroupList = new List<RetangleAreaEffectExecution>();

    public override void Awake()
    {
        base.Awake();
    }

    public override void MInit()
    {
        velocity = 0;
        base.MInit();
    }

    public override void MUpdate()
    {
        // 更新云组的位置 
        List<RetangleAreaEffectExecution> delList = new List<RetangleAreaEffectExecution>();
        foreach (var e in cloudGroupList)
        {
            if(!e.isActiveAndEnabled)
                delList.Add(e);
        }
        foreach (var e in delList)
        {
            cloudGroupList.Remove(e);
        }
        foreach (var e in cloudGroupList)
        {
            e.FloatDict["offsetX"] += velocity;
        }
        base.MUpdate();
    }

    public override void OnCollision(Collider2D collision)
    {
        RetangleAreaEffectExecution e;
        if (collision.TryGetComponent<RetangleAreaEffectExecution>(out e))
        {
            if (!cloudGroupList.Contains(e) && e.FloatDict.ContainsKey("offsetX"))
            {
                Debug.Log("cloudGroup!");
                cloudGroupList.Add(e);
            }
                
        }
        else
        {
            base.OnCollision(collision);
        }
    }

    public override bool IsMeetingCondition(BaseUnit unit)
    {
        // BOSS单位无视地形效果
        if (unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        return base.IsMeetingCondition(unit);
    }

    public override void OnExit(Collider2D collision)
    {
        RetangleAreaEffectExecution e;
        if (collision.TryGetComponent<RetangleAreaEffectExecution>(out e))
        {
            if (cloudGroupList.Contains(e))
                cloudGroupList.Remove(e);
        }
        else
        {
            base.OnExit(collision);
        }
    }

    public override void OnEnemyEnter(MouseUnit unit)
    {
        // 获取目标身上唯一的风任务
        WindTask t = null;
        if (unit.GetTask(TaskName) == null)
        {
            t = new WindTask(unit, this);
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as WindTask;
            t.Add(this);
        }
        base.OnEnemyEnter(unit);
    }

    public override void OnEnemyExit(MouseUnit unit)
    {
        // 获取目标身上唯一的风任务
        if (unit.GetTask(TaskName) == null)
        {
            Debug.LogWarning("目标在没有风域任务的情况下就退出了风域！");
        }
        else
        {
            WindTask t = unit.GetTask(TaskName) as WindTask;
            t.Remove(this);
        }
        base.OnEnemyExit(unit);
    }

    public float GetVelocity()
    {
        return velocity;
    }

    public void SetVelocity(float v)
    {
        velocity = v;
    }

    public static WindAreaEffectExecution GetInstance(int col, int row, Vector2 position)
    {
        WindAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/WindAreaEffect").GetComponent<WindAreaEffectExecution>();
        e.MInit();
        e.Init(MapManager.GetYIndex(position.y), col, row, 0, 0, false, true);
        e.SetBoxCollider2D(Vector2.zero, new Vector2(col * MapManager.gridWidth, row * MapManager.gridHeight));
        e.isAffectMouse = true;
        e.transform.position = position;
        return e;
    }

    /// <summary>
    /// 设置风域为经典运作模式
    /// </summary>
    /// <param name="e">受影响的风狱</param>
    /// <param name="StayTime">静止等待时间</param>
    /// <param name="ChangeTime">加减速时间</param>
    /// <param name="UniformTime">匀速时间</param>
    /// <param name="isLeftStart">是否从左向移动开始</param>
    public static void SetClassicalWindAreaEffectMode(WindAreaEffectExecution e, int startState, int StayTime, int ChangeTime, int UniformTime, bool isLeftStart)
    {
        // 风域方向变化周期
        int state = 0; // 阶段 0等待，1加速 2匀速 3减速
        int rotate_x = -1;
        int timeLeft = 0;
        GameController.Instance.AddTasker(
            //Action InitAction, 
            delegate
            {
                timeLeft = StayTime;
                state = startState;
            },
            //Action UpdateAction, 
            delegate
            {
                if (timeLeft > 0)
                    timeLeft--;
                else
                {
                    state++;
                    if (state == 4)
                    {
                        rotate_x = -rotate_x;
                        state = 0;
                    }

                    // 下一个阶段经过的时间
                    switch (state)
                    {
                        case 0:
                            timeLeft = StayTime;
                            break;
                        case 1:
                            timeLeft = ChangeTime;
                            break;
                        case 2:
                            timeLeft = UniformTime;
                            break;
                        case 3:
                            timeLeft = ChangeTime;
                            break;
                        default:
                            // 虽然走正常逻辑不会遇到Default情况
                            state = 0;
                            timeLeft = StayTime;
                            break;
                    }
                }

                // 以下，根据情况做事
                switch (state)
                {
                    // 加速
                    case 1:
                        {
                            float r = 1 - (float)timeLeft / ChangeTime;
                            e.SetVelocity(TransManager.TranToVelocity(1f) * r * rotate_x);
                        }
                        break;
                    // 减速
                    case 3:
                        {
                            float r = (float)timeLeft / ChangeTime;
                            e.SetVelocity(TransManager.TranToVelocity(1f) * r * rotate_x);
                        }
                        break;
                    default: break;
                }
            },
            //Func<bool> EndCondition, 
            delegate
            {
                return false;
            },
            //Action EndEvent
            delegate
            {

            }
            );
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/WindAreaEffect", gameObject);
    }


    // 风域任务
    private class WindTask : ITask
    {
        private static FloatModifier AddDamageRate = new FloatModifier(1.5f); // 顺风，受伤增幅
        private static FloatModifier DecDamageRate = new FloatModifier(0.5f); // 逆风，减伤

        private List<WindAreaEffectExecution> list = new List<WindAreaEffectExecution>(); // 进入的风域
        private BaseUnit unit;
        private bool isWind; // 当前是否受风
        private bool isDownWind; // 当前是否顺风（目标移动方向与风向一致）

        public WindTask(BaseUnit unit, WindAreaEffectExecution w)
        {
            this.unit = unit;
            list.Add(w);
        }

        public void OnEnter()
        {
            isWind = false; // 默认不受风
            isDownWind = false; // 默认为逆风
        }

        public void OnUpdate()
        {
            // 获取当前所受风速和
            float velocity = 0;
            foreach (var w in list)
            {
                velocity += w.GetVelocity();
            }
            //unit.SetPosition(unit.GetPosition() +  new Vector3(velocity, 0));
            unit.transform.position = unit.transform.position + new Vector3(velocity, 0, 0);
            //unit.SetPosition((Vector2)unit.GetPosition() + new Vector2(velocity, 0));
            // 检测状态是否发生变化及后续处理

            if (isWind) 
            {
                // 如果受风的情况下

                if (velocity == 0)
                {
                    // 风速降为0则转为不受风
                    unit.NumericBox.DamageRate.RemoveModifier(AddDamageRate);
                    unit.NumericBox.DamageRate.RemoveModifier(DecDamageRate);
                    isWind = false;
                }
                else if (!isDownWind && unit.moveRotate.x * velocity >= 0)
                {
                    // 逆风时 检测目标朝向与风向是否同向，如果是则转为顺风
                    ChangeToDownWindModifier();
                    isDownWind = true;
                }
                else if (isDownWind && unit.moveRotate.x * velocity < 0)
                {
                    // 顺风时 检测目标朝向与风向是否反向，如果是则转为逆风
                    ChangeToUpWindModifier();
                    isDownWind = false;
                }
            }
            else
            {
                // 当不受风时
                if(velocity != 0)
                {
                    // 有了风速，要改为受风
                    isWind = true;
                    // 判断是顺风还是逆风
                    if(unit.moveRotate.x * velocity >= 0)
                    {
                        ChangeToDownWindModifier();
                        isDownWind = true;
                    }
                    else
                    {
                        ChangeToUpWindModifier();
                        isDownWind = false;
                    }
                }
            }
        }

        public bool IsMeetingExitCondition()
        {
            return list.Count <= 0;
        }

        public void OnExit()
        {
            unit.NumericBox.DamageRate.RemoveModifier(AddDamageRate);
            unit.NumericBox.DamageRate.RemoveModifier(DecDamageRate);
        }

        public void Add(WindAreaEffectExecution w)
        {
            if(!list.Contains(w))
                list.Add(w);
        }

        public void Remove(WindAreaEffectExecution w)
        {
            list.Remove(w);
        }

        /// <summary>
        /// 切换为顺风标签
        /// </summary>
        private void ChangeToDownWindModifier()
        {
            unit.NumericBox.DamageRate.RemoveModifier(AddDamageRate);
            unit.NumericBox.DamageRate.RemoveModifier(DecDamageRate);

            unit.NumericBox.DamageRate.AddModifier(AddDamageRate);
        }

        /// <summary>
        /// 切换为逆风标签
        /// </summary>
        private void ChangeToUpWindModifier()
        {
            unit.NumericBox.DamageRate.RemoveModifier(AddDamageRate);
            unit.NumericBox.DamageRate.RemoveModifier(DecDamageRate);

            unit.NumericBox.DamageRate.AddModifier(DecDamageRate);
        }
    }
}
