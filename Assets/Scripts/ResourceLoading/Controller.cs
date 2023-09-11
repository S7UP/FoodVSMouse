using System.Collections.Generic;

using UnityEngine;

namespace ResourceLoading
{
    /// <summary>
    /// ºËÐÄ¿ØÖÆÆ÷
    /// </summary>
    public class Controller
    {
        private static Dictionary<string, Food.IFood> foodPackageDict = new Dictionary<string, Food.IFood>()
        {
        //{ "0", new Food.SmallStove() },
        //{ "1", new Food.CupLight() },
        //{ "2", new Food.BigStove() },
        //{ "3", new Food.CoffeePowder() },
        //{ "4", new Food.CherryPuddingFoodUnit() },
        //{ "5", new Food.IceCream() },
        //{ "6", new Food.WaterPipeFoodUnit() },
        //{ "7", new Food.ThreeLineFoodUnit() },
        //{ "8", new Food.HeaterFoodUnit() },
        //{ "9", new Food.WoodenDisk() },
        //{ "10", new Food.CottonCandy() },
        //{ "11", new Food.IceBucketFoodUnit() },
        //{ "12", new Food.MelonShield() },
        //{ "13", new Food.Takoyaki() },
        //{ "14", new Food.FlourBag() },
        //{ "15", new Food.WineBottleBoom() },
        //{ "16", new Food.CokeBoom() },
        //{ "17", new Food.BoiledWaterBoom() },
        //{ "18", new Food.WiskyBoom() },
        //{ "19", new Food.PineappleBreadBoom() },
        //{ "20", new Food.SpinCoffee() },
        //{ "22", new Food.MouseCatcher() },
        //{ "23", new Food.SpicyStringBoom() },
        //{ "25", new Food.CoffeeCup() },
        //{ "30", new Food.SugarGourd() },
        //{ "31", new Food.MushroomDestroyer() },
        //{ "33", new Food.PokerShield() },
        //{ "34", new Food.SaladPitcher() },
        //{ "35", new Food.ChocolatePitcher() },
        //{ "36", new Food.TofuPitcher() },
        //{ "37", new Food.EggPitcher() },
        //{ "26", new Food.IceEggPitcher() },
        //{ "38", new Food.ToastBread() },
        //{ "39", new Food.ChocolateBread() },
        //{ "40", new Food.RaidenBaguette() },
        //{ "41", new Food.HotDog() }
        };
        private static Dictionary<string, Mouse.IMouse> mousePackageDict = new Dictionary<string, Mouse.IMouse>()
        {
            //{ "0", new Mouse.NormalMouse() },
            //{ "1", new Mouse.StraddleMouse() },
            //{ "2", new Mouse.KangarooMouse() },
            //{ "4", new Mouse.LadderMouse() },
            //{ "5", new Mouse.HealMouse() },
            //{ "6", new Mouse.FlyMouse() },
            //{ "7", new Mouse.FlyBarrierMouse() },
            //{ "8", new Mouse.FlySelfDestructMouse() },
            //{ "9", new Mouse.AerialBombardmentMouse() },
            //{ "10", new Mouse.AirTransportMouse() },
            //{ "11", new Mouse.NormalWaterMouse() },
            //{ "12", new Mouse.SubmarineMouse() },
            //{ "13", new Mouse.RowboatMouse() },
            //{ "14", new Mouse.FrogMouse() },
            //{ "15", new Mouse.CatapultMouse() },
            //{ "16", new Mouse.Mole() },
            //{ "17", new Mouse.PenguinMouse() },
            //{ "18", new Mouse.ArsonMouse() },
            //{ "19", new Mouse.PandaMouse() },
            //{ "20", new Mouse.MagicMirrorMouse() },
            //{ "22", new Mouse.NinjaMouse() },
            //{ "23", new Mouse.NinjaRetinueMouse() },
            //{ "24", new Mouse.PandaRetinueMouse() },
            //{ "26", new Mouse.SnailMouse() },
            //{ "27", new Mouse.NonMainstreamMouse() },
            //{ "28", new Mouse.BeeMouse() },
            //{ "29", new Mouse.CanMouse() },
            //{ "33", new Mouse.WonderLandNormalMouse() },
            //{ "34", new Mouse.WonderLandMole() },
            //{ "35", new Mouse.WonderLandFairy() },
            //{ "44", new Mouse.MagicianMouse() },
            //{ "45", new Mouse.GhostMouse() },
        };
        private static Dictionary<string, Boss.IBoss> bossPackageDict = new Dictionary<string, Boss.IBoss>() 
        {
            //{ "0", new Boss.DongJun() },
            //{ "1", new Boss.ANuo() },
            //{ "2", new Boss.Pharaoh1() },
            //{ "3", new Boss.IceSlag() },
            //{ "4", new Boss.Thundered() },
            //{ "5", new Boss.PinkPaul() },
            //{ "6", new Boss.BlondeMary() },
            //{ "7", new Boss.SteelClawPete() },
            //{ "8", new Boss.HellShiter() },
            //{ "9", new Boss.NeedleBaron() },
            //{ "10", new Boss.MistyJulie() },
            //{ "11", new Boss.LieutenantHum() },
            //{ "12", new Boss.GrumpyJack() },
            //{ "13", new Boss.BlazingKingKong() },
            //{ "14", new Boss.XiaoMing() },
        };

        public void LoadFood(List<AvailableCardInfo> cardList)
        {
            foreach (var c in cardList)
            {
                if (foodPackageDict.ContainsKey(c.type.ToString()))
                {
                    Food.IFood pk = foodPackageDict[c.type.ToString()];
                    pk.Load(c.maxShape);
                }
            }
        }

        public void UnLoadFood(List<AvailableCardInfo> cardList)
        {
            foreach (var c in cardList)
            {
                if (foodPackageDict.ContainsKey(c.type.ToString()))
                {
                    Food.IFood pk = foodPackageDict[c.type.ToString()];
                    pk.UnLoad(c.maxShape);
                }
            }
        }

        public void LoadMouse(List<BaseEnemyGroup> enemyList)
        {
            foreach (var c in enemyList)
            {
                Debug.Log("type="+c.mEnemyInfo.type+", shape = "+c.mEnemyInfo.shape);
                if (mousePackageDict.ContainsKey(c.mEnemyInfo.type.ToString()))
                {
                    Mouse.IMouse pk = mousePackageDict[c.mEnemyInfo.type.ToString()];
                    pk.Load(c.mEnemyInfo.shape);
                }
            }
        }

        public void UnLoadMouse(List<BaseEnemyGroup> enemyList)
        {
            foreach (var c in enemyList)
            {
                if (mousePackageDict.ContainsKey(c.mEnemyInfo.type.ToString()))
                {
                    Mouse.IMouse pk = mousePackageDict[c.mEnemyInfo.type.ToString()];
                    pk.UnLoad(c.mEnemyInfo.shape);
                }
            }
        }

        public void LoadBoss(List<BaseEnemyGroup> enemyList)
        {
            foreach (var c in enemyList)
            {
                if (bossPackageDict.ContainsKey(c.mEnemyInfo.type.ToString()))
                {
                    Boss.IBoss pk = bossPackageDict[c.mEnemyInfo.type.ToString()];
                    pk.Load(c.mEnemyInfo.shape);
                }
            }
        }

        public void UnLoadBoss(List<BaseEnemyGroup> enemyList)
        {
            foreach (var c in enemyList)
            {
                if (bossPackageDict.ContainsKey(c.mEnemyInfo.type.ToString()))
                {
                    Boss.IBoss pk = bossPackageDict[c.mEnemyInfo.type.ToString()];
                    pk.UnLoad(c.mEnemyInfo.shape);
                }
            }
        }
    }
}
