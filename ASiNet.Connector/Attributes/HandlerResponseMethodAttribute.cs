using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Connector.Attributes;
/// <summary>
/// Пометив метод этим атрибутом вы указываете что этот метод является методом маршрутизатора.
/// работает только в нутри классов с атрибутом <see cref="HandlerAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HandlerResponseMethodAttribute : Attribute
{

    public HandlerResponseMethodAttribute(string name)
    {
        Name = name;
    }
    public string Name { get; set; }
}