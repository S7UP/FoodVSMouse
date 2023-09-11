namespace ResourceLoading.Food
{
    public class CokeBoom : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/16/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/16/BoomEffect");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/16");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/16/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/16/BoomEffect");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/16");
        }
    }
}