using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test
{
    public static void OnGameControllerAwake()
    {
        Debug.Log("OnGameControllerAwake()");
        // 添加平A技能
        //GeneralAttackSkillAbility atk = new GeneralAttackSkillAbility(GameController.Instance.CreateMouseUnit(0, new BaseEnemyGroup.EnemyInfo() { type = 0, shape = 0 }));
        //FoodUnit food = new FoodUnit();
        //food.mUnitType = UnitType.Food;
        //food.mType = 7;
        //food.mShape = 2;
        //GeneralAttackSkillAbility atk = new GeneralAttackSkillAbility(food);
        //atk.Init("普通攻击", 0.0f, 57.0f, 1.0f, SkillAbility.Type.GeneralAttack, false);
        //atk.SaveInfo();

        // 添加美食卡槽信息
        //BaseCardBuilder.SaveNewCardBuilderInfo();
    }
}
