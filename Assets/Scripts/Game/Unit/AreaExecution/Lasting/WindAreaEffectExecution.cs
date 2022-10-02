using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ����
/// </summary>
public class WindAreaEffectExecution : RetangleAreaEffectExecution
{
    private const string TaskName = "WindTask"; // ר����������
    public float velocity; // ���٣�Ĭ�Ϸ��������ҵģ��������������

    // �����������Ʋ���
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
        // BOSS��λ���ӵ���Ч��
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
        // ��ȡĿ������Ψһ�ķ�����
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
        // ��ȡĿ������Ψһ�ķ�����
        if (unit.GetTask(TaskName) == null)
        {
            Debug.LogWarning("Ŀ����û�з������������¾��˳��˷���");
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
    /// ���÷���Ϊ��������ģʽ
    /// </summary>
    /// <param name="e">��Ӱ��ķ���</param>
    /// <param name="StayTime">��ֹ�ȴ�ʱ��</param>
    /// <param name="ChangeTime">�Ӽ���ʱ��</param>
    /// <param name="UniformTime">����ʱ��</param>
    /// <param name="isLeftStart">�Ƿ�������ƶ���ʼ</param>
    public static void SetClassicalWindAreaEffectMode(WindAreaEffectExecution e, int startState, int StayTime, int ChangeTime, int UniformTime, bool isLeftStart)
    {
        // ������仯����
        int state = 0; // �׶� 0�ȴ���1���� 2���� 3����
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


    // ��������
    private class WindTask : ITask
    {
        private static FloatModifier AddDamageRate = new FloatModifier(1.5f); // ˳�磬��������
        private static FloatModifier DecDamageRate = new FloatModifier(0.5f); // ��磬����

        private List<WindAreaEffectExecution> list = new List<WindAreaEffectExecution>(); // ����ķ���
        private BaseUnit unit;
        private bool isWind; // ��ǰ�Ƿ��ܷ�
        private bool isDownWind; // ��ǰ�Ƿ�˳�磨Ŀ���ƶ����������һ�£�

        public WindTask(BaseUnit unit, WindAreaEffectExecution w)
        {
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
            //unit.SetPosition(unit.GetPosition() +  new Vector3(velocity, 0));
            unit.transform.position = unit.transform.position + new Vector3(velocity, 0, 0);
            //unit.SetPosition((Vector2)unit.GetPosition() + new Vector2(velocity, 0));
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
    }
}
