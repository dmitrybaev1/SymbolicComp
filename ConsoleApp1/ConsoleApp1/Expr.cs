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

    public static class ExprExtensions
    {
        public static Expr Apply(this string sym, params Expr[] args)
        {
            return new ApplyExpr(new SymbolExpr(sym), args);
        }
    }

    public abstract class Expr : IComparable<Expr>
    {
        public T Apply<T>(IExprVisitor<T> visitor)
        {
            return this.ApplyImpl(visitor);
        }

        protected abstract T ApplyImpl<T>(IExprVisitor<T> visitor);

        public override string ToString()
        {
            return this.Apply(SimpleExprPrinter.Instance);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is Expr other) ? this.CompareTo(other) == 0 : false;
        }

        public int CompareTo(Expr other)
        {
            return this.CompareToImpl(other);
        }

        protected abstract int CompareToImpl(Expr other);

        public static implicit operator Expr(string s)
        {
            return new SymbolExpr(s);
        }

        public static implicit operator Expr(double x)
        {
            return new LiteralExpr(x);
        }

        public Expr this[params Expr[] args]
        {
            get { return new ApplyExpr(this, args); }
        }
    }

    public class ApplyExpr : Expr
    {
        public Expr Head { get; private set; }
        public ReadOnlyCollection<Expr> Args { get; private set; }

        public ApplyExpr(Expr head, params Expr[] args)
            : this(head, (IEnumerable<Expr>)args) { }

        public ApplyExpr(Expr head, IEnumerable<Expr> args)
        {
            if (head == null || args == null || args.Any(a => a == null))
                throw new ArgumentNullException();

            this.Head = head;
            this.Args = new ReadOnlyCollection<Expr>(args.ToArray());
        }

        protected override T ApplyImpl<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitApply(this);
        }

        protected override int CompareToImpl(Expr other)
        {
            if (other.TryDecomposeApply(out var otherHead, out var otherArgs))
            {
                var r = this.Head.CompareTo(otherHead);
                if (r != 0)
                    return r;

                r = this.Args.Count.CompareTo(otherArgs.Count);
                if (r != 0)
                    return r;

                for (int i = 0; i < this.Args.Count; i++)
                {
                    r = this.Args[i].CompareTo(otherArgs[i]);
                    if (r != 0)
                        return r;
                }

                return 0;
            }
            else
            {
                return 1;
            }
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

        protected override int CompareToImpl(Expr other)
        {
            return other.TryGetLiteral(out var otherValue) ? this.Value.CompareTo(otherValue) : -1;
        }
    }

    public class SymbolExpr : Expr
    {
        public string Name { get; private set; }

        public SymbolExpr(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException();

            this.Name = name;
        }

        protected override T ApplyImpl<T>(IExprVisitor<T> visitor)
        {
            return visitor.VisitSymbol(this);
        }

        protected override int CompareToImpl(Expr other)
        {
            if (other.TryGetSymbol(out var otherName))
                return this.Name.CompareTo(otherName);

            return other.IsApply() ? -1 : 1;
        }
    }

    class SimpleExprPrinter : IExprVisitor<string>
    {
        public static readonly SimpleExprPrinter Instance = new SimpleExprPrinter();

        string IExprVisitor<string>.VisitApply(ApplyExpr apply)
        {
            return apply.Head.Apply(this) + "[" + string.Join(", ", apply.Args.Select(a => a.Apply(this))) + "]";
        }

        string IExprVisitor<string>.VisitLiteral(LiteralExpr literal)
        {
            return literal.Value.ToString();
        }

        string IExprVisitor<string>.VisitSymbol(SymbolExpr symbol)
        {
            return symbol.Name;
        }
    }
}
