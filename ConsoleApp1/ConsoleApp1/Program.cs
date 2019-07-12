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


            Expr power = "Power", mul = "Mul", sum = "Sum";
            Expr x = "x";
            Expr pattern = "Pattern";

            Expr expr = sum[power[x, 2], mul[4, x], 1];
            Expr template = sum[power[pattern["t"], 2], mul[pattern["b"], pattern["t"]], pattern["c"]];

            Expr f = "f", m = "m", a = "a";

            {
                { f[pattern[m],pattern[a]], mul[m,a]  }
            };
            Expr input = f[3, 10];

            //var value = mul[m, a];
            //var t2 = value.Replace(new Dictionary<Expr, Expr> { { m, 3 },{ a, 10} });

            //Console.WriteLine(value);
            //Console.WriteLine(t2);
            //Console.ReadKey();
            //return;

            foreach (var kv in defs)
            {
                Dictionary<string, Expr> matches = null;
                Dictionary<Expr, Expr> replacements = new Dictionary<Expr, Expr>();
                if (input.TryMatch(kv.Key, out matches))
                {
                    foreach (var match in matches)
                    {
                        Console.WriteLine($"{match.Key} - {match.Value}");
                        replacements.Add(match.Key, match.Value);
                        //Console.WriteLine(t);
                    }
                    var t = kv.Value.Replace(replacements);
                    Console.WriteLine(t);
                }
            }
            //if (TryMatch(null, null, out var matches))
            //{
            //}

            //var match = Regex.Match("input", "pattern");
            //if (match.Success)
            //{
            //    var rmatches = match.Groups;

            //}




            Console.Read();
        }
    }
}

namespace System
{
    public struct ValueTuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public ValueTuple(T1 a, T2 b)
        {
            this.Item1 = a;
            this.Item2 = b;
        }
    }

    public struct ValueTuple<T1, T2, T3>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;

        public ValueTuple(T1 a, T2 b, T3 c)
        {
            this.Item1 = a;
            this.Item2 = b;
            this.Item3 = c;
        }
    }
}
