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
            //IChunk Chunk1 = Chunk.Derivative(Var);

            //if (Chunk1 is BaseChunk cb && cb.Var == null && cb.Exp == 1 && cb.Coeff == 0)
            //{
            //    if (Exp is BaseChunk eb && eb.Var == null && Math.Pow(eb.Coeff, eb.Exp) == 0) return new BaseChunk(Coeff);
            //    return new BaseChunk(0);
            //}

            if (Exp.IsNumber())
            {
                var b = Exp as BaseChunk;
                var exp = Math.Pow(b.Coeff, b.Exp);

                //if (exp - 1 == 0)
                //{
                //    Chunk1.Multiply(Coeff);
                //    return Chunk1;
                //}

                IChunk Chunk2 = Chunker.Chain(Coeff * exp, Chunk, new BaseChunk(exp - 1));

                //if (Chunk1 == null) return Chunk2;

                return Chunker.Product(Chunk.Derivative(Var), Chunk2);
            }
            else
            {
                return Chunker.Product(Copy(), Chunker.Sum(Chunker.Product(Exp.Derivative(Var), Chunker.Func(Chunk.Copy(), Functions.ln)), Chunker.Product(Exp.Copy(), Chunker.Product(Chunk.Derivative(Var), Chunker.Chain(1, Chunk.Copy(), new BaseChunk(-1)))) ),Coeff);

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
            // 0(f)^n => 0
            if (Coeff == 0) return new BaseChunk(0);

            var expSimp = Exp.Simplified();

            // n(f)^0 => n
            if (expSimp.IsZero()) return new BaseChunk(Coeff);

            var simp = Chunk.Simplified();
            if (simp.IsZero()) new BaseChunk(0);

            // n(f)^m
            if (expSimp.IsNumber())
            {
                var exp = (expSimp as BaseChunk).AsNumber();

                // n(f)^1 => nf
                if (exp == 1)
                {
                    return Chunk.MultiplyBy(Coeff).Simplified();
                }

                // n(b)^m
                if (simp is BaseChunk b)
                {
                    // n(c)^m => nc^m
                    if (b.IsNumber()) return new BaseChunk(Coeff * Math.Pow(b.AsNumber(), exp));
                    // n(x)^m => nx^m
                    else 
                    {
                        //return Chunker.Base(Coeff * Math.Pow(b.Coeff, exp), b.Var, b.Exp * exp);

                        var bCopy = b.Copy() as BaseChunk;
                        bCopy.Exp *= exp;
                        bCopy.Coeff = Math.Pow(bCopy.Coeff, exp) * Coeff;

                        return bCopy;
                    }
                }

                // n(f)^m => n(f)^m
                var retCoef = Coeff * Math.Pow(simp.Coeff, exp);
                simp.Coeff = 1;
                var ReturnVar = Chunker.Chain(retCoef, simp, new BaseChunk(exp));
                return ReturnVar;
            }
            else
            {
                

                return Chunker.Chain(Coeff, simp, expSimp);
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
        

        public IChunk MultiplyBy(double factor)
        {
            IChunk NewChunk = Copy();
            NewChunk.Multiply(factor);
            return NewChunk;
        }

        public double Subs(Dictionary<string, double> Values)
        {
            return Math.Pow(Chunk.Subs(Values), Exp.Subs(Values)) * Coeff;
        }

        public IChunk Replace(Dictionary<string, IChunk> Values)
        {
            return Chunker.Chain(Coeff, Chunk.Replace(Values), Exp.Replace(Values));
        }

        public IChunk Expanded()
        {
            if (Exp.IsNumber())
            {
                double expNum = (Exp as BaseChunk).AsNumber();
                double LoopCount = Math.Abs(expNum);
                if (LoopCount % 1 != 0) return Chunker.Chain(Coeff, Chunk.Expanded(), Exp.Expanded());


                IChunk copy = Chunk.Expanded();

                List<IChunk> retChunks = new List<IChunk>();
                for (int i = 0; i < LoopCount; i++)
                {
                    retChunks.Add(copy.Copy());
                }
                if (expNum < 0) return Chunker.Chain(1,Chunker.Product(retChunks, Coeff),new BaseChunk(-1));
                else return Chunker.Product(retChunks, Coeff);
            }
            return Chunker.Chain(Coeff, Chunk.Expanded(), Exp.Expanded());


            //if (Exp is BaseChunk g && g.Var == null)
            //{
            //    return GetExpandedWithBaseExp(g);
            //}
            //else if (Exp is ChainChunk c)
            //{
            //    var exp = c.Expanded().Simplified();

            //    if (exp is BaseChunk b) return GetExpandedWithBaseExp(b);

            //    return Chunker.Chain(Coeff, Chunk.Expanded(), exp.Expanded());
            //}
            //else
            //{
            //    return Chunker.Chain(Coeff, Chunk.Expanded(), Exp.Expanded());
            //}
        }

        //private IChunk GetExpandedWithBaseExp(BaseChunk g)
        //{
        //    var exp = Math.Pow(g.Coeff, g.Exp);

        //    if (exp < 0)
        //    {
        //        return Chunker.Chain(Coeff, Chunk.Expanded(), Exp);
        //    }
        //    else if (exp == 0) return new BaseChunk(1);
        //    else if (exp == 1)
        //    {
        //        var ReturnChunk = Chunk.Copy();
        //        ReturnChunk.MultiplyExpanded(Coeff);
        //        return ReturnChunk.Expanded();
        //    }
        //    else
        //    {
        //        var ChunkExpanded = Chunk.Expanded();
        //        var ReturnChunk = ChunkExpanded.Copy();
        //        for (int i = 1; i < exp; i++)
        //        {
        //            ReturnChunk = Chunker.Product(ReturnChunk, ChunkExpanded.Copy());
        //        }
        //        ((ProductChunk)ReturnChunk).Chunk2.MultiplyExpanded(Coeff);
        //        return ReturnChunk;
        //    }
        //}

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

        public string ToCode()
        {
            return $"new ChainChunk({Coeff},{Chunk.ToCode()},{Exp.ToCode()})";
        }

        public bool IsOne()
        {
            return false;
        }

        public bool IsZero()
        {
            return false;
        }

        public bool IsConstant()
        {
            return Chunk.IsConstant();
        }

        public bool IsNumber()
        {
            return false;
        }

        public bool IsSimpleNumber()
        {
            return false;
        }
    }
}
