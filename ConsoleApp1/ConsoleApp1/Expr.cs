using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public interface IExprVisitor<T>
    {
        T VisitApply(ApplyExpr apply);
        T VisitLiteral(LiteralExpr literal);
        T VisitSymbol(SymbolExpr symbol);
    }

    public abstract class Expr
    {
        public T Apply<T>(IExprVisitor<T> visitor)
        {
            return this.ApplyImpl(visitor);
        }

        protected abstract T ApplyImpl<T>(IExprVisitor<T> visitor);
    }

    public class ApplyExpr : Expr
    {
        public Expr Head { get; private set; }
        public ReadOnlyCollection<Expr> Args { get; private set; }

        public ApplyExpr(Expr head, params Expr[] args)
            : this(head, (IEnumerable<Expr>)args) { }

        public ApplyExpr(Expr head, IEnumerable<Expr> args)
        {
            this.Head = head;
            this.Args = new ReadOnlyCollection<Expr>(args.ToArray());
        }

        protected override T ApplyImpl<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitApply(this);
        }
    }

    public class LiteralExpr : Expr
    {
        public double Value { get; private set; }

        public LiteralExpr(double value)
        {
            this.Value = value;
        }

        protected override T ApplyImpl<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }

    public class SymbolExpr : Expr
    {
        public string Name { get; private set; }

        public SymbolExpr(string name)
        {
            this.Name = name;
        }

        protected override T ApplyImpl<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitSymbol(this);
        }
    }
}
