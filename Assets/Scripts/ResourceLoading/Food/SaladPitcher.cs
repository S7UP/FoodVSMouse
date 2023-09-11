namespace ResourceLoading.Food
{
    public class SaladPitcher : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/34/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/34/Bullet");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/34");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/34/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/34/Bullet");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/34");
        }
    }
}