namespace ResourceLoading.Food
{
    public class WaterPipeFoodUnit : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/6/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/6/Bullet"); // ×Óµ¯
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/6");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/6/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/6/Bullet"); // ×Óµ¯
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/6");
        }
    }
}