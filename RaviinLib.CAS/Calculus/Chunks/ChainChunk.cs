using System;
using System.Collections.Generic;
using System.Linq;

namespace RaviinLib.CAS
{
    public class ChainChunk: IChunk
    {
        public double Coeff { get; set; }
        public IChunk Chunk { get; set; }
        public IChunk Exp { get; set; }

        private bool? _ExpIsNum = null;
        public bool ExpIsNum { get
            {
                if (_ExpIsNum == null) _ExpIsNum = Exp is BaseChunk b && b.Var == null;
                return (bool)_ExpIsNum;
            } 
        }

        private double? _ExpAsNum = null;
        public double? ExpAsNum
        {
            get
            {
                if (_ExpAsNum == null && ExpIsNum)
                {
                    var exp = Exp as BaseChunk;
                    _ExpAsNum =  Math.Pow(exp.Coeff,exp.Exp);                        
                }
                return _ExpAsNum;
            }
        }

        public ChainChunk(double Coeff, IChunk Chunk, IChunk Exp)
        {
            this.Coeff = Coeff;
            this.Chunk = Chunk;
            this.Exp = Exp;
        }

        public IChunk Derivative(string Var)
        {
            IChunk Chunk1 = Chunk.Derivative(Var);

            if (Exp is BaseChunk b && b.Var == null)
            {
                var exp = Math.Pow(b.Coeff, b.Exp);
                if (exp - 1 == 0)
                {
                    Chunk1.Multiply(Coeff);
                    return Chunk1;
                }

                ChainChunk Chunk2 = new ChainChunk(Coeff * exp, Chunk, new BaseChunk(exp - 1, null, 1));

                if (Chunk1 == null) return Chunk2;

                return new ProductChunk(Chunk1, Chunk2);
            }
            else
            {
                return new ProductChunk(Copy(), new SumChunk(new List<IChunk>() { new ProductChunk(Exp.Derivative(Var), new FuncChunk(Chunk.Copy(), Functions.ln)), new ProductChunk(Exp.Copy(), new ProductChunk(Chunk.Derivative(Var), new ChainChunk(1, Chunk.Copy(), new BaseChunk(-1, null, 1)))) })) { Coeff = Coeff};

                //var ExpCopy = Exp.Copy();
                //ExpCopy.Multiply(Coeff);
                //return new ProductChunk(new ProductChunk(ExpCopy,new ChainChunk(1,Chunk.Copy(),new SumChunk(Exp.Copy(),new BaseChunk(-1, null ,1)))),Chunk1); // d/dx x^x => ( x * (x)^(x-1) ) * (1)
            }
        }

        public override string ToString()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() : string.Empty;
            string exp = Exp is ChainChunk || Exp is SumChunk || Exp is BaseChunk o && o.Exp == 1 ? $"^{Exp}" : Exp is BaseChunk b && b.Var == null ? Math.Pow(b.Coeff, b.Exp) == 1 ? string.Empty : $"^{Exp}" : $"^({Exp})"; // if Exp == 1 => "" | if Exp != 1 => "^number" | if Exp = Chunk => "^(Chunk)"
            string Inner = Chunk is SumChunk ? Chunk.ToString() : $"({Chunk})";
            return $"{coeff}{Inner}{exp}";
        }

        public IChunk Simplified()
        {
            if (Coeff == 0) return null;

            var expSimp = Exp.Simplified();
            if (expSimp == null) return new BaseChunk(1, null, 1);

            if (expSimp is BaseChunk g && g.Var == null)
            {
                var exp = Math.Pow(g.Coeff, g.Exp);

                if (exp == 1)
                {
                    var chunk = Chunk.Copy();
                    chunk.Multiply(Coeff);

                    return chunk.Simplified();
                }

                var simp = Chunk.Simplified();
                if (simp == null) return null;

                if (simp is BaseChunk b)
                {
                    if (b.Var != null) return new BaseChunk(Coeff * Math.Pow(b.Coeff, exp), b.Var, b.Exp * exp);
                    else return new BaseChunk(Coeff * Math.Pow(b.Coeff, exp), null, 1);
                }

                var ReturnVar = new ChainChunk(Coeff * Math.Pow(simp.Coeff, exp), simp, expSimp);
                simp.Coeff = 1;
                return ReturnVar;
            }
            else
            {
                var simp = Chunk.Simplified();
                if (simp == null) return null;

                return new ChainChunk(Coeff, simp, expSimp);
            }
        }

        public IChunk Copy()
        {
            return new ChainChunk(Coeff, Chunk.Copy(), Exp.Copy());
        }

        public void Multiply(double factor)
        {
            Coeff *= factor;
        }
        public void MultiplyExpanded(double factor)
        {
            Multiply(factor);
        }

        public double Subs(Dictionary<string, double> Values)
        {
            return Math.Pow(Chunk.Subs(Values), Exp.Subs(Values)) * Coeff;
        }

        public IChunk Replace(Dictionary<string, IChunk> Values)
        {
            return new ChainChunk(Coeff, Chunk.Replace(Values), Exp.Replace(Values));
        }

        public IChunk Expanded()
        {
            if (Exp is BaseChunk g && g.Var == null)
            {
                return GetExpandedWithBaseExp(g);
            }
            else if (Exp is ChainChunk c)
            {
                var exp = c.Expanded().Simplified();

                if (exp is BaseChunk b) return GetExpandedWithBaseExp(b);

                return new ChainChunk(Coeff, Chunk.Expanded(), exp.Expanded());
            }
            else
            {
                return new ChainChunk(Coeff, Chunk.Expanded(), Exp.Expanded());
            }
        }

        private IChunk GetExpandedWithBaseExp(BaseChunk g)
        {
            var exp = Math.Pow(g.Coeff, g.Exp);

            if (exp < 0)
            {
                return new ChainChunk(Coeff, Chunk.Expanded(), Exp);
            }
            else if (exp == 0) return new BaseChunk(1, null, 1);
            else if (exp == 1)
            {
                var ReturnChunk = Chunk.Copy();
                ReturnChunk.MultiplyExpanded(Coeff);
                return ReturnChunk.Expanded();
            }
            else
            {
                var ChunkExpanded = Chunk.Expanded();
                var ReturnChunk = ChunkExpanded.Copy();
                for (int i = 1; i < exp; i++)
                {
                    ReturnChunk = new ProductChunk(ReturnChunk, ChunkExpanded.Copy());
                }
                ((ProductChunk)ReturnChunk).Chunk2.MultiplyExpanded(Coeff);
                return ReturnChunk;
            }
        }

        public string ToLatex()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() : string.Empty;
            if (coeff == "-1") coeff = "-";
            string exp = Exp is ChainChunk || Exp is SumChunk || Exp is BaseChunk o && o.Exp != 1 ? $"^{{{Exp.ToLatex()}}}" : Exp is BaseChunk b && b.Var == null ? Math.Pow(b.Coeff, b.Exp) == 1 ? string.Empty : $"^{{{Exp.ToLatex()}}}" : $@"^{{\left({Exp.ToLatex()}\right)}}";
                
            string Inner = Chunk is SumChunk ? Chunk.ToLatex() : $@"\left({Chunk.ToLatex()}\right)";
            return $@"{coeff}{Inner}{exp}";
        }

        public IUnit GetUnit(Dictionary<string, IUnit> VariableUnitPairs)
        {
            IUnit Inner = Chunk.GetUnit(VariableUnitPairs);
            var ExpUnit = Exp.GetUnit(VariableUnitPairs).Simplify();

            if (!(ExpUnit is BaseUnit)) throw new Exception($"Can not take a unit^unit: {Inner}^({ExpUnit})");

            if (Exp is BaseChunk b && b.Var == null) return Inner.Exponentiate(Math.Pow(b.Coeff,b.Exp));

            throw new Exception($"Can not evaluate power: {Exp}");
        }

        public IChunk Antiderivative(string Var)
        {
            if (Exp is BaseChunk g && g.Var == null)
            {
                var deriv = Expanded().Simplified()?.Antiderivative(Var) ?? new BaseChunk(0);
                deriv.Multiply(Coeff);
                return deriv;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public List<string> GetVariables()
        {
            return Chunk.GetVariables().Union(Exp.GetVariables()).ToList();
        }
    }
}
