using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Expr expr = new ApplyExpr(Func.Plus, new ApplyExpr(Func.Multiply, new LiteralExpr(2), new ApplyExpr(Func.Power, new SymbolExpr("x"), new LiteralExpr(2))), new ApplyExpr(Func.Plus, new SymbolExpr("x"),
                new ApplyExpr(Func.Minus, new LiteralExpr(7), new LiteralExpr(2))));
         
            Console.Read();
        }
        private bool Match(Expr e,Expr p,Dictionary<string,Expr> m)
        {
            return false;
        }
    }
}
