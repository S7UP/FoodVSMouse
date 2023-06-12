using S7P.Numeric;

using System;
using System.Collections.Generic;
/// <summary>
/// 词条管理器
/// </summary>
public class TagsManager
{
    private static bool isLoad;
    private const string localPath = "Tags/";

    private static ExcelManager.CSV _General_csv;
    public static ExcelManager.CSV General_csv { get { if (_General_csv == null) Load(); return _General_csv; } }
    public static Dictionary<string, TagInfo> generalTagInfoDict = new Dictionary<string, TagInfo>();
    // 通用词条具体功能实现代码字典
    public static Dictionary<string, Action> generalTagActionDict = new Dictionary<string, Action>()
    {
        { "General_hp0",  delegate{ GeneralHp("General_hp0"); } },
        { "General_hp1",  delegate{ GeneralHp("General_hp1"); } },
        { "General_hp2",  delegate{ GeneralHp("General_hp2"); } },
        { "General_moveSpeed0", delegate{ GeneralMoveSpeed("General_moveSpeed0"); } },
        { "General_moveSpeed1", delegate{ GeneralMoveSpeed("General_moveSpeed1"); } },
        { "General_moveSpeed2", delegate{ GeneralMoveSpeed("General_moveSpeed2"); } },
        { "General_attack0", delegate{ GeneralAttack("General_attack0"); } },
        { "General_attack1", delegate{ GeneralAttack("General_attack1"); } },
        { "General_attack2", delegate{ GeneralAttack("General_attack2"); } },
        { "General_attackSpeed0", delegate{ GeneralAttackSpeed("General_attackSpeed0"); } },
        { "General_attackSpeed1", delegate{ GeneralAttackSpeed("General_attackSpeed1"); } },
        { "General_attackSpeed2", delegate{ GeneralAttackSpeed("General_attackSpeed2"); } },
        { "General_burnDefence0", delegate{ GeneralBurnDefence("General_burnDefence0"); } },
        { "General_burnDefence1", delegate{ GeneralBurnDefence("General_burnDefence1"); } },
        { "General_burnDefence2", delegate{ GeneralBurnDefence("General_burnDefence2"); } },
        { "General_lastDefence",  delegate{ GeneralLastDefence("General_lastDefence"); } },
    };

    public static void Load()
    {
        if (!isLoad)
        {
            _General_csv = ExcelManager.ReadCSV(localPath+"GeneralTags", 5);
            for (int i = 0; i < General_csv.GetRow(); i++)
            {
                TagInfo info = new TagInfo() {
                    id = General_csv.GetValue(i, 0),
                    path = General_csv.GetValue(i, 1),
                    name = General_csv.GetValue(i, 2),
                    rank = int.Parse(General_csv.GetValue(i, 3)),
                    descript = General_csv.GetValueByReplaceAllParam(i, 4),
                    isNull = false,
                    ParamArray = General_csv.GetParamDict(i, 4)
                };
                generalTagInfoDict.Add(info.id, info);
            }
            isLoad = true;
        }
    }

    /// <summary>
    /// 获取某关的预设词条表
    /// </summary>
    /// <param name="chapterIndex"></param>
    /// <param name="sceneIndex"></param>
    /// <param name="stageIndex"></param>
    /// <returns></returns>
    public static List<TagInfo[]> GetTagArrayList(int chapterIndex, int sceneIndex, int stageIndex)
    {
        List<TagInfo[]> list = new List<TagInfo[]>();
        // 载入通用词条表
        foreach (var arr in GetGeneralTagArrayList())
        {
            list.Add(arr);
        }
        return list;
    }

    /// <summary>
    /// 获取通用的词条表
    /// </summary>
    /// <returns></returns>
    private static List<TagInfo[]> GetGeneralTagArrayList()
    {
        List<TagInfo[]> list = new List<TagInfo[]>();
        // 加血
        {
            TagInfo[] arr = new TagInfo[4] { generalTagInfoDict["General_hp0"], generalTagInfoDict["General_hp1"], generalTagInfoDict["General_hp2"], new TagInfo() { isNull = true } };
            list.Add(arr);
        }
        // 加速
        {
            TagInfo[] arr = new TagInfo[4] { generalTagInfoDict["General_moveSpeed0"], generalTagInfoDict["General_moveSpeed1"], generalTagInfoDict["General_moveSpeed2"], new TagInfo() { isNull = true } };
            list.Add(arr);
        }
        // 加攻击
        {
            TagInfo[] arr = new TagInfo[4] { generalTagInfoDict["General_attack0"], generalTagInfoDict["General_attack1"], generalTagInfoDict["General_attack2"], new TagInfo() { isNull = true } };
            list.Add(arr);
        }
        // 加攻速
        {
            TagInfo[] arr = new TagInfo[4] { generalTagInfoDict["General_attackSpeed0"], generalTagInfoDict["General_attackSpeed1"], generalTagInfoDict["General_attackSpeed2"], new TagInfo() { isNull = true } };
            list.Add(arr);
        }
        // 爆抗
        {
            TagInfo[] arr = new TagInfo[4] { generalTagInfoDict["General_burnDefence0"], generalTagInfoDict["General_burnDefence1"], generalTagInfoDict["General_burnDefence2"], new TagInfo() { isNull = true } };
            list.Add(arr);
        }
        // 无猫
        {
            TagInfo[] arr = new TagInfo[4] { new TagInfo() { isNull = true }, generalTagInfoDict["General_lastDefence"], new TagInfo() { isNull = true }, new TagInfo() { isNull = true } };
            list.Add(arr);
        }
        return list;
    }

    public static TagInfo GetGeneralTagInfo(string id)
    {
        return generalTagInfoDict[id];
    }

    public static Action GetTagAction(string id)
    {
        if (generalTagInfoDict.ContainsKey(id))
        {
            if (generalTagActionDict.ContainsKey(id))
                return generalTagActionDict[id];
            else
                return null;
        }
        return null;
    }

    #region 一些词条功能实现的方法
    private static void GeneralHp(string id)
    {
        // 小兵
        {
            float hpRate = 1 + GetGeneralTagInfo(id).ParamArray["hp0"][0] / 100;
            float dmgRate = 1 - GetGeneralTagInfo(id).ParamArray["defence"][0] / 100;
            Action<MouseUnit> action = (u) => { 
                u.SetMaxHpAndCurrentHp(u.mMaxHp * hpRate);
                bool isBuff = false;
                FloatModifier mod = new FloatModifier(dmgRate);
                CustomizationTask t = new CustomizationTask();
                t.AddOnEnterAction(delegate {
                    u.NumericBox.DamageRate.AddModifier(mod);
                    isBuff = true;
                });
                t.AddTaskFunc(delegate {
                    if (isBuff)
                    {
                        if (u.IsBlock())
                        {
                            isBuff = false;
                            u.NumericBox.DamageRate.RemoveModifier(mod);
                        }
                            
                    }
                    else
                    {
                        if (!u.IsBlock())
                        {
                            isBuff = true;
                            u.NumericBox.DamageRate.AddModifier(mod);
                        }
                    }
                    return false;
                });
                u.taskController.AddTask(t);
            };
            GameController.Instance.mMouseFactory.AddProcessAction(action);
        }
        // BOSS
        {
            float hpRate = 1 + GetGeneralTagInfo(id).ParamArray["hp1"][0] / 100;
            Action<MouseUnit> action = (u) => { u.SetMaxHpAndCurrentHp(u.mMaxHp * hpRate); };
            GameController.Instance.mBossFactory.AddProcessAction(action);
        }
    }

    private static void GeneralMoveSpeed(string id)
    {
        Action<MouseUnit> action = (u) => { u.NumericBox.MoveSpeed.AddPctAddModifier(new FloatModifier(GetGeneralTagInfo(id).ParamArray["percent"][0])); };
        GameController.Instance.mMouseFactory.AddProcessAction(action);
    }

    private static void GeneralBurnDefence(string id)
    {
        Action<MouseUnit> action = (u) => { u.NumericBox.BurnRate.AddModifier(new FloatModifier(1 - GetGeneralTagInfo(id).ParamArray["percent"][0]/100)); };
        GameController.Instance.mMouseFactory.AddProcessAction(action);
    }

    private static void GeneralAttack(string id)
    {
        Action<MouseUnit> action = (u) => { u.NumericBox.Attack.AddPctAddModifier(new FloatModifier(GetGeneralTagInfo(id).ParamArray["percent"][0])); };
        GameController.Instance.mMouseFactory.AddProcessAction(action);
    }

    private static void GeneralAttackSpeed(string id)
    {
        Action<MouseUnit> action = (u) => { u.NumericBox.AttackSpeed.AddPctAddModifier(new FloatModifier(GetGeneralTagInfo(id).ParamArray["percent"][0])); };
        GameController.Instance.mMouseFactory.AddProcessAction(action);
    }

    private static void GeneralLastDefence(string id)
    {
        for (int i = 0; i < 7; i++)
        {
            BaseCat cat = GameController.Instance.mItemController.GetSpecificRowCat(i);
            if (cat != null)
                cat.MDestory();
        }
    }
    #endregion
}
