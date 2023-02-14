using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Connector;
public record Route(string ControllerName, string MethodName)
{
    public static Route FromPath(string path)
    {
        var result = path.Split('/');
        return new(result[0], result[1]);
    }

    public static string ToPath(Route route) => $"{route.ControllerName}/{route.MethodName}";
}
