namespace ResourceLoading.Food
{
    public class MelonShield : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/12/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/12");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/12/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/12");
        }
    }
}