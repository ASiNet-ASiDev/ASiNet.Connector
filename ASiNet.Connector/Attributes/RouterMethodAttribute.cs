namespace ASiNet.Connector.Attributes;

/// <summary>
/// Пометив метод этим атрибутом вы указываете что этот метод является методом маршрутизатора.
/// работает только в нутри классов с атрибутом <see cref="RouterAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RouterMethodAttribute : Attribute
{
    public RouterMethodAttribute(string name)
    {
        Name = name;
    }
    public string Name { get; set; }
}
