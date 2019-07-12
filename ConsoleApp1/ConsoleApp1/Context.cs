using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class SymbolDefinition
    {
        public SymbolExpr Symbol { get; private set; }

        public Expr ValueOrNull { get; private set; }
        public Dictionary<Expr, Expr> Definitions { get; private set; }
        public HashSet<Expr> Attributes { get; private set; }

        public SymbolDefinition(SymbolExpr symbol)
        {
            this.Symbol = symbol;
            this.Definitions = new Dictionary<Expr, Expr>();
            this.Attributes = new HashSet<Expr>();
        }

        public Expr ApplyTo(ApplyExpr applyExpr)
        {
            var cases = this.Definitions.Select(kv => applyExpr.TryMatch(kv.Key, out var matches)
                                                                 ? (template: kv.Key, value: kv.Value, matches: matches)
                                                                 : (null, null, null))
                            .Where(d => d.value != null)
                            .ToArray();

            if (cases.Length == 1)
            {
                var def = cases.First();

                return def.value.Replace(def.matches.ToDictionary(kv => (Expr)kv.Key, kv => kv.Value));
            }
            else if (cases.Length == 0)
            {
                return applyExpr;
            }
            else
            {
                return KnownExpr.Abort[applyExpr, "Ambiguos match", "Ambiguities".Apply(cases.Select(c => c.template).ToArray())];
            }
        }
    }

    class Context : IExprVisitor<Expr>
    {
        public const int IterationsLimit = 1000000;
        public const int RecursionLimit = 500;

        public Dictionary<SymbolExpr, SymbolDefinition> KnownSymbols { get; private set; }

        int _recursionDepth = 0;

        public Context()
        {
            this.KnownSymbols = new Dictionary<SymbolExpr, SymbolDefinition>();
        }

        public Expr Evaluate(Expr expr)
        {
            _recursionDepth = 0;
            return this.EvaluateImpl(expr);
        }

        Expr EvaluateImpl(Expr expr)
        {
            _recursionDepth++;
            if (_recursionDepth > RecursionLimit)
                return KnownExpr.Abort["Recursion limit hit", expr];

            Expr result = expr.Apply(this);

            var count = 0;
            while (!this.IsAbortExpr(result) && !expr.Equals(result))
            {
                if (count > IterationsLimit)
                {
                    result = KnownExpr.Abort["Iterations limit hit", result];
                    break;
                }

                count++;
                expr = result;
                result = expr.Apply(this);
            }

            _recursionDepth--;
            return result;
        }

        private bool IsAbortExpr(Expr expr)
        {
            return expr.TryDecomposeApply(out var head, out var args) ? head.Equals(KnownExpr.Abort) : false;
        }

        Expr IExprVisitor<Expr>.VisitApply(ApplyExpr apply)
        {
            Expr newHead = this.EvaluateImpl(apply.Head);

            var args = apply.Args.ToArray();

            Expr epxr;
            if (newHead is SymbolExpr headSymbol && this.KnownSymbols.TryGetValue(headSymbol, out var def))
            {
                if (def.Attributes.Contains(KnownExpr.HoldAll))
                    args = args.Select(this.EvaluateImpl).ToArray();
                else if (def.Attributes.Contains(KnownExpr.HoldFirst))
                    args = args.Take(1).Concat(args.Skip(1).Select(this.EvaluateImpl)).ToArray();
                else if (def.Attributes.Contains(KnownExpr.HoldRest))
                    args = args.Take(1).Select(this.EvaluateImpl).Concat(args.Skip(1)).ToArray();

                if (def.Attributes.Contains(KnownExpr.Flat))
                    args = args.SelectMany(a => a.FlattenLeaves(e => e.FindApplyArgsExprs() ?? new Expr[0], e => !e.IsApply())).ToArray();
                if (def.Attributes.Contains(KnownExpr.Orderless))
                    Array.Sort(args);

                // TODO: listable

                epxr = def.ApplyTo(new ApplyExpr(newHead, args));
                if (!epxr.Equals(apply))
                    return epxr;
            }
            else
            {
                epxr = apply;
            }

            // apply builtin functions

            throw new NotImplementedException();
        }

        Expr IExprVisitor<Expr>.VisitLiteral(LiteralExpr literal)
        {
            return literal;
        }

        Expr IExprVisitor<Expr>.VisitSymbol(SymbolExpr symbol)
        {
            if (KnownSymbols.TryGetValue(symbol, out var def) && def.ValueOrNull != null)
            {
                return def.ValueOrNull;
            }
            else
            {
                return symbol;
            }
        }
    }
}
