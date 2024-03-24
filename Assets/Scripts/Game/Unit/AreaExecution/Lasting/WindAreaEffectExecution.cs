using System.Collections.Generic;
using UnityEngine;
using S7P.Numeric;
/// <summary>
/// ����
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

    private const string TaskName = "WindTask"; // ר����������

    public const string IgnoreWind = "IgnoreWind"; // ���ӷ糡�Ĵ���
    public float velocity; // ���٣�Ĭ�Ϸ��������ҵģ��������������
    public float add_dmg_rate = 1;
    public float dec_dmg_rate = 1;

    // �����������Ʋ���
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
            // ��ȡĿ������Ψһ�ķ�����
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
            // ��ȡĿ������Ψһ�ķ�����
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
        // �����ٶ�
        {
            State s = GetCurrentState();
            if (s != null)
                velocity = Mathf.Lerp(s.start_v, s.end_v, 1 - (float)currentTimeLeft / s.totoalTime);
            //else
            //    velocity = 0;
        }
        if (currentTimeLeft <= 0)
        {
            // ������һ�׶�
            currentStateIndex++;
            if (stateList.Count == 0)
                currentStateIndex = -1;
            else
                currentStateIndex = currentStateIndex % stateList.Count;
            // �����state����һ�׶ε�
            State s = GetCurrentState();
            if (s != null)
                currentTimeLeft = s.totoalTime;
        }


        // ���������λ�� 
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
    /// ��ȡĳ��״̬
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
    /// ���÷���Ϊ��������ģʽ
    /// </summary>
    /// <param name="e">��Ӱ��ķ���</param>
    /// <param name="StayTime">��ֹ�ȴ�ʱ��</param>
    /// <param name="ChangeTime">�Ӽ���ʱ��</param>
    /// <param name="UniformTime">����ʱ��</param>
    /// <param name="isMoveRightStart">�Ƿ�������ƶ���ʼ</param>
    public static void SetClassicalWindAreaEffectMode(WindAreaEffectExecution e, int startState, int StayTime, int ChangeTime, int UniformTime, bool isMoveRightStart)
    {
        // ������仯����
        int state = 0; // �׶� 0�ȴ���1���� 2���� 3����
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

                // ��һ���׶ξ�����ʱ��
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
                        // ��Ȼ�������߼���������Default���
                        state = 0;
                        timeLeft = StayTime;
                        break;
                }
            }

            // ���£������������
            switch (state)
            {
                // ����
                case 1:
                    {
                        float r = 1 - (float)timeLeft / ChangeTime;
                        e.SetVelocity(TransManager.TranToVelocity(1f) * r * rotate_x);
                    }
                    break;
                // ����
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


    // ��������
    private class WindTask : ITask
    {
        private static FloatModifier AddDamageRate = new FloatModifier(1); // ˳�磬��������
        private static FloatModifier DecDamageRate = new FloatModifier(1); // ��磬����

        private List<WindAreaEffectExecution> list = new List<WindAreaEffectExecution>(); // ����ķ���
        private BaseUnit unit;
        private bool isWind; // ��ǰ�Ƿ��ܷ�
        private bool isDownWind; // ��ǰ�Ƿ�˳�磨Ŀ���ƶ����������һ�£�

        public WindTask(BaseUnit unit, WindAreaEffectExecution w, float add_dmg_rate, float dec_dmg_rate)
        {
            AddDamageRate.Value = add_dmg_rate;
            DecDamageRate.Value = dec_dmg_rate;
            this.unit = unit;
            list.Add(w);
        }

        public void OnEnter()
        {
            isWind = false; // Ĭ�ϲ��ܷ�
            isDownWind = false; // Ĭ��Ϊ���
        }

        public void OnUpdate()
        {
            // ��ȡ��ǰ���ܷ��ٺ�
            float velocity = 0;
            foreach (var w in list)
            {
                velocity += w.GetVelocity();
            }
            unit.transform.position = unit.transform.position + new Vector3(velocity, 0, 0);
            // ���״̬�Ƿ����仯����������

            if (isWind) 
            {
                // ����ܷ�������

                if (velocity == 0)
                {
                    // ���ٽ�Ϊ0��תΪ���ܷ�
                    unit.NumericBox.DamageRate.RemoveModifier(AddDamageRate);
                    unit.NumericBox.DamageRate.RemoveModifier(DecDamageRate);
                    isWind = false;
                }
                else if (!isDownWind && unit.moveRotate.x * velocity >= 0)
                {
                    // ���ʱ ���Ŀ�곯��������Ƿ�ͬ���������תΪ˳��
                    ChangeToDownWindModifier();
                    isDownWind = true;
                }
                else if (isDownWind && unit.moveRotate.x * velocity < 0)
                {
                    // ˳��ʱ ���Ŀ�곯��������Ƿ����������תΪ���
                    ChangeToUpWindModifier();
                    isDownWind = false;
                }
            }
            else
            {
                // �����ܷ�ʱ
                if(velocity != 0)
                {
                    // ���˷��٣�Ҫ��Ϊ�ܷ�
                    isWind = true;
                    // �ж���˳�绹�����
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
        /// �л�Ϊ˳���ǩ
        /// </summary>
        private void ChangeToDownWindModifier()
        {
            unit.NumericBox.DamageRate.RemoveModifier(AddDamageRate);
            unit.NumericBox.DamageRate.RemoveModifier(DecDamageRate);

            unit.NumericBox.DamageRate.AddModifier(AddDamageRate);
        }

        /// <summary>
        /// �л�Ϊ����ǩ
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
