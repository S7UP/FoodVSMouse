using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test
{
    public static void OnGameControllerAwake()
    {
        Debug.Log("OnGameControllerAwake()");
        // ���ƽA����
        //GeneralAttackSkillAbility atk = new GeneralAttackSkillAbility(GameController.Instance.CreateMouseUnit(0, new BaseEnemyGroup.EnemyInfo() { type = 0, shape = 0 }));
        //FoodUnit food = new FoodUnit();
        //food.mUnitType = UnitType.Food;
        //food.mType = 7;
        //food.mShape = 2;
        //GeneralAttackSkillAbility atk = new GeneralAttackSkillAbility(food);
        //atk.Init("��ͨ����", 0.0f, 57.0f, 1.0f, SkillAbility.Type.GeneralAttack, false);
        //atk.SaveInfo();

        // �����ʳ������Ϣ
        //BaseCardBuilder.SaveNewCardBuilderInfo();
    }
}
