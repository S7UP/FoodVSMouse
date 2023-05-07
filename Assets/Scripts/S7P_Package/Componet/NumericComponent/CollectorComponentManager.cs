using S7P.Component;

public class CollectorComponentManager
{
    public static IntergerCollectorComponent GetIntergerCollectorComponent(ComponentController controller)
    {
        IComponent c;
        if(controller.TryGet(IntergerCollectorComponent.ComponentKey, out c))
        {
            return c as IntergerCollectorComponent;
        }
        else
        {
            c = new IntergerCollectorComponent();
            controller.Add(IntergerCollectorComponent.ComponentKey, c);
            return c as IntergerCollectorComponent;
        }
    }

    public static FloatCollectorComponent GetFloatCollectorComponent(ComponentController controller)
    {
        IComponent c;
        if (controller.TryGet(FloatCollectorComponent.ComponentKey, out c))
        {
            return c as FloatCollectorComponent;
        }
        else
        {
            c = new FloatCollectorComponent();
            controller.Add(FloatCollectorComponent.ComponentKey, c);
            return c as FloatCollectorComponent;
        }
    }
}
