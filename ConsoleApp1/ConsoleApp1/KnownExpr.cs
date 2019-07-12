using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class KnownExpr
    {
        private static readonly SymbolExpr hold, all, rest, first, listable, orderless, flat, abort;

        static KnownExpr()
        {
            typeof(KnownExpr).GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                             .Where(f => f.FieldType == typeof(SymbolExpr))
                             .ToList()
                             .ForEach(f => f.SetValue(null, new SymbolExpr(MakeName(f.Name))));
        }

        private static string MakeName(string name)
        {
            return char.ToUpper(name.First()) + name.Substring(1);
        }

        public static readonly Expr HoldAll = hold[all];
        public static readonly Expr HoldFirst = hold[first];
        public static readonly Expr HoldRest = hold[rest];
        public static readonly Expr Flat = flat;
        public static readonly Expr Orderless = orderless;
        public static readonly Expr Listable = listable;
        public static readonly Expr Abort = abort;
    }
}
