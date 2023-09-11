namespace ResourceLoading.Food
{
    public class PokerShield : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/33/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/33");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/33/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/33");
        }
    }
}