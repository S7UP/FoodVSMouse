namespace ResourceLoading.Mouse
{
    public class FlySelfDestructMouse : IMouse
    {
        private int type = 8;

        protected override void O_Load(int shape)
        {
            for (int i = 0; i < MouseManager.GetAttribute(type, shape).mHertRateList.Count; i++)
                GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + type + "/" + shape + "/" + i);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Mouse/" + type);
            GameManager.Instance.GetRuntimeAnimatorController("Mouse/"+type+"/BoomEffect");
        }

        protected override void O_UnLoad(int shape)
        {
            for (int i = 0; i < MouseManager.GetAttribute(type, shape).mHertRateList.Count; i++)
                GameManager.Instance.UnLoadRuntimeAnimatorController("Mouse/" + type + "/" + shape + "/" + i);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Mouse/" + type);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Mouse/"+type+"/BoomEffect");
        }
    }
}