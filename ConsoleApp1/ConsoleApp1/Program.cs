using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Expr> list = new List<Expr>();
            list.Add(new SymbolExpr("s"));
            list.Add(new LiteralExpr(2));
            foreach(Expr e in list)
            {
                if(e.IsSymbol("s"))
                {
                    Console.WriteLine("there is");
                }
            }
            Console.Read();
        }
    }
}
