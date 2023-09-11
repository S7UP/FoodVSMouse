namespace ResourceLoading.Food
{
    public class CupLight : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/1/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/1");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/1/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/1");
        }
    }
}