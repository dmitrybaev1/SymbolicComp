using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class SymbolVisitor : IExprVisitor<bool>
    {
        readonly string _symbolName;

        public SymbolVisitor(string symbolName)
        {
            _symbolName = symbolName;
        }

        public bool VisitApply(ApplyExpr apply)
        {
            return false;
        }

        public bool VisitLiteral(LiteralExpr literal)
        {
            return false;
        }

        public bool VisitSymbol(SymbolExpr symbol)
        {
            return symbol.Name == _symbolName;
        }

    }

    static class Extensions
    {
        //static readonly IExprVisitor<bool> _isSymbolCheker = new SymbolVisitor();

        public static bool IsSymbol(this Expr expr, string name)
        {
            return expr.Apply(new SymbolVisitor(name));
        }
    }
}
