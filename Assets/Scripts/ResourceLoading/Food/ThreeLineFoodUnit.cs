namespace ResourceLoading.Food
{
    public class ThreeLineFoodUnit : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/7/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/7/Bullet");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/7");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/7/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/7/Bullet");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/7");
        }
    }
}