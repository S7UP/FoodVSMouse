using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// è�۱�ʯ
/// </summary>
public class CatEyesJewelSkill : BaseJewelSkill
{
    public CatEyesJewelSkill(float maxEnergy, float startEnergy, float deltaEnergy, Dictionary<string, float[]> paramDict) : base(maxEnergy, startEnergy, deltaEnergy, paramDict)
    {

    }

    protected override void OnExecute()
    {
        // ����èè
        for (int i = 0; i < 7; i++)
        {
            BaseCat cat = (BaseCat)GameController.Instance.CreateItem(new Vector3(MapManager.GetColumnX(-1), MapManager.GetRowY(i), 0), (int)ItemNameTypeMap.Cat, 0);
            cat.SetRowIndex(i);
            cat.OnTriggerEvent();
        }
    }
}
