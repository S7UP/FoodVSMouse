/// <summary>
/// 在战斗场景中建造老鼠的工厂
/// </summary>
public class MouseFactory
{

    public MouseUnit GetMouse(int type, int shape)
    {
        MouseUnit m = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Mouse/" + type).GetComponent<MouseUnit>();
        m.mType = type;
        m.mShape = shape;
        m.MInit();
        m.SetAttribute(MouseManager.GetAttribute(type, shape));
        return m;
    }
}
