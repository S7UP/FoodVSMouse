namespace ResourceLoading.Food
{
    public class EggPitcher : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/37/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/37/" + shape + "/Bullet");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/37");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/37/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/37/" + shape + "/Bullet");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/37");
        }
    }
}