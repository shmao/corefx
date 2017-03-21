using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class SRFormat
{
    public static string Format(params object[] objects)
    {
        return objects[0].ToString();
    }
}

