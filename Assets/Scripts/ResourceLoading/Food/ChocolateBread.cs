namespace ResourceLoading.Food
{
    public class ChocolateBread : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/38/" + shape);
            GameManager.Instance.GetSprite("Effect/Shield");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/38");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/38/" + shape);
            GameManager.Instance.UnLoadSprite("Effect/Shield");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/38");
        }
    }
}