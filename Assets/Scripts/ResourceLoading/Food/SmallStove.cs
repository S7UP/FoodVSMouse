namespace ResourceLoading.Food
{
    public class SmallStove:IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/0/"+shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/0");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/0/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/0");
        }
    }
}
