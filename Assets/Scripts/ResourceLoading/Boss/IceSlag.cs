namespace ResourceLoading.Boss
{
    public class IceSlag : IBoss
    {
        private int type = 3;

        private string[] pathArr = new string[]
        {
            "Boss/3/IceBullet", "Boss/3/FireBullet", "Boss/3/LightBullet"
        };

        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Boss/" + type + "/" + shape + "/0");
            foreach (var path in pathArr)
                GameManager.Instance.GetRuntimeAnimatorController(path);
            GameManager.Instance.GetSprite("Boss/3/IceIcon");
            GameManager.Instance.GetSprite("Boss/3/FireIcon");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Boss/" + type + "/" + shape);
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Boss/" + type + "/" + shape + "/0");
            foreach (var path in pathArr)
                GameManager.Instance.UnLoadRuntimeAnimatorController(path);
            GameManager.Instance.UnLoadSprite("Boss/3/IceIcon");
            GameManager.Instance.UnLoadSprite("Boss/3/FireIcon");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Boss/" + type + "/" + shape);
        }
    }
}