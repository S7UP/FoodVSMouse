namespace ResourceLoading.Food
{
    public class RaidenBaguette : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/40/" + shape);
            GameManager.Instance.GetSprite("Food/40/lightning");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/40");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/40/" + shape);
            GameManager.Instance.UnLoadSprite("Food/40/lightning");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/40");
        }
    }
}