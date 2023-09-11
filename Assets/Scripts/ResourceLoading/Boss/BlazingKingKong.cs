namespace ResourceLoading.Boss
{
    public class BlazingKingKong : IBoss
    {
        private int type = 13;

        private string[] pathArr = new string[]
        {
            "Boss/13/Stab", "Boss/12/FireBullet"
        };

        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Boss/" + type + "/" + shape + "/0");
            foreach (var path in pathArr)
                GameManager.Instance.GetRuntimeAnimatorController(path);
            GameManager.Instance.GetSprite("Food/40/lightning");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Boss/" + type + "/" + shape);
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Boss/" + type + "/" + shape + "/0");
            foreach (var path in pathArr)
                GameManager.Instance.UnLoadRuntimeAnimatorController(path);
            GameManager.Instance.UnLoadSprite("Food/40/lightning");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Boss/" + type + "/" + shape);
        }
    }
}