namespace ASiNet.Connector.Attributes;
/// <summary>
/// Пометив класс этим атрибутом вы указываете что этот класс является маршрутизатором.
/// </summary>

[AttributeUsage(AttributeTargets.Class)]
public class RouterAttribute : Attribute
{
    public RouterAttribute(string name)
    {
        RouterName = name;
    }
    public string RouterName { get; set; }
}
