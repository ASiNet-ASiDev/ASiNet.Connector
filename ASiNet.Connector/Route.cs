using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Connector;
public record Route(string ControllerName, string MethodName, int id)
{
    public static Route FromPath(string path)
    {
        var result = path.Split('/');
        if(result.Length == 2)
            return new(result[0], result[1], -1);
        else if(result.Length == 3)
            return new(result[1], result[2], int.Parse(result[0]));
        else
            throw new InvalidOperationException("invalid path");
    }

    public static string ToPath(Route route) => $"{route.id}/{route.ControllerName}/{route.MethodName}";

    public static Route Default => new(string.Empty, string.Empty, -1);
}
