namespace ResourceLoading.Boss
{
    public class Pharaoh1 : IBoss
    {
        private int type = 2;

        private string[] pathArr = new string[]
        {
            "Boss/2/0/0", "Boss/2/0/1", "Boss/2/0/2", "Boss/2/Coffin", "Boss/2/Mummy/0", "Boss/2/Mummy/1", "Boss/2/Bandage", "Boss/2/Bug", "Boss/2/Curse"
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