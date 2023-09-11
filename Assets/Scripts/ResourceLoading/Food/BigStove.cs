namespace ResourceLoading.Food
{
    public class BigStove : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/2/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/2");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/2/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/2");
        }
    }
}