namespace ASiNet.Connector.Attributes;
/// <summary>
/// Пометив класс этим атрибутом вы указываете что этот класс является маршрутизатором.
/// </summary>

[AttributeUsage(AttributeTargets.Class)]
public class HandlerAttribute : Attribute
{
    public HandlerAttribute(string name)
    {
        Name = name;
    }
    public string Name { get; set; }
}
