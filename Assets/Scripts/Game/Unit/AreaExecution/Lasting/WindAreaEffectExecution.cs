using System.Collections.Generic;
using UnityEngine;
using S7P.Numeric;
/// <summary>
/// 风域
/// </summary>
public class WindAreaEffectExecution : RetangleAreaEffectExecution
{
    public class State
    {
        public int totoalTime = 0;
        public float start_v = 0;
        public float end_v = 0;
    }

    public List<State> stateList = new List<State>();
    public int currentStateIndex = 0;
    public int currentTimeLeft = 0;

    private const string TaskName = "WindTask"; // 专属的任务名

    public const string IgnoreWind = "IgnoreWind"; // 无视风场的词条
    public float velocity; // 风速，默认方向是向右的，负数则表明向左
    public float add_dmg_rate = 1;
    public float dec_dmg_rate = 1;

    // 自身所覆盖云层组
    private List<RetangleAreaEffectExecution> cloudGroupList = new List<RetangleAreaEffectExecution>();


    public override void MInit()
    {
        stateList.Clear();
        currentStateIndex = -1;
        currentTimeLeft = 0;
        velocity = 0;
        base.MInit();

        AddEnemyEnterConditionFunc((m) => {
            if (m.IsBoss() || m.NumericBox.GetBoolNumericValue(IgnoreWind) || !MouseManager.IsGeneralMouse(m))
                return false;
            return true;
        });

        SetOnEnemyEnterAction((u) => {
            // 获取目标身上唯一的风任务
            WindTask t = null;
            if (u.GetTask(TaskName) == null)
            {
                t = new WindTask(u, this, add_dmg_rate, dec_dmg_rate);
                u.AddUniqueTask(TaskName, t);
            }
            else
            {
                t = u.GetTask(TaskName) as WindTask;
                t.Add(this);
            }
        });

        SetOnEnemyExitAction((u) => {
            // 获取目标身上唯一的风任务
            if (u.GetTask(TaskName) != null)
            {
                WindTask t = u.GetTask(TaskName) as WindTask;
                t.Remove(this);
            }
        });
    }

    public override void MUpdate()
    {
        currentTimeLeft--;
        // 更新速度
        {
            State s = GetCurrentState();
            if (s != null)
                velocity = Mathf.Lerp(s.start_v, s.end_v, 1 - (float)currentTimeLeft / s.totoalTime);
            //else
            //    velocity = 0;
        }
        if (currentTimeLeft <= 0)
        {
            // 进入下一阶段
            currentStateIndex++;
            if (stateList.Count == 0)
                currentStateIndex = -1;
            else
                currentStateIndex = currentStateIndex % stateList.Count;
            // 下面的state是下一阶段的
            State s = GetCurrentState();
            if (s != null)
                currentTimeLeft = s.totoalTime;
        }


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

    /// <summary>
    /// 获取某个状态
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public State GetState(int index)
    {
        if (index <= stateList.Count)
            for (int i = 0; i < index - stateList.Count + 1; i++)
                stateList.Add(new State());
        return stateList[index];
    }

    private State GetCurrentState()
    {
        if (currentStateIndex >= 0 && currentStateIndex < stateList.Count)
            return stateList[currentStateIndex];
        else
            return null;
    }

    public override void OnCollision(Collider2D collision)
    {
        RetangleAreaEffectExecution e;
        if (collision.TryGetComponent<RetangleAreaEffectExecution>(out e))
        {
            if (!cloudGroupList.Contains(e) && e.FloatDict.ContainsKey("offsetX"))
            {
                cloudGroupList.Add(e);
            }
        }
        else
        {
            base.OnCollision(collision);
        }
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

    public float GetVelocity()
    {
        return velocity;
    }

    public void SetVelocity(float v)
    {
        velocity = v;
    }

    public static WindAreaEffectExecution GetInstance(float col, float row, Vector2 position)
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
    /// <param name="e">受影响的风域</param>
    /// <param name="StayTime">静止等待时间</param>
    /// <param name="ChangeTime">加减速时间</param>
    /// <param name="UniformTime">匀速时间</param>
    /// <param name="isMoveRightStart">是否从右向移动开始</param>
    public static void SetClassicalWindAreaEffectMode(WindAreaEffectExecution e, int startState, int StayTime, int ChangeTime, int UniformTime, bool isMoveRightStart)
    {
        // 风域方向变化周期
        int state = 0; // 阶段 0等待，1加速 2匀速 3减速
        int rotate_x;
        if (isMoveRightStart)
            rotate_x = 1;
        else
            rotate_x = -1;
        int timeLeft = 0;

        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            timeLeft = StayTime;
            state = startState;
        });
        t.AddTaskFunc(delegate
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
            return false;
        });
        e.AddTask(t);
    }

    public static void SetClassicalWindAreaEffectMode(WindAreaEffectExecution e, int startState, int StayTime, int ChangeTime, int UniformTime)
    {
        SetClassicalWindAreaEffectMode(e, startState, StayTime, ChangeTime, UniformTime, false);
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/WindAreaEffect", gameObject);
    }


    // 风域任务
    private class WindTask : ITask
    {
        private static FloatModifier AddDamageRate = new FloatModifier(1); // 顺风，受伤增幅
        private static FloatModifier DecDamageRate = new FloatModifier(1); // 逆风，减伤

        private List<WindAreaEffectExecution> list = new List<WindAreaEffectExecution>(); // 进入的风域
        private BaseUnit unit;
        private bool isWind; // 当前是否受风
        private bool isDownWind; // 当前是否顺风（目标移动方向与风向一致）

        public WindTask(BaseUnit unit, WindAreaEffectExecution w, float add_dmg_rate, float dec_dmg_rate)
        {
            AddDamageRate.Value = add_dmg_rate;
            DecDamageRate.Value = dec_dmg_rate;
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
            unit.transform.position = unit.transform.position + new Vector3(velocity, 0, 0);
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

        public void ShutDown()
        {
            
        }

        public bool IsClearWhenDie()
        {
            return true;
        }
    }
}
