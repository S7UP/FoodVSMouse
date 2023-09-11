namespace ResourceLoading.Food
{
    public class IceEggPitcher : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/26/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Bullet/7/0");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/26");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/26/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/26");
            // GameManager.Instance.UnLoadRuntimeAnimatorController("Bullet/7/0");
        }
    }
}