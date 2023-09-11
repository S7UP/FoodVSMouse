namespace ResourceLoading.Boss
{
    public class MistyJulie : IBoss
    {
        private int type = 10;

        private string[] pathArr = new string[]
        {
            "Boss/10/Missile"
        };

        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Boss/" + type + "/" + shape + "/0");
            foreach (var path in pathArr)
                GameManager.Instance.GetRuntimeAnimatorController(path);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Boss/" + type + "/" + shape);
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Boss/" + type + "/" + shape + "/0");
            foreach (var path in pathArr)
                GameManager.Instance.UnLoadRuntimeAnimatorController(path);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Boss/" + type + "/" + shape);
        }
    }
}