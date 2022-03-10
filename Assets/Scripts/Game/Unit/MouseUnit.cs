using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using static UnityEngine.UI.CanvasScaler;

public class MouseUnit : BaseUnit
{
    // 老鼠单位的属性
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        public double baseMoveSpeed;
        public double[] hertRateList; 
    }

    // Awake里Find一次的组件
    public Rigidbody2D rigibody2D;
    protected Animator animator;

    // 其他属性
    public float mBaseMoveSpeed; // 基础移动速度
    public float mCurrentMoveSpeed; // 当前移动速度
    protected double[] mHertRateList; // 切换贴图时的受伤比率（高->低)
    public int mHertIndex; // 受伤贴图阶段

    // 索敌相关
    protected bool isBlock; // 是否被阻挡
    protected BaseUnit mBlockUnit; // 阻挡者

    /// <summary>
    /// 老鼠每次被投入战场时要做的初始化工作，要确定其各种属性
    /// </summary>
    public override void MInit()
    {
        base.MInit();
        // 从Json中读取的属性以及相关的初始化
        MouseUnit.Attribute attr = GameController.Instance.GetMouseAttribute();

        mBaseMoveSpeed = (float)attr.baseMoveSpeed; // 移动速度值为格/秒，默认设为0.5格/秒
        mCurrentMoveSpeed = mBaseMoveSpeed;
        mHertRateList = (double[])attr.hertRateList;
        mHertIndex = 0;

        // 移动状态test
        SetActionState(new MoveState(this));
        // 更新贴图
        UpdateRuntimeAnimatorController();
    }

    /// <summary>
    /// 只有对象被创建时做一次，主要是用来获取各组件的引用
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        jsonPath += "Mouse/";
        mPreFabPath = "Mouse/Pre_Mouse";
        // 组件获取
        rigibody2D = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.transform.Find("Ani_Mouse").gameObject.GetComponent<Animator>();
    }

    /// <summary>
    /// 单位被对象池回收时触发，主要是用来将各种属性再初始化回去
    /// </summary>
    public override void OnDisable()
    {
        base.OnDisable();
        // 其他属性
        mBaseMoveSpeed = 0; // 基础移动速度
        mCurrentMoveSpeed = 0; // 当前移动速度
        mHertRateList = null;

        // 索敌相关
        isBlock = false; // 是否被阻挡
        mBlockUnit = null; // 阻挡者
    }



    /// <summary>
    /// 至少大部分老鼠应该有一个能直接攻击的手段
    /// </summary>
    /// <param name="unit"></param>
    public void TakeDamage(BaseUnit unit)
    {
        unit.OnDamage(mCurrentAttack);
    }


    // 注：一个Collider2D不应该直接使用Transform或者其offset属性来移动它，而是应该使用Rigidbody2D的移动代替之。这样会得到最好的表现和正确的碰撞检测。
    // 因此，操作老鼠移动不应该用上面的transform,而是用下面的rigibody2D
    public override void SetPosition(Vector3 V3)
    {
        rigibody2D.MovePosition(V3);
    }

    // 判断是否发现攻击目标
    protected virtual void SearchTarget()
    {
        // 被阻挡了
        if (isBlock)
        {
            // 阻挡对象是否有效
            if (mBlockUnit.IsValid())
            {
                SetActionState(new AttackState(this));
            }
            else
            {
                isBlock = false;
                SetActionState(new MoveState(this));
            }
        }
    } 

    // 判断能不能攻击了，限定在OnAttackState()里调用，允许子类重写
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

    // 当真正攻击时做出的具体操作。
    protected virtual void Attack()
    {
        if (mBlockUnit != null && mBlockUnit.IsValid())
        {
            TakeDamage(mBlockUnit);
        }
    }


    // 以下为 IBaseStateImplementor 接口的方法实现
    public override void OnIdleState()
    {
        // 攻击重置
        mAttackFlag = true;
        // 索敌逻辑
        SearchTarget();
    }

    public override void OnMoveState()
    {
        // 攻击重置
        mAttackFlag = true;
        // 移动更新
        SetPosition((Vector2)GetPosition() + Vector2.left * mCurrentMoveSpeed * 0.005f);
        // 索敌逻辑
        SearchTarget();
    }

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
            SetActionState(new IdleState(this));
        }
    }

    /// <summary>
    /// 老鼠单位默认都是处在同行被美食阻挡
    /// 提示，特殊类型老鼠可以重写这个方法而强制设为不可阻挡，如幽灵鼠
    /// </summary>
    public override bool CanBlock(BaseUnit unit)
    {
        if(unit is FoodUnit)
        {
            Debug.Log("检测到美食单位");
            return GetRowIndex() == unit.GetRowIndex();
        }
        return false;
    }
    
    /// <summary>
    /// 老鼠单位默认被处在同行的子弹击中
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return GetRowIndex() == bullet.GetRowIndex();
    }

    /// <summary>
    /// 每帧检测血量，更新单位贴图状态
    /// </summary>
    protected virtual void UpdateHertMap()
    {
        // 要是死了的话就免了吧
        if (isDeathState)
            return;

        // 是否要切换控制器的flag
        bool flag = false;
        // 恢复到上一个受伤贴图检测
        while(mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex --;
            flag = true;
        }
        // 下一个受伤贴图的检测
        while(mHertIndex < mHertRateList.Length && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex ++;
            flag = true;
        }
        // 有切换通知时才切换
        if(flag)
            UpdateRuntimeAnimatorController();
    }

    /// <summary>
    /// 自动更新贴图
    /// </summary>
    /// <param name="collision"></param>
    public void UpdateRuntimeAnimatorController()
    {
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + mType + "/" + mShape + "/" + mHertIndex);
    }

    // rigibody相关
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 死亡动画时不接受任何碰撞事件
        if (isDeathState)
        {
            return;
        }

        if (collision.tag.Equals("Food"))
        {
            // 检测到美食单位碰撞了！
            FoodUnit food = collision.GetComponent<FoodUnit>();
            if (UnitManager.CanBlock(this, food)) // 检测双方能否互相阻挡
            {
                isBlock = true;
                mBlockUnit = food;
            }
        }
        else if (collision.tag.Equals("Bullet"))
        {
            // 检测到子弹单位碰撞了
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            if(UnitManager.CanBulletHit(this, bullet)) // 检测双方能否互相击中
            {
                bullet.TakeDamage(this);
            }
        }
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // 更新贴图控制器状态
        UpdateHertMap();
    }

    /// <summary>
    /// 状态相关
    /// </summary>
    public override void OnIdleStateEnter()
    {
        animator.Play("Idle");
    }

    public override void OnMoveStateEnter()
    {
        animator.Play("Move");
    }

    public override void OnAttackStateEnter()
    {
        animator.Play("Attack");
    }

    public override void OnDieStateEnter()
    {
        animator.Play("Die");
    }

    public override void DuringDeath()
    {
        // 切换时的第一帧直接不执行update()，因为下述的info.normalizedTime的值还停留在上一个状态，逻辑会出问题！
        if (currentStateTimer <= 0)
        {
            return;
        }
        // 获取Die的动作信息，使得回收时机与动画显示尽可能同步
        int currentFrame = AnimatorManager.GetCurrentFrame(animator);
        int totalFrame = AnimatorManager.GetTotalFrame(animator);
        if (currentFrame>totalFrame && currentFrame%totalFrame == 1) // 动画播放完毕后调用AfterDeath()
        {
            AfterDeath();
        }
    }


    public static void SaveNewMouseInfo()
    {
        MouseUnit.Attribute attr = new MouseUnit.Attribute()
        {
            baseAttrbute = new BaseUnit.Attribute()
            {
                name = "黄瓜平民鼠", // 单位的具体名称
                type = 0, // 单位属于的分类
                shape = 3, // 单位在当前分类的变种编号

                baseHP = 170, // 基础血量
                baseAttack = 10, // 基础攻击
                baseAttackSpeed = 1.0, // 基础攻击速度
                attackPercent = 0.6,
                baseHeight = 0, // 基础高度
            },
            baseMoveSpeed = 1.0,
            hertRateList = new double[] { 0.5 }
        };

        Debug.Log("开始存档老鼠信息！");
        JsonManager.Save(attr, "Mouse/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        Debug.Log("老鼠信息存档完成！");
    }
}
