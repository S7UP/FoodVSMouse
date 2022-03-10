using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using static UnityEngine.GraphicsBuffer;

public class FoodUnit : BaseUnit
{
    // 美食单位的属性
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        public FoodType foodType;
    }

    // Awake获取的组件
    protected Animator animator;
    protected Animator rankAnimator;

    // 其他
    public FoodType mFoodType; // 美食职业划分

    private BaseGrid mGrid; // 卡片所在的格子（单格卡)
    private List<BaseGrid> mGridList; // 卡片所在的格子（多格卡）
    public bool isUseSingleGrid; // 是否只占一格


    public override void Awake()
    {
        base.Awake();
        mPreFabPath = "Food/Pre_Food";
        jsonPath = "Food/";
        animator = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        rankAnimator = transform.Find("Ani_Rank").gameObject.GetComponent<Animator>();
    }

    // 单位被对象池回收时触发
    public override void OnDisable()
    {
        base.OnDisable();
        mFoodType = 0; // 美食职业划分
        mGrid = null; // 卡片所在的格子（单格卡)
        mGridList = null; // 卡片所在的格子（多格卡）
        isUseSingleGrid = false; // 是否只占一格
    }



    // 判断能不能开火了，限定在OnAttackState()里调用，允许子类重写
    protected virtual bool CanAttack()
    {
        // 获取Attack的动作信息，使得伤害判定与动画显示尽可能同步
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        // 这个normalizedTime的小数可以近似表示一个动画播放进度的百分比，个位数则可以表示已循环的次数。
        int c = Mathf.FloorToInt(info.normalizedTime);

        // 动画进度在到一定时启动伤害判定
        float percent = info.normalizedTime - c;
        if (percent >= attackPercent && mAttackFlag)
        {
            return true;
        }
        return false;
    }

    // 当真正开火时发出的子弹相关内容，允许且需要子类重写以达到多态性。
    protected virtual void Attack()
    {
        // Debug.Log("卡片攻击了！");
        for (int i = -1; i <= 1; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                GameController.Instance.CreateBullet(transform.position + Vector3.right * 0.5f * j + Vector3.up * 0.7f * i);
            }
        }
    }

    // 每次对象被创建时要做的初始化工作
    public override void MInit()
    {
        base.MInit();

        FoodUnit.Attribute attr = GameController.Instance.GetFoodAttribute();
        mFoodType = attr.foodType;

        mGridList = new List<BaseGrid>();
        isUseSingleGrid = true;

        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/7/2");
        SetActionState(new IdleState(this));
        
        rankAnimator.Play("12"); // 先播放12星级的图标动画
    }

    // 在待机状态时每帧要做的事
    public override void OnIdleState()
    {
        // 默认为攻击计数器为零时才能发起攻击
        if (mAttackCDLeft <= 0)
        {
            SetActionState(new AttackState(this));
            mAttackCDLeft += mAttackCD;
        }
    }

    // 在攻击状态时每帧要做的事
    public override void OnAttackState()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 获取Attack的动作信息，使得伤害判定与动画显示尽可能同步
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (CanAttack())
        {
            Attack();
            mAttackFlag = false;
        }
        else if (info.normalizedTime >= 1.0f) // 攻击动画播放完一次后转为待机状态
        {
            mAttackFlag = true;
            SetActionState(new IdleState(this));
        }
    }


    /// <summary>
    /// 获取卡片所在的格子
    /// </summary>
    /// <returns></returns>
    public BaseGrid GetGrid()
    {
        return mGrid;
    }

    public void SetGrid(BaseGrid grid)
    {
        mGrid = grid;
    }

    /// <summary>
    /// 对于多格卡片请使用这个
    /// </summary>
    /// <returns></returns>
    public List<BaseGrid> GetGridList()
    {
        return mGridList;
    }

    /// <summary>
    /// 当与单位发生碰撞时能否阻挡的判定
    /// 默认是自身与老鼠同行时可以阻挡老鼠
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public override bool CanBlock(BaseUnit unit)
    {
        if(unit is MouseUnit)
        {
            Debug.Log("检测到老鼠单位！");
            return GetRowIndex() == unit.GetRowIndex();
        }
        return false; // 别的单位暂时默认不能阻挡
    }

    /// <summary>
    /// 美食单位默认被处在同行的子弹击中
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return GetRowIndex() == bullet.GetRowIndex();
    }

    /// <summary>
    /// 状态相关
    /// </summary>
    public override void OnIdleStateEnter()
    {
        animator.Play("Idle");
    }

    public override void OnAttackStateEnter()
    {
        animator.Play("Attack");
    }

    public override void OnDieStateEnter()
    {
        // 对于美食来说没有死亡动画的话，直接回收对象就行，在游戏里的体现就是直接消失
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }
}