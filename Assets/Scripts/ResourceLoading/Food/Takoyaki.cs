namespace ResourceLoading.Food
{
    public class Takoyaki : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/13/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/13/Bullet");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/13");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/13/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/13/Bullet");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/13");
        }
    }
}