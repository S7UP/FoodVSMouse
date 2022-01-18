using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 在战斗场景中，负责管理老鼠单位的碰撞逻辑
/// 注：这个组件由程序自动添加给老鼠对象，而非编辑器手动添加实现！
/// </summary>
public class MouseCollision : MonoBehaviour
{
    // 由外部自动赋值的引用
    public MouseUnit mouseUnit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        // 被东西挡着了！
        if (collision.tag.Equals("Food"))
        {
            Debug.Log("切换为攻击模式！");
            MouseAttackState state = new MouseAttackState(mouseUnit);
            mouseUnit.SetActionState(state);
            state.mTarget = collision.gameObject;
        }
    }
}
