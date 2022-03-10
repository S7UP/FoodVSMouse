using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ս�����������������Ϸ��λ
/// </summary>
public class BaseUnit : MonoBehaviour, IGameControllerMember, IBaseStateImplementor
{
    protected string jsonPath; // �洢JSON�ļ������·��

    // ��Ҫ���ش洢�ı���
    [System.Serializable]
    public struct Attribute
    {
        public string name; // ��λ�ľ�������
        public int type; // ��λ���ڵķ���
        public int shape; // ��λ�ڵ�ǰ����ı��ֱ��

        public double baseHP; // ����Ѫ��
        public double baseAttack; // ��������
        public double baseAttackSpeed; // ���������ٶ�
        public double attackPercent; // �������ж�ʱ�������ŵİٷֱ�
        public int baseHeight; // �����߶�
    }

    // ����ı���
    public float mBaseHp; //+ ��������ֵ
    public float mMaxHp; //+ �������ֵ
    public float mCurrentHp; //+ ��ǰ����ֵ
    public float mBaseAttack; //+ ����������
    public float mCurrentAttack; //+ ��ǰ������
    public float mBaseAttackSpeed; //+ ���������ٶ�
    public float mCurrentAttackSpeed; //+ ��ǰ�����ٶ�
    public int mAttackCD; //+ ��ǰ�������
    public int mAttackCDLeft; //+ �������������
    protected float attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
    protected bool mAttackFlag; // ������һ�ι����ܷ�������flag
    public int mHeight; //+ �߶�
    public bool isDeathState; // �Ƿ�������״̬


    public string mName; // ��ǰ��λ����������
    public int mType; // ��ǰ��λ�����ࣨ�粻ͬ�Ŀ�����ͬ������
    public int mShape; // ��ǰ���൥λ����ۣ�ͬһ�ſ���0��1��2ת�������0��1��2ת�����������֣�

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬
    public int currentStateTimer = 0; // ��ǰ״̬�ĳ���ʱ�䣨�л�״̬ʱ�����ã�

    protected string mPreFabPath; // Ԥ��������/·��

    public virtual void Awake()
    {
        jsonPath = "";
    }

    // ��λ�����������ȡ�����߸մ���ʱ����
    public virtual void OnEnable()
    {
        // MInit();
    }

    // ��λ������ػ���ʱ����
    public virtual void OnDisable()
    {
        // ����ı���
        mBaseHp = 0; //+ ��������ֵ
        mMaxHp = 0; //+ �������ֵ
        mCurrentHp = 0; //+ ��ǰ����ֵ
        mBaseAttack = 0; //+ ����������
        mCurrentAttack = 0; //+ ��ǰ������
        mBaseAttackSpeed = 0; //+ ���������ٶ�
        mCurrentAttackSpeed = 0; //+ ��ǰ�����ٶ�
        mAttackCD = 0; //+ ��ǰ�������
        mAttackCDLeft = 0; //+ �������������
        attackPercent = 0; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
        mAttackFlag = false; // ������һ�ι����ܷ�������flag
        mHeight = 0; //+ �߶�
        isDeathState = true;

        mName = null; // ��ǰ��λ����������
        mType = 0; // ��ǰ��λ�����ࣨ�粻ͬ�Ŀ�����ͬ������
        mShape = 0; // ��ǰ���൥λ����ۣ�ͬһ�ſ���0��1��2ת�������0��1��2ת�����������֣�

        mCurrentActionState = null; //+ ��ǰ����״̬
        currentStateTimer = 0; // ��ǰ״̬�ĳ���ʱ�䣨�л�״̬ʱ�����ã�
    }

    // �л�����״̬
    public void SetActionState(IBaseActionState state)
    {
        if (mCurrentActionState != null)
        {
            mCurrentActionState.OnExit();
        }
        mCurrentActionState = state;
        currentStateTimer = -1; // ���ü�����������Ϊ-1�Ǳ�֤��һ��ִ�е�״̬Updateʱ����ȡ���ļ�����ֵһ��Ϊ0����ϸʵ�ּ�����MUpdate()����
        mCurrentActionState.OnEnter();
    }

    // �ܵ��˺�
    public virtual void OnDamage(float dmg)
    {
        mCurrentHp -= dmg;
        if(mCurrentHp <= 0)
        {
            BeforeDeath();
        }
    }

    /// <summary>
    /// ���㵱ǰѪ���ٷֱ�
    /// </summary>
    /// <returns></returns>
    public float GetHeathPercent()
    {
        return mCurrentHp / mMaxHp;
    }

    public virtual Vector3 GetPosition()
    {
        return gameObject.transform.position;
    }

    // ����λ��
    public virtual void SetPosition(Vector3 V3)
    {
        gameObject.transform.position = V3;
    }

    // �����������������������ȵģ�
    public virtual void BeforeDeath()
    {
        // �����ض����ؾ�һ�°�
        // ����overrideһ��,���ж�һ�����������˲����ø�������ķ����ˣ����ܾȵģ�
        // TNND,Ϊʲô���ȣ����������ǰɣ��������ǰɣ�.wav

        // ������������״̬
        isDeathState = true;
        SetActionState(new DieState(this));
    }

    // �����������ˣ�����ʱ���м�֡״̬��Ҫ������
    public virtual void DuringDeath()
    {
        // ��֪��Ҫ��ɶ�ˣ���������ط��϶��Ȳ�����
    }

    // ����Ÿ�ʲô����û�˾Ȳ�����
    public virtual void AfterDeath()
    {
        // ������ҲҪ����Ϊ�����
        // override
        // Ȼ����ȥ����

        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }


    // ����Ϊ IGameControllerMember �ӿڷ�����ʵ��
    public virtual void MInit()
    {
        //// Ѫ��
        //mBaseHp = 100;
        //mMaxHp = mBaseHp;
        //mCurrentHp = mMaxHp;
        //// ������
        //mBaseAttack = 10;
        //mCurrentAttack = mBaseAttack;
        //// �����ٶ��빥�����
        //mBaseAttackSpeed = 0.5f;
        //mCurrentAttackSpeed = mBaseAttackSpeed;
        //mAttackCD = Mathf.FloorToInt(ConfigManager.fps / (mCurrentAttackSpeed));
        //mAttackCDLeft = 0;
        //attackPercent = 0.60f; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
        //mAttackFlag = true; // ������һ�ι����ܷ�������flag
        //// �߶�
        //mHeight = 0;

        BaseUnit.Attribute attr = GameController.Instance.GetBaseAttribute();
        // Ѫ��
        mBaseHp = (float)attr.baseHP;
        mMaxHp = mBaseHp;
        mCurrentHp = mMaxHp;
        // ������
        mBaseAttack = (float)attr.baseAttack;
        mCurrentAttack = mBaseAttack;
        // �����ٶ��빥�����
        mBaseAttackSpeed = (float)attr.baseAttackSpeed;
        mCurrentAttackSpeed = mBaseAttackSpeed;
        mAttackCD = Mathf.FloorToInt(ConfigManager.fps / (mCurrentAttackSpeed));
        mAttackCDLeft = 0;
        attackPercent = (float)attr.attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���
        mAttackFlag = true; // ������һ�ι����ܷ�������flag
        // �߶�
        mHeight = attr.baseHeight;
        // ����״̬
        isDeathState = false;

        // ��ʼ����ǰ����״̬
        SetActionState(new BaseActionState(this));
    }

    public virtual void MUpdate()
    {
        // �������ݸ���
        if (mAttackCDLeft > 0)
        {
            mAttackCDLeft--;
        }
        // ��λ����״̬��״̬�����������ƶ���������������������
        currentStateTimer += 1; 
        mCurrentActionState.OnUpdate();
    }

    public virtual void MDestory()
    {

    }

    public virtual void MPause()
    {

    }

    public virtual void MResume()
    {

    }

    /// <summary>
    /// �����λ�Ƿ���ս���ϴ��
    /// </summary>
    public bool IsValid()
    {
        return isActiveAndEnabled;
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        return MapManager.GetYIndex(transform.position.y);
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        return MapManager.GetXIndex(transform.position.x);
    }

    /// <summary>
    /// �����Ƿ����赲��������unit��������������д���������ʵ�ֶ�̬
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBlock(BaseUnit unit)
    {
        return false;
    }

    /// <summary>
    /// �����ܷ񱻴�������bullet���У�������������д���������ʵ�ֶ�̬
    /// </summary>
    /// <returns></returns>
    public virtual bool CanHit(BaseBullet bullet)
    {
        return false;
    }

    /// <summary>
    /// ״̬���
    /// </summary>
    public virtual void OnIdleStateEnter()
    {

    }
    public virtual void OnMoveStateEnter()
    {

    }
    public virtual void OnAttackStateEnter()
    {

    }
    public virtual void OnDieStateEnter()
    {

    }
    public virtual void OnIdleState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnMoveState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnAttackState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnStaticState()
    {
        throw new System.NotImplementedException();
    }


    /// <summary>
    /// �����뾲ֹ״̬ʱ
    /// </summary>
    public virtual void OnStaticStateEnter()
    {

    }

    /// <summary>
    /// ���˳���ֹ״̬ʱ
    /// </summary>
    public virtual void OnStaticStateExit()
    {

    }

    // ����
}
