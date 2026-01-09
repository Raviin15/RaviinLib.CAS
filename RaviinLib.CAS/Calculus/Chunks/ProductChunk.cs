using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics;

namespace RaviinLib.CAS
{
    public class ProductChunk : IChunk
    {
        public double Coeff { get; set; } = 1;
        public IChunk Chunk1 { get; set; }
        public IChunk Chunk2 { get; set; }

        public ProductChunk(IChunk Chunk1, IChunk Chunk2)
        {
            this.Chunk1 = Chunk1;
            this.Chunk2 = Chunk2;
        }

        public override string ToString()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() + "(" : string.Empty;
            if (coeff == "-1(") coeff = "-(";
            return $"{coeff}{Chunk1} * {Chunk2}" + (coeff == string.Empty ? "" : ")");
        }

        public IChunk Simplified()
        {
            IChunk a = Chunk1?.Simplified();
            IChunk b = Chunk2?.Simplified();

            if (a == null || b == null)
            {
                return null;
            }



            double coeff = Coeff;
            if (a is BaseChunk j && j.Var == null && b is BaseChunk k && k.Var == null)
            {
                var jVal = Math.Pow(j.Coeff,j.Exp);
                var kVal = Math.Pow(k.Coeff, k.Exp);
                return new BaseChunk(jVal*kVal,null,1);

            }
            else if (a is BaseChunk n && n.Var == null)
            {
                b.Multiply(Math.Pow(n.Coeff,n.Exp) * coeff);
                //b.Coeff *= n.Coeff * coeff;
                return b;
            }
            else if (b is BaseChunk m && m.Var == null)
            {
                a.Multiply(Math.Pow(m.Coeff, m.Exp) * coeff);
                return a;
            }
            else if (a is BaseChunk o && b is BaseChunk p && o.Var == p.Var)
            {
                var tmpExp = o.Exp + p.Exp;
                var tmpCoeff = coeff * o.Coeff * p.Coeff;
                if (tmpExp == 0) return new BaseChunk(tmpCoeff,null,1);

                return new BaseChunk(tmpCoeff, o.Var, tmpExp);
            }

            if (a is ProductChunk || b is ProductChunk || new IChunkComparerIgnoreCoeffExp().Equals(a, b))
            {
                return CascadeSimplify(new ProductChunk(a,b));
            }

            coeff *= a.Coeff * b.Coeff;
            a.Coeff = 1;
            b.Coeff = 1;
            var ReturnVar = new ProductChunk(a, b) { Coeff = coeff };

            return ReturnVar;

        }

        private IChunk CascadeSimplify(ProductChunk Chunk)
        {

            (IEnumerable<IChunk> ProductChunks, double CascadeCoef) = CascadeGetChunks(Chunk);

            //Simplify BaseChunks
            IEnumerable<BaseChunk> Base =
                ProductChunks.OfType<BaseChunk>()
                .GroupBy(c => c.Var)
                .Select(group => group.Aggregate((a, b) => a.Exp + b.Exp == 0 ? new BaseChunk(a.Coeff * b.Coeff) : new BaseChunk(a.Coeff * b.Coeff, a.Var, a.Exp + b.Exp))).Where(c => !(c.Var == null && c.Coeff == 1));

            var ChainGroup = ProductChunks
                .OfType<ChainChunk>()
                .GroupBy(c => c, new IChunkComparerIgnoreCoeffExp());

            List<IChunk> Chain = new List<IChunk>();
            foreach (var group in ChainGroup)
            {
                var Exp = new SumChunk(group.Select(c => c.Exp).ToList()).Simplified();
                var Coeff = group.Select(c => c.Coeff).Aggregate((a, b) => a * b);
                if (Exp != null) Chain.Add(new ChainChunk(Coeff, ((ChainChunk)group.Key).Chunk.Copy(),Exp));
            }
            //.Select(g => { var Chain = (ChainChunk)(g.Key.Copy()); Chain.Coeff = g.Select(c => c.Coeff).Aggregate((a, b) => a * b); Chain.Exp = new SumChunk(g.Cast<IChunk>().ToList()).Simplified() ?? new BaseChunk(0, null, 1); return Chain; })
            //.Where(c => c.Exp is BaseChunk b && b.Var == null && b.Coeff != 0); //Chain.Exp = g.Cast<ChainChunk>().Sum(c => c.Exp)

            //IEnumerable<IChunk> CombiningLikeExp = SimplifiedBaseChunk
            //    .GroupBy(c => c.Exp)
            //    .Select(group =>
            //    {

            //        if (group.Count() > 1)
            //        {
            //            group.ToList().ForEach(c => { Chunk.Coeff *= c.Coeff; c.Coeff = 1; c.Exp = 1; });

            //            var Aggregate = group.Cast<IChunk>().Aggregate((a, b) => new ProductChunk(a, b));

            //            return (IChunk)new ChainChunk(Chunk.Coeff, Aggregate, group.Key); ;
            //        }
            //        else
            //        {
            //            return (IChunk)group.First();
            //        }
            //    });

            var FuncGroup = ProductChunks
                .OfType<FuncChunk>()
                .GroupBy(c => c, new IChunkComparerIgnoreCoeffExp());

            List<IChunk> Func = new List<IChunk>();
            foreach (var group in FuncGroup)
            {
                IChunk Exp = new BaseChunk(group.Count(),null,1);
                var Coeff = group.Select(c => c.Coeff).Aggregate((a, b) => a * b);
                var Key = (FuncChunk)group.Key;
                Func.Add(new ChainChunk(Coeff,new FuncChunk(Key.Chunk.Copy(), Key.Function) { SecondChunk = Key.SecondChunk?.Copy() }, Exp));
            }

            //Combine Simplified BaseChunks and non-BaseChunks
            ProductChunks = ProductChunks.Where(c => !(c is BaseChunk)).Concat(Base); //CombiningLikeExp
            ProductChunks = ProductChunks.Where(c => !(c is ChainChunk)).Concat(Chain); //CombiningLikeExp
            ProductChunks = ProductChunks.Where(c => !(c is FuncChunk)).Concat(Func); //CombiningLikeExp

            var ReturnCoef = Chunk.Coeff * ProductChunks.Select(c => c.Coeff).Aggregate((a, b) => a * b);
            ProductChunks = ProductChunks.Select(c => { c.Coeff = 1; return c; });
            //Combine back into ProductChunk chain
            var Aggregate = ProductChunks.Aggregate((a, b) => new ProductChunk(a, b));
            Aggregate.Coeff = ReturnCoef * CascadeCoef;
            return Aggregate;


        }
        private static (IEnumerable<IChunk> Chunks, double Coeff) CascadeGetChunks(ProductChunk Chunk)
        {
            double returnCoeff = Chunk.Coeff;

            List<IChunk> ProductChunks = new List<IChunk>();
            if (Chunk.Chunk1 is ProductChunk c1)
            {
                var ret = CascadeGetChunks(c1);
                ProductChunks.AddRange(ret.Chunks);
                returnCoeff *= ret.Coeff;
            }
            else
            {
                var Simp = Chunk.Chunk1.Simplified();
                if (Simp != null) ProductChunks.Add(Simp);
            }

            if (Chunk.Chunk2 is ProductChunk c2)
            {
                var ret = CascadeGetChunks(c2);
                ProductChunks.AddRange(ret.Chunks);
                returnCoeff *= ret.Coeff;
            }
            else
            {
                var Simp = Chunk.Chunk2.Simplified();
                if (Simp != null) ProductChunks.Add(Simp);
            }
            return (ProductChunks, returnCoeff);
        }


        public IChunk Derivative(string Var)
        {
            List<IChunk> Chunks = new List<IChunk>();

            var Chunk1Derivative = Chunk1.Derivative(Var);
            var Chunk2Derivative = Chunk2.Derivative(Var);

            if (
                (Chunk1Derivative is BaseChunk cb1 && cb1.Var == null && cb1.Exp == 1 && cb1.Coeff == 0) 
                ||
                (Chunk2Derivative is BaseChunk cb2 && cb2.Var == null && cb2.Exp == 1 && cb2.Coeff == 0)
                ) return new BaseChunk(0);

            if (Chunk1Derivative is BaseChunk b && b.Var == null && b.Exp == 1)
            {
                if (b.Coeff == 1) Chunks.Add(Chunk2);
                else if (b.Coeff != 0) Chunks.Add(new ProductChunk(Chunk1Derivative, Chunk2));
            }
            else Chunks.Add(new ProductChunk(Chunk1Derivative, Chunk2));

            if (Chunk2Derivative is BaseChunk b2 && b2.Var == null && b2.Exp == 1)
            {
                if (b2.Coeff == 1) Chunks.Add(Chunk1);
                else if (b2.Coeff != 0) Chunks.Add(new ProductChunk(Chunk1, Chunk2Derivative));
            }
            else Chunks.Add(new ProductChunk(Chunk1, Chunk2Derivative));

            return new SumChunk(Chunks);
        }

        public IChunk Copy()
        {
            return new ProductChunk(Chunk1.Copy(), Chunk2.Copy()) { Coeff = Coeff };
        }

        public void Multiply(double factor)
        {
            Coeff *= factor;
            //Chunk1.Multiply(factor);
            //Chunk2.Multiply(factor);
        }

        public void MultiplyExpanded(double factor)
        {
            Chunk1.MultiplyExpanded(Coeff * factor);
            Coeff = 1;
        }

        public double Subs(Dictionary<string, double> Values)
        {
            return (Chunk1 != null ? Chunk1.Subs(Values) : 0) * (Chunk2 != null ? Chunk2.Subs(Values) : 0) * Coeff;
        }

        public IChunk Replace(Dictionary<string, IChunk> Values)
        {
            return new ProductChunk(Chunk1.Replace(Values), Chunk2.Replace(Values)) { Coeff = Coeff };
        }

        public IChunk Expanded()
        {
            IChunk a = Chunk1.Expanded();
            IChunk b = Chunk2.Expanded();


            if (a is SumChunk aSum && b is SumChunk bSum)
            {
                List<IChunk> Chunks = new List<IChunk>();

                foreach (var aChunk in aSum.Chunks)
                {
                    foreach (var bChunk in bSum.Chunks)
                    {
                        var aCopy = aChunk.Copy();
                        var bCopy = bChunk.Copy();
                        aCopy.MultiplyExpanded(Coeff);
                        Chunks.Add(new ProductChunk(aCopy, bCopy));
                    }
                }

                return new SumChunk(Chunks) { Coeff = aSum.Coeff * bSum.Coeff };
            }
            else if (a is SumChunk aSum2 && b is BaseChunk bBase)
            {
                List<IChunk> Chunks = new List<IChunk>();

                foreach (var aChunk in aSum2.Chunks)
                {
                    var aCopy = aChunk.Copy();
                    var bCopy = bBase.Copy();
                    aCopy.MultiplyExpanded(Coeff);
                    Chunks.Add(new ProductChunk(aCopy, bCopy));
                }

                return new SumChunk(Chunks);
            }
            else if (a is BaseChunk aBase && b is SumChunk bSum2)
            {
                List<IChunk> Chunks = new List<IChunk>();

                foreach (var bChunk in bSum2.Chunks)
                {
                    var bCopy = bChunk.Copy();
                    var aCopy = aBase.Copy();
                    bCopy.MultiplyExpanded(Coeff);
                    Chunks.Add(new ProductChunk(bCopy, aCopy));
                }

                return new SumChunk(Chunks);
            }
            else if (a is SumChunk aSum3 && b is ProductChunk bProd)
            {
                List<IChunk> Chunks = new List<IChunk>();

                foreach (var aChunk in aSum3.Chunks)
                {
                    var aCopy = aChunk.Copy();
                    var bCopy = bProd.Copy();
                    aCopy.MultiplyExpanded(Coeff);
                    Chunks.Add(new ProductChunk(aCopy, bCopy));
                }

                return new SumChunk(Chunks);
            }
            else if (a is ProductChunk aProd && b is SumChunk bSum3)
            {
                List<IChunk> Chunks = new List<IChunk>();

                foreach (var bChunk in bSum3.Chunks)
                {
                    var bCopy = bChunk.Copy();
                    var aCopy = aProd.Copy();
                    bCopy.MultiplyExpanded(Coeff);
                    Chunks.Add(new ProductChunk(bCopy, aCopy));
                }

                return new SumChunk(Chunks);
            }


            a.Multiply(Coeff);
            return new ProductChunk(a, b);



            //throw new("Something Broke :)"); //Only happens if Chunk1 and Chunk2 are not in a managable format


        }

        public string ToLatex()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() + @"\left(" : string.Empty;
            if (coeff == @"-1\left(") coeff = @"-\left(";
            return $@"{coeff}{Chunk1.ToLatex()}\cdot {Chunk2.ToLatex()}" + (coeff == string.Empty ? "" : @"\right)");
        }

        public IUnit GetUnit(Dictionary<string, IUnit> VariableUnitPairs)
        {
            var Unit1 = Chunk1.GetUnit(VariableUnitPairs);
            var Unit2 = Chunk2.GetUnit(VariableUnitPairs);

            if (Unit1 is BaseUnit ab && Unit2 is BaseUnit bb)
            {
                List<IUnit> num = new List<IUnit>();
                List<IUnit> den = new List<IUnit>();
                if (ab.Power >= 0) num.Add(ab);
                else den.Add(ab.Exponentiate(-1));
                if (bb.Power >= 0) num.Add(bb);
                else den.Add(bb.Exponentiate(-1));
                return new Unit(num,den);
            }
            else if (Unit1 is BaseUnit ab2 && Unit2 is Unit bu)
            {
                if (ab2.Power >= 0) bu.Numerator.Add(ab2);
                else bu.Denominator.Add(ab2.Exponentiate(-1));
                return bu;
            }
            else if (Unit1 is Unit au && Unit2 is BaseUnit bb2)
            {
                if (bb2.Power >= 0) au.Numerator.Add(bb2);
                else au.Denominator.Add(bb2.Exponentiate(-1));
                return au;
            }
            else if (Unit1 is Unit au2 && Unit2 is Unit bu2)
            {
                //Combine Num and Den lists;
                return new Unit(new List<IUnit>(au2.Numerator).Concat(bu2.Numerator).ToList(), new List<IUnit>(au2.Denominator).Concat(bu2.Denominator).ToList());
            }

            throw new Exception("Something went wrong! No case for types.");
        }

        public IChunk Antiderivative(string Var)
        {
            var ExpSimp = Expanded().Simplified();
            if (new IChunkComparer().Equals(this, ExpSimp)) throw new Exception($"Can not calculate Antiderivative of {this}");
            var anti = ExpSimp?.Antiderivative(Var) ?? new BaseChunk(0);
            
            anti.Multiply(Coeff);
            return anti;
        }

        public List<string> GetVariables()
        {
            return Chunk1.GetVariables().Union(Chunk2.GetVariables()).ToList();
        }

        public string ToCode()
        {
            return $"new ProductChunk({Chunk1.ToCode()},{Chunk2.ToCode()})";
        }
    }
}
