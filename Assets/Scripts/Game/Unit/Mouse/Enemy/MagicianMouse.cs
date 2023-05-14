
using UnityEngine;
/// <summary>
/// ħ��ʦ��
/// </summary>
public class MagicianMouse : MouseUnit
{
    private static RuntimeAnimatorController Retinue_RuntimeAnimatorController; // ��ӵ�
    private float distance; // Ͷ����ˮƽ����
    private float minCanAttackPosX; // ���Թ�����x��С���꣨��С��ĳ��X��Ͳ����ڹ����ˣ�
    private float maxCanAttackPosX; 


    public override void Awake()
    {
        if (Retinue_RuntimeAnimatorController == null)
            Retinue_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Mouse/44/Retinue");
        base.Awake();
    }

    public override void MInit()
    {
        distance = 4 * MapManager.gridWidth;
        minCanAttackPosX = MapManager.GetColumnX(1) + distance; // ����㷨������Զ���Դ������
        maxCanAttackPosX = MapManager.GetColumnX(8);
        base.MInit();
    }

    public override void OnIdleState()
    {
        // ûĿ���˾�����
        if (!IsMeetGeneralAttackCondition())
            SetActionState(new MoveState(this));
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ûС��ĳ��x������ܹ�����
        return transform.position.x > minCanAttackPosX && transform.position.x < maxCanAttackPosX;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ����һ��С����
        MouseModel m = MouseModel.GetInstance(Retinue_RuntimeAnimatorController);
        m.SetBaseAttribute(100, 10, 2.0f, 1.0f, 0, 0.5f, 0);
        m.transform.position = transform.position;
        m.currentYIndex = MapManager.GetYIndex(m.transform.position.y);
        m.SetActionState(new MoveState(m));
        GameController.Instance.AddMouseUnit(m);

        // Ȼ������ӳ�ȥ
        // ���һ�����������
        CustomizationTask t = TaskManager.GetParabolaTask(m, distance/60, distance/2, m.transform.position, m.transform.position + distance * (Vector3)moveRotate, false);
        t.AddOnEnterAction(delegate {
            m.SetActionState(new TransitionState(m));
            m.animatorController.Play("Fly", true);
        });
        t.AddOnExitAction(delegate
        {
            m.SetActionState(new MoveState(m));
            // ������غ���ѣ2s
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 120, false));
        });
        m.AddTask(t);
    }


    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new MoveState(this));
    }
}
