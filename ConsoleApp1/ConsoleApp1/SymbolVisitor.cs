using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class SymbolCheckingVisitor : IExprVisitor<bool>
    {
        readonly string _symbolName;

        public SymbolCheckingVisitor(string symbolName)
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

    class SymbolExtractingVisitor : IExprVisitor<string>
    {
        public static readonly SymbolExtractingVisitor Instance = new SymbolExtractingVisitor();

        private SymbolExtractingVisitor() { }

        public string VisitApply(ApplyExpr apply)
        {
            return null;
        }

        public string VisitLiteral(LiteralExpr literal)
        {
            return null;
        }

        public string VisitSymbol(SymbolExpr symbol)
        {
            return symbol.Name;
        }
    }

    class LiteralExtractingVisitor : IExprVisitor<double?>
    {
        public static readonly LiteralExtractingVisitor Instance = new LiteralExtractingVisitor();

        private LiteralExtractingVisitor() { }

        public double? VisitApply(ApplyExpr apply)
        {
            return null;
        }

        public double? VisitLiteral(LiteralExpr literal)
        {
            return literal.Value;
        }

        public double? VisitSymbol(SymbolExpr symbol)
        {
            return null;
        }
    }

    class LiteralCheckingVisitor : IExprVisitor<bool>
    {
        readonly double _literal;

        public LiteralCheckingVisitor(double literal)
        {
            _literal = literal;
        }
        public bool VisitApply(ApplyExpr apply)
        {
            return false;
        }

        public bool VisitLiteral(LiteralExpr literal)
        {
            return literal.Value == _literal;
        }

        public bool VisitSymbol(SymbolExpr symbol)
        {
            return false;
        }
    }

    class ApplyCheckingVisitor : IExprVisitor<bool>
    {
        readonly IExprVisitor<bool> _headCondition;
        readonly IExprVisitor<bool>[] _argsConds;

        public ApplyCheckingVisitor(IExprVisitor<bool> headCondition, IExprVisitor<bool>[] argsConds)
        {
            _headCondition = headCondition;
            _argsConds = argsConds;
        }

        public bool VisitApply(ApplyExpr apply)
        {
            bool isOk = _headCondition == null || apply.Head.Apply(_headCondition);

            if (_argsConds != null)
            {
                if (apply.Args.Count == _argsConds.Length)
                {
                    for (int i = 0; i < _argsConds.Length; i++)
                    {
                        isOk &= apply.Args[i].Apply(_argsConds[i]);
                    }
                }
                else
                {
                    isOk = false;
                }
            }

            return isOk;
        }

        public bool VisitLiteral(LiteralExpr literal)
        {
            return false;
        }

        public bool VisitSymbol(SymbolExpr symbol)
        {
            return false;
        }
    }

    class CapturingVisitor : IExprVisitor<bool>
    {
        private Dictionary<string, Expr> _matches;
        private string _captureName;

        public CapturingVisitor(Dictionary<string, Expr> matches, string captureName)
        {
            _matches = matches;
            _captureName = captureName;
        }

        private bool CaptureExpr(Expr expr)
        {
            if (_matches.TryGetValue(_captureName, out var alreadyCapturedExpr))
            {
                return expr.CompareTo(alreadyCapturedExpr) == 0;
            }
            else
            {
                _matches.Add(_captureName, expr);
                return true;
            }
        }

        public bool VisitApply(ApplyExpr apply)
        {
            return this.CaptureExpr(apply);
        }

        public bool VisitLiteral(LiteralExpr literal)
        {
            return this.CaptureExpr(literal);
        }

        public bool VisitSymbol(SymbolExpr symbol)
        {
            return this.CaptureExpr(symbol);
        }
    }

    class MatcherBuilder : IExprVisitor<IExprVisitor<bool>>
    {
        private Dictionary<string, Expr> matches;

        public MatcherBuilder(Dictionary<string, Expr> matches)
        {
            this.matches = matches;
        }

        public IExprVisitor<bool> VisitApply(ApplyExpr apply)
        {
            if (apply.Head.IsSymbol("Pattern"))
            {
                if (apply.Args.Count < 1 || !apply.Args[0].TryGetSymbol(out var name))
                    throw new InvalidOperationException("Invalid pattern");

                return new CapturingVisitor(matches, name);
            }
            else
            {
                return new ApplyCheckingVisitor(apply.Head.Apply(this), apply.Args.Select(a => a.Apply(this)).ToArray());
            }
        }

        public IExprVisitor<bool> VisitLiteral(LiteralExpr literal)
        {
            return new LiteralCheckingVisitor(literal.Value);
        }

        public IExprVisitor<bool> VisitSymbol(SymbolExpr symbol)
        {
            return new SymbolCheckingVisitor(symbol.Name);
        }
    }

    class ReplacingVisitor : IExprVisitor<Expr>
    {
        readonly Dictionary<Expr, Expr> _dict;

        public ReplacingVisitor(Dictionary<Expr, Expr> dict)
        {
            _dict = dict;
        }

        private Expr ReplaceImpl(Expr expr)
        {
            return _dict.TryGetValue(expr, out var newExpr) ? newExpr : expr;
        }

        public Expr VisitApply(ApplyExpr apply)
        {
            if (_dict.TryGetValue(apply, out var newExpr))
                return newExpr;

            List<Expr> exprs = new List<Expr>();
            foreach (Expr e in apply.Args)
            {
                exprs.Add(e.Apply(this));
            }
            return new ApplyExpr(apply.Head.Apply(this), exprs);
        }

        public Expr VisitLiteral(LiteralExpr literal)
        {
            return _dict.TryGetValue(literal, out var newExpr) ? newExpr : literal;
        }

        public Expr VisitSymbol(SymbolExpr symbol)
        {
            return _dict.TryGetValue(symbol, out var newExpr) ? newExpr : symbol;
        }
    }

    class ApplyArgsCollectionVisitor : IExprVisitor<(Expr, ReadOnlyCollection<Expr>)>
    {
        (Expr, ReadOnlyCollection<Expr>) IExprVisitor<(Expr, ReadOnlyCollection<Expr>)>.VisitApply(ApplyExpr apply)
        {
            return (apply.Head, apply.Args);
        }

        (Expr, ReadOnlyCollection<Expr>) IExprVisitor<(Expr, ReadOnlyCollection<Expr>)>.VisitLiteral(LiteralExpr literal)
        {
            return (null, null);
        }

        (Expr, ReadOnlyCollection<Expr>) IExprVisitor<(Expr, ReadOnlyCollection<Expr>)>.VisitSymbol(SymbolExpr symbol)
        {
            return (null, null);
        }
    }

}
