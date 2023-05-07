using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 轰炸宝石
/// </summary>
public class BombingJewelSkill : BaseJewelSkill
{
    private static RuntimeAnimatorController Shooter_RuntimeAnimatorController;
    private static RuntimeAnimatorController Boom_RuntimeAnimatorController;


    public BombingJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict) : base(maxEnergy, startEnergy, deltaEnergy, paramDict)
    {
        if(Shooter_RuntimeAnimatorController == null)
        {
            Shooter_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Jewel/2/Shooter");
            Boom_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Jewel/2/BoomEffect");
        }
    }

    protected override void OnExecute()
    {
        Vector2[] v2_arr = new Vector2[5] {
            new Vector2(5, 2), new Vector2(5, 4), new Vector2(7, 1), new Vector2(7, 3), new Vector2(7, 5)
        };
        foreach (var v2 in v2_arr)
        {
            CreateBomb(MapManager.GetGridLocalPosition(v2.x, v2.y));
        }
    }

    /// <summary>
    /// 产生一个炸弹
    /// </summary>
    private void CreateBomb(Vector2 pos)
    {
        CustomizationItem item = CustomizationItem.GetInstance(pos, Shooter_RuntimeAnimatorController);
        CustomizationTask t = new CustomizationTask();
        t.AddOnEnterAction(delegate {
            item.animatorController.Play("Execute");
        });
        t.AddTaskFunc(delegate {
            if (item.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                return true;
            }
            return false;
        });
        t.AddOnExitAction(delegate {
            // 产生特效
            BaseEffect e = BaseEffect.CreateInstance(Boom_RuntimeAnimatorController, null, "Boom", null, false);
            e.transform.position = item.transform.position;
            GameController.Instance.AddEffect(e);
            // 产生爆炸判定
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(item.transform.position + 0.5f * Vector3.left * MapManager.gridWidth, 4, 3, "ItemCollideEnemy");
            r.isAffectMouse = true;
            r.SetInstantaneous();
            r.SetOnEnemyEnterAction((u) => {
                BurnManager.BurnDamage(item, u);
            });
            GameController.Instance.AddAreaEffectExecution(r);
            item.ExecuteDeath();
        });
        item.AddTask(t);
        GameController.Instance.AddItem(item);
    }
}
