using System;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ������
/// </summary>
public class LadderMouse : MouseUnit
{
    private static Sprite[] Item_SpriteArray;

    private FloatModifier velocityBuffModifier; // ����buff
    private float old_P2_HpRate; // �׶ε�2ԭʼѪ������

    private bool isDrop;

    public override void Awake()
    {
        if (Item_SpriteArray == null)
        {
            Item_SpriteArray = new Sprite[3];
            for (int i = 0; i < Item_SpriteArray.Length; i++)
                Item_SpriteArray[i] = GameManager.Instance.GetSprite("Mouse/4/" + i + "/Item");
        }
        base.Awake();
    }

    public override void MInit()
    {
        isDrop = false;
        base.MInit();
        velocityBuffModifier = new FloatModifier(100);
        NumericBox.MoveSpeed.AddPctAddModifier(velocityBuffModifier); // ��ʼ���100%����

        CustomizationTask t = new CustomizationTask();
        t.AddTaskFunc(delegate {
            old_P2_HpRate = (float)mHertRateList[1];
            return false;
        });
        AddTask(t);
    }

    public override void OnAttackStateEnter()
    {
        if (!isDrop)
        {
            BaseUnit target = GetCurrentTarget();
            if (target != null && target.IsAlive() && mCurrentAttack * 10 < target.mMaxHp)
            {
                SetActionState(new CastState(this));
                return;
            }
        }
        base.OnAttackStateEnter();
    }

    public override void OnIdleStateEnter()
    {
        base.OnIdleStateEnter();

    }

    /// <summary>
    /// ����¥��
    /// </summary>
    public override void OnCastStateEnter()
    {
        animatorController.Play("Put");
    }

    public override void OnCastState()
    {
        BaseUnit target = GetCurrentTarget();
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce() || target == null || !target.IsAlive() || !UnitManager.CanBlock(this, target))
        {
            SetActionState(new MoveState(this));
        }
    }

    public override void OnCastStateExit()
    {
        BaseUnit target = GetCurrentTarget();
        if(target != null && target.IsAlive() && UnitManager.CanBlock(this, target))
        {
            isDrop = true;
            // ��Ѫ�����ڽ׶ε�2����ǿ��Ѫ��Ϊ�׶ε�2��Ѫ��
            mCurrentHp = Mathf.Min(mCurrentHp, mMaxHp * old_P2_HpRate - 1);
            Vector3 pos = transform.position + 0.25f * MapManager.gridWidth * (Vector3)moveRotate;
            if (moveRotate.x < 0)
            {
                // ����
                pos = new Vector3(Mathf.Max(target.transform.position.x - 0.25f * MapManager.gridWidth, pos.x), pos.y);
            }
            else
            {
                // ����
                pos = new Vector3(Mathf.Min(target.transform.position.x + 0.25f * MapManager.gridWidth, pos.x), pos.y);
            }
            CreateLadder(pos);
            UpdateHertMap();
        }
    }
    

    /// <summary>
    /// ����ͼ����ʱҪ������
    /// </summary>
    public override void OnUpdateRuntimeAnimatorController()
    {
        // 0 Я���������ɳ��
        // 1 Я�����𵯻ɳ��
        // 2 �޵�������
        // 3 �޵��ɸ���
        if (mHertIndex > 0)
        {
            mHertRateList[0] = double.MaxValue;
        }
        if (mHertIndex > 1)
        {
            mHertRateList[0] = double.MaxValue;
            mHertRateList[1] = double.MaxValue;
            // �����û�з����Ӿͱ������Ѫ�����£���ô����һ����������Ķ���ͬʱ���ö�Ӧ����Ϊ��ʩ�ţ����˺������ظ��ͷţ�
            if (isDrop)
                animatorController.Play("Move");
            else
            {
                isDrop = true;
                animatorController.Play("Drop");
            }
            // �Ƴ�����buff
            NumericBox.MoveSpeed.RemovePctAddModifier(velocityBuffModifier);
        }
    }


    private void CreateLadder(Vector2 pos)
    {
        MouseModel m = MouseModel.GetInstance(Item_SpriteArray[mShape]);
        m.transform.position = pos;
        m.SetBaseAttribute(10000, 10, 1.0f, 0, 100, 0, 0);
        m.canTriggerCat = false;
        m.canTriggerLoseWhenEnterLoseLine = false;
        m.isIgnoreRecordDamage = true;
        m.moveRotate = moveRotate;
        if (moveRotate.x > 0)
            m.spriteRenderer.transform.localScale = new Vector2(-1, 1);
        StatusManager.AddIgnoreSettleDownBuff(m, new BoolModifier(true));
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
        m.AddCanBeSelectedAsTargetFunc(delegate { return false; });
        m.AddCanBlockFunc(delegate { return false; });
        m.AddCanHitFunc(delegate { return false; });
        WaterGridType.AddNoAffectByWater(m, new BoolModifier(true)); // �������ˮʴ
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.SetActionState(new MoveState(m));
        GameController.Instance.AddMouseUnit(m);

        // ʹ��Χ�ڵĿ�Ƭ������ѣ �� ʧȥ�赲����
        {
            Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate { return false; };

            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(0.49f*MapManager.gridWidth, MapManager.gridHeight / 2), "ItemCollideAlly");
            r.name = "Ladder_Stun_Ally_Area";
            r.isAffectFood = true;
            r.SetOnFoodEnterAction((f) => {
                f.AddCanBlockFunc(noBlockFunc);
            });
            r.SetOnFoodExitAction((f) => {
                f.RemoveCanBlockFunc(noBlockFunc);
            });
            GameController.Instance.AddAreaEffectExecution(r);


            int timeLeft = 1;
            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                timeLeft--;
                if(timeLeft <= 0)
                {
                    timeLeft += 15;
                    foreach (var f in r.foodUnitList.ToArray())
                        f.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(f, 15, false));
                }
                r.transform.position = m.transform.position;
                return !m.IsAlive();
            });
            task.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(task);
        }

        // ʹ��Χ�ڵ�������Ծ
        {
            float moveDistance = 0;
            switch (mShape)
            {
                case 1:
                    moveDistance = 2*MapManager.gridWidth;
                    break;
                default:
                    moveDistance = MapManager.gridWidth;
                    break;
            }

            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth / 2, MapManager.gridHeight / 2), "ItemCollideEnemy");
            r.name = "Ladder_Jump_Enemy_Area";
            r.isAffectMouse = true;
            r.SetAffectHeight(0);
            r.AddEnemyEnterConditionFunc((u) => {
                return !u.IsBoss() && !UnitManager.IsFlying(u) && u.moveRotate.x*m.moveRotate.x>0 && MouseManager.IsGeneralMouse(u);
            });
            r.SetOnEnemyEnterAction((u) => {
                // ���һ����������� (��Զ���͵����ſڣ��߼����ͽ����ˣ�����һ�й�Ƥ�����赲������
                float dist = Mathf.Min(moveDistance, Mathf.Abs(MapManager.GetColumnX(-0.6f) - u.transform.position.x));
                CustomizationTask t = TaskManager.GetParabolaTask(u, dist / 60, dist / 2, u.transform.position, u.transform.position + (Vector3)u.moveRotate * dist, false, true);
                // �ҽ�ֹ�ƶ�
                u.DisableMove(true);
                t.AddOnExitAction(delegate {
                    u.DisableMove(false);
                });
                u.AddTask(t);
            });
            GameController.Instance.AddAreaEffectExecution(r);


            CustomizationTask task = new CustomizationTask();
            task.AddTaskFunc(delegate {
                r.transform.position = m.transform.position;
                return !m.IsAlive();
            });
            task.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(task);
        }
    }

}
