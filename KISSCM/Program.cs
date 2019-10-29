using System.Linq;

namespace KISS
{
    class Program
    {
        static int Main(string[] args)
        {
            //dotnet .\KISSCM.dll /f:Kissprops.xml

            if (args.Length == 0)
            {
                args = new [] {"kissprops.xml"};
            }

            return ArgumentHelper.ProcessScripts(args[0]) ? 0 : 400;
        }
    }
}