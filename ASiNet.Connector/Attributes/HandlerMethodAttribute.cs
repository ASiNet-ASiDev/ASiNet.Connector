using ASiNet.Connector.Enums;

namespace ASiNet.Connector.Attributes;

/// <summary>
/// Пометив метод этим атрибутом вы указываете что этот метод является методом маршрутизатора.
/// работает только в нутри классов с атрибутом <see cref="HandlerAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HandlerMethodAttribute : Attribute
{
    public HandlerMethodAttribute(string name)
    {
        Name = name;
    }
    public string Name { get; set; }
}
