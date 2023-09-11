namespace ResourceLoading.Food
{
    public class SugarGourd : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/30/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Bullet/30/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/30");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/30/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Bullet/30/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/30");
        }
    }
}