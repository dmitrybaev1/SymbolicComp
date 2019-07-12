using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{

    static class Extensions
    {
        //static readonly IExprVisitor<bool> _isSymbolCheker = new SymbolVisitor();

        public static bool IsSymbol(this Expr expr, string s)
        {
            return expr.Apply(new SymbolCheckingVisitor(s));
        }

        public static bool TryGetSymbol(this Expr expr, out string s)
        {
            return (s = expr.Apply(SymbolExtractingVisitor.Instance)) != null;
        }

        public static bool TryGetLiteral(this Expr expr, out double x)
        {
            var r = expr.Apply(LiteralExtractingVisitor.Instance);
            x = r.HasValue ? r.Value : default(double);
            return r.HasValue;
        }

        public static bool TryGetApply(this Expr expr, out double x)
        {
            var r = expr.Apply(LiteralExtractingVisitor.Instance);
            x = r.HasValue ? r.Value : default(double);
            return r.HasValue;
        }

        public static bool IsApply(this Expr expr, IExprVisitor<bool> headCond = null, IExprVisitor<bool>[] argsConds = null)
        {
            return expr.Apply(new ApplyCheckingVisitor(headCond, argsConds));
        }

        public static ReadOnlyCollection<Expr> FindApplyArgsExprs(this Expr expr)
        {
            return expr.Apply(new ApplyArgsCollectionVisitor()).Item2;
        }

        public static bool TryDecomposeApply(this Expr expr, out Expr head, out ReadOnlyCollection<Expr> args)
        {
            (head, args) = expr.Apply(new ApplyArgsCollectionVisitor());
            return args != null;
        }

        public static Expr Replace(this Expr expr, Dictionary<Expr, Expr> dict)
        {
            return expr.Apply(new ReplacingVisitor(dict));
        }

        public static bool TryMatch(this Expr expr, Expr pattern, out Dictionary<string, Expr> matches)
        {
            matches = new Dictionary<string, Expr>();

            var visitor = pattern.Apply(new MatcherBuilder(matches));
            var result = expr.Apply(visitor);
            return result;
        }

        public static IEnumerable<T> FlattenLeaves<T>(this T node, Func<T, IEnumerable<T>> childsSelector, Func<T, bool> isLeaf)
        {
            if (isLeaf(node))
                yield return node;

            foreach (var child in childsSelector(node))
                foreach (var item in FlattenLeaves(child, childsSelector, isLeaf))
                    yield return item;
        }
    }
}
