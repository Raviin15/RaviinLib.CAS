using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics;

namespace RaviinLib.CAS
{
    public class ProductChunk : IChunk
    {
        public double Coeff { get; set; }

        public List<IChunk> Chunks { get; set; }
        //public IChunk Chunk1 { get; set; }
        //public IChunk Chunk2 { get; set; }

        public ProductChunk(IChunk Chunk1, IChunk Chunk2, double Coeff = 1)
        {
            //this.Chunk1 = Chunk1;
            //this.Chunk2 = Chunk2;
            this.Chunks = new List<IChunk>() { Chunk1, Chunk2 };  
            this.Coeff = Coeff;
        }

        public ProductChunk(List<IChunk> Chunks, double Coeff = 1)
        {
            this.Chunks = Chunks;
            this.Coeff = Coeff;
        }

        public override string ToString()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() : string.Empty;
            if (coeff == "-1") coeff = "-";
            return $"{coeff}(" + string.Join(" * ", Chunks) + ")";
        }

        public IChunk Simplified()
        {
            return Chunker.Product(SimplificationLogic(Chunks), Coeff);

            //var PrevChunks = Chunks;
            //while (true)
            //{
            //    var returnchunks = SimplificationLogic(PrevChunks);

            //    if (PrevChunks.Count != returnchunks.Count)
            //    {
            //        PrevChunks = returnchunks;
            //        continue;
            //    }
            //    // Order-insensitive comparison of chunk sets
            //    var saChunksGrouped = PrevChunks.GroupBy(x => x, new IChunkComparer());
            //    var sbChunksGrouped = returnchunks.GroupBy(x => x, new IChunkComparer());
            //    bool Continue = false;
            //    foreach (var groupA in saChunksGrouped)
            //    {
            //        int countA = groupA.Count();
            //        int countB = sbChunksGrouped.FirstOrDefault(g => new IChunkComparer().Equals(g.Key, groupA.Key))?.Count() ?? -1;
            //        if (countA != countB)
            //        {
            //            PrevChunks = returnchunks;
            //            Continue = true;
            //            break;
            //        }
            //    }
            //    if (Continue) continue;
            //    break;

            //} ;

            //return Chunker.Product(PrevChunks, Coeff);

            #region Old 2 Chunk Method
            //IChunk a = Chunk1.Simplified();
            //IChunk b = Chunk2.Simplified();

            //if (a.IsZero() || b.IsZero())
            //{
            //    return new BaseChunk(0);
            //}

            //if (new IChunkComparerIgnoreCoeff().Equals(a, b))
            //{
            //    var aCopy = a.Copy();
            //    aCopy.Coeff = 1;
            //    var eqCoeff = a.Coeff * b.Coeff * Coeff;

            //    return Chunker.Chain(eqCoeff, aCopy, new BaseChunk(2));
            //}

            //if (a.IsNumber() && b.IsNumber())
            //{
            //    return new BaseChunk((a as BaseChunk).AsNumber() * (b as BaseChunk).AsNumber());
            //}
            //else if (a.IsNumber())
            //{
            //    //b.Multiply(Math.Pow(n.Coeff,n.Exp) * coeff);
            //    //b.Coeff *= n.Coeff * coeff;
            //    return b.MultiplyBy((a as BaseChunk).AsNumber() * Coeff);
            //}
            //else if (b.IsNumber())
            //{
            //    //a.Multiply(Math.Pow(m.Coeff, m.Exp) * coeff);
            //    return a.MultiplyBy((b as BaseChunk).AsNumber() * Coeff);
            //}
            //else if (a is BaseChunk o && b is BaseChunk p && o.Var == p.Var)
            //{
            //    var tmpExp = o.Exp + p.Exp;
            //    var tmpCoeff = Coeff * o.Coeff * p.Coeff;

            //    return Chunker.Base(tmpCoeff, o.Var, tmpExp);
            //}

            ////Need to Update this!
            ////if (a is ProductChunk || b is ProductChunk)
            ////{
            ////    return CascadeSimplify(new ProductChunk(a,b));
            ////}

            //var coeff = a.Coeff * b.Coeff * Coeff;
            //a.Coeff = 1;
            //b.Coeff = 1;
            //var ReturnVar = Chunker.Product(a, b, coeff);

            //return ReturnVar;
            #endregion
        }

        private List<IChunk> SimplificationLogic(List<IChunk> chunks)
        {
            var AllChunks = chunks.SelectMany(c => Chunker.ProductGetChunksOf(c)).Select(c => c.Simplified());

            List<BaseChunk> BaseChunks = new List<BaseChunk>();

            Dictionary<IChunk, List<IChunk>> OtherGroups = new Dictionary<IChunk, List<IChunk>>(new IChunkComparer());
            //List<ChainChunk> ChainChunks = new List<ChainChunk>();
            //List<FuncChunk> FuncChunks = new List<FuncChunk>();
            //List<SumChunk> SumChunks = new List<SumChunk>();


            foreach (var chunk in AllChunks)
            {
                switch (chunk)
                {
                    case BaseChunk b:
                        BaseChunks.Add(b);
                        break;
                    case ProductChunk p:
                        throw new Exception($"ProductChunk escaped condensing! This: {this}");
                    case ChainChunk c:
                        if (OtherGroups.TryGetValue(c.Chunk,out List<IChunk> KeyListChain))
                        {
                            KeyListChain.Add(c);
                        }
                        else
                        {
                            OtherGroups[c.Chunk] = new List<IChunk>() { c };
                        }
                        break;

                    default:
                        if (OtherGroups.TryGetValue(chunk, out List<IChunk> KeyListDefault))
                        {
                            KeyListDefault.Add(chunk);
                        }
                        else
                        {
                            OtherGroups[chunk] = new List<IChunk>() { chunk };
                        }
                        break;


                        //case ChainChunk c:
                        //    ChainChunks.Add(c);
                        //    break;
                        //case FuncChunk f:
                        //    FuncChunks.Add(f);
                        //    break;
                        //case SumChunk p:
                        //    SumChunks.Add(p);
                        //    break;

                        //default:
                        //    throw new Exception($"ProductChunk escaped condensing! This: {this}");
                }
            }


            List<IChunk> Base = BaseChunks
                .GroupBy(c => c, new IChunkComparerIgnoreCoeffExp())
                .Select(g => Chunker.Base(g.Aggregate(1.0, (acc, h) => acc * h.Coeff), (g.Key as BaseChunk).Var, g.Sum(c => c.Exp)))
                .ToList();

            List<IChunk> Others = new List<IChunk>();
            foreach (var Item in OtherGroups)
            {
                double totalCoeff = 1.0;
                IChunk totalExp = new BaseChunk(0);

                foreach (var Chunk in Item.Value)
                {
                    totalCoeff *= Chunk.Coeff;
                    if (Chunk is ChainChunk c)
                    {
                        totalExp = Chunker.Sum(totalExp,c.Exp);
                    }
                    else
                    {
                        totalExp = Chunker.Sum(totalExp, new BaseChunk(1));
                    }
                }

                Others.Add(Chunker.Chain(totalCoeff,Item.Key,totalExp));
            }

            //List<IChunk> Chain = ChainChunks
            //    .GroupBy(c => c, new IChunkComparerIgnoreCoeffExp())
            //    .Select(g =>
            //    {
            //        if (!(g.Key as ChainChunk).Exp.IsNumber())
            //        {
            //            return Chunker.Chain(g.Aggregate(1.0, (acc, h) => acc * h.Coeff), (g.Key as ChainChunk).Chunk.Copy(), new SumChunk(g.Select(c => c.Exp).ToList()));
            //        }
            //        else
            //        {
            //            return Chunker.Chain(g.Aggregate(1.0, (acc, h) => acc * h.Coeff), (g.Key as ChainChunk).Chunk.Copy(), new BaseChunk(g.Sum(c => (c.Exp as BaseChunk).AsNumber())));
            //        }
            //    })
            //    .ToList();

            //List<IChunk> Func = FuncChunks
            //    .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
            //    .Select(g => Chunker.Chain(g.Aggregate(1.0, (acc, h) => acc * h.Coeff), g.Key.Copy(), new BaseChunk(g.Count())))
            //    .ToList();

            //List<IChunk> Sum = SumChunks
            //    .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
            //    .Select(g => Chunker.Chain(g.Aggregate(1.0, (acc, h) => acc * h.Coeff), g.Key.Copy(), new BaseChunk(g.Count())))
            //    .ToList();

            List<IChunk> returnchunks = new List<IChunk>();

            returnchunks.AddRange(Base);
            returnchunks.AddRange(Others);
            //returnchunks.AddRange(Chain);
            //returnchunks.AddRange(Func);
            //returnchunks.AddRange(Sum);

            return returnchunks;
        }



        public IChunk Derivative(string Var)
        {
            IChunk first = Chunks[0];
            List<IChunk> rest = Chunks.Skip(1).Select(c => c.Copy()).ToList();

            if (rest.Count == 0) return first.Derivative(Var);
            var restChunk = (rest.Count == 1) ? rest[0] : new ProductChunk(rest);

            return Chunker.Sum(Chunker.Product(first.Derivative(Var), restChunk),Chunker.Product(first.Copy(), restChunk.Derivative(Var)),Coeff);


            //var Chunk1Derivative = Chunk1.Derivative(Var);
            //var Chunk2Derivative = Chunk2.Derivative(Var);

            //return Chunker.Sum( Chunker.Product(Chunk1Derivative, Chunk2), Chunker.Product(Chunk1, Chunk2Derivative), Coeff );




            //List<IChunk> Chunks = new List<IChunk>();
            //if (
            //    (Chunk1Derivative is BaseChunk cb1 && cb1.Var == null && cb1.Exp == 1 && cb1.Coeff == 0) 
            //    ||
            //    (Chunk2Derivative is BaseChunk cb2 && cb2.Var == null && cb2.Exp == 1 && cb2.Coeff == 0)
            //    ) return new BaseChunk(0);

            //if (Chunk1Derivative is BaseChunk b && b.Var == null && b.Exp == 1)
            //{
            //    if (b.Coeff == 1) Chunks.Add(Chunk2);
            //    else if (b.Coeff != 0) Chunks.Add(new ProductChunk(Chunk1Derivative, Chunk2));
            //}
            //else Chunks.Add(new ProductChunk(Chunk1Derivative, Chunk2));

            //if (Chunk2Derivative is BaseChunk b2 && b2.Var == null && b2.Exp == 1)
            //{
            //    if (b2.Coeff == 1) Chunks.Add(Chunk1);
            //    else if (b2.Coeff != 0) Chunks.Add(new ProductChunk(Chunk1, Chunk2Derivative));
            //}
            //else Chunks.Add(new ProductChunk(Chunk1, Chunk2Derivative));

            //return new SumChunk(Chunks);
        }

        public IChunk Copy()
        {
            List<IChunk> NewChunks = new List<IChunk>();
            Chunks.ForEach(c => NewChunks.Add(c.Copy()));
            return new ProductChunk(NewChunks, Coeff);
        }

        public void Multiply(double factor)
        {
            Coeff *= factor;
            //Chunk1.Multiply(factor);
            //Chunk2.Multiply(factor);
        }

        public IChunk MultiplyBy(double factor)
        {
            IChunk NewChunk = Copy();
            NewChunk.Multiply(factor);
            return NewChunk;
        }

        public double Subs(Dictionary<string, double> Values)
        {

            double returnVar = 0;
            foreach (var chunk in Chunks)
            {
                returnVar *= chunk.Subs(Values);
            }
            return returnVar * Coeff;
        
            //return (Chunk1 != null ? Chunk1.Subs(Values) : 0) * (Chunk2 != null ? Chunk2.Subs(Values) : 0) * Coeff;
        }

        public IChunk Replace(Dictionary<string, IChunk> Values)
        {
            List<IChunk> newChunks = new List<IChunk>();

            foreach (var chunk in Chunks)
            {
                newChunks.Add(chunk.Replace(Values));
            }
            return Chunker.Product(newChunks, Coeff);

            //return Chunker.Product(Chunk1.Replace(Values), Chunk2.Replace(Values), Coeff);
        }

        public IChunk Expanded()
        {
            List<IChunk> newChunks = new List<IChunk>();

            foreach (var chunk in Chunks)
            {
                var Expanded = chunk.Expanded();
                //if (Expanded != null)
                //{
                //    if (Expanded is SumChunk s)
                //    {
                //        s.MultiplyExpanded(s.Coeff);
                //        newChunks.AddRange(s.Chunks);
                //    }
                //    else newChunks.Add(Expanded);

                //}
                newChunks.Add(Expanded);
            }

            return Chunker.Product(newChunks, Coeff);

            #region Old
            //IChunk a = Chunk1.Expanded();
            //IChunk b = Chunk2.Expanded();


            //if (a is SumChunk aSum && b is SumChunk bSum)
            //{
            //    List<IChunk> Chunks = new List<IChunk>();

            //    foreach (var aChunk in aSum.Chunks)
            //    {
            //        foreach (var bChunk in bSum.Chunks)
            //        {
            //            var aCopy = aChunk.Copy();
            //            var bCopy = bChunk.Copy();
            //            aCopy.MultiplyExpanded(Coeff);
            //            Chunks.Add(Chunker.Product(aCopy, bCopy));
            //        }
            //    }

            //    return Chunker.Sum(Chunks, aSum.Coeff * bSum.Coeff);
            //}
            //else if (a is SumChunk aSum2 && b is BaseChunk bBase)
            //{
            //    List<IChunk> Chunks = new List<IChunk>();

            //    foreach (var aChunk in aSum2.Chunks)
            //    {
            //        var aCopy = aChunk.Copy();
            //        var bCopy = bBase.Copy();
            //        aCopy.MultiplyExpanded(Coeff);
            //        Chunks.Add(Chunker.Product(aCopy, bCopy));
            //    }

            //    return Chunker.Sum(Chunks);
            //}
            //else if (a is BaseChunk aBase && b is SumChunk bSum2)
            //{
            //    List<IChunk> Chunks = new List<IChunk>();

            //    foreach (var bChunk in bSum2.Chunks)
            //    {
            //        var bCopy = bChunk.Copy();
            //        var aCopy = aBase.Copy();
            //        bCopy.MultiplyExpanded(Coeff);
            //        Chunks.Add(Chunker.Product(bCopy, aCopy));
            //    }

            //    return Chunker.Sum(Chunks);
            //}
            //else if (a is SumChunk aSum3 && b is ProductChunk bProd)
            //{
            //    List<IChunk> Chunks = new List<IChunk>();

            //    foreach (var aChunk in aSum3.Chunks)
            //    {
            //        var aCopy = aChunk.Copy();
            //        var bCopy = bProd.Copy();
            //        aCopy.MultiplyExpanded(Coeff);
            //        Chunks.Add(Chunker.Product(aCopy, bCopy));
            //    }

            //    return Chunker.Sum(Chunks);
            //}
            //else if (a is ProductChunk aProd && b is SumChunk bSum3)
            //{
            //    List<IChunk> Chunks = new List<IChunk>();

            //    foreach (var bChunk in bSum3.Chunks)
            //    {
            //        var bCopy = bChunk.Copy();
            //        var aCopy = aProd.Copy();
            //        bCopy.MultiplyExpanded(Coeff);
            //        Chunks.Add(Chunker.Product(bCopy, aCopy));
            //    }

            //    return Chunker.Sum(Chunks);
            //}


            //a.Multiply(Coeff);
            //return Chunker.Product(a, b);



            //throw new("Something Broke :)"); //Only happens if Chunk1 and Chunk2 are not in a managable format

            #endregion
        }

        public string ToLatex()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() : string.Empty;
            if (coeff == "-1") coeff = "-";
            return $@"{coeff}\left(" + string.Join("*", Chunks.Select(c => c.ToLatex())) + @"\right)";
        }

        public IUnit GetUnit(Dictionary<string, IUnit> VariableUnitPairs)
        {
            var Unit1 = Chunks[0].GetUnit(VariableUnitPairs);
            var Unit2 = (new ProductChunk(Chunks.Skip(1).ToList())).GetUnit(VariableUnitPairs);

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
            return Chunks.SelectMany(c => c.GetVariables()).Distinct().ToList();
            //return Chunk1.GetVariables().Union(Chunk2.GetVariables()).ToList();
        }

        public string ToCode()
        {
            return $"new ProductChunk(new List<IChunk>(){{{string.Join(",", Chunks.Select(c => c.ToCode()))}}}, {Coeff})";
            //return $"new ProductChunk({Chunk1.ToCode()},{Chunk2.ToCode()})";
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
            return Chunks.All(c => c.IsConstant());
            //return Chunk1.IsConstant() && Chunk2.IsConstant();
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
