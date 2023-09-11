namespace ResourceLoading.Food
{
    public class BoiledWaterBoom : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/17/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/17/BoomEffect");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/17");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/17/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/17/BoomEffect");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/17");
        }
    }
}