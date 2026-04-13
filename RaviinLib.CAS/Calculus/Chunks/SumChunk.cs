using System;
using System.Linq;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public class SumChunk: IChunk
    {
        public double Coeff { get; set; }
        public List<IChunk> Chunks { get; set; }

        public SumChunk(List<IChunk> Chunks, double Coeff = 1)
        {
            this.Chunks = Chunks;
            this.Coeff = Coeff;
        }

        public IChunk Derivative(string Var)
        {
            //List<IChunk> chunks = new List<IChunk>();

            //foreach (var Chunk in Chunks)
            //{
            //    var d = Chunk.Derivative(Var);
            //    //if ((d is BaseChunk b && b.Var == null && b.Exp == 1 && b.Coeff == 0)) continue;//(d is BaseChunk b && b.Coeff == 0) ||
            //    chunks.Add(d);
            //}

            //if (chunks.Count == 0) return new BaseChunk(0);
            //if (chunks.Count == 1) return chunks[0];
            return Chunker.Sum(Chunks.Select(c => c.Derivative(Var)).ToList(), Coeff);
        }

        public override string ToString()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() : string.Empty;
            if (coeff == "-1") coeff = "-";
            return $"{coeff}(" + string.Join(" + ", Chunks) + ")";
        }

        public IChunk Copy()
        {
            List<IChunk> NewChunks = new List<IChunk>();
            Chunks.ForEach(c => NewChunks.Add(c.Copy()));
            return new SumChunk(NewChunks) { Coeff = Coeff };
        }

        public void Multiply(double factor)
        {
            Coeff *= factor;
            //foreach (var chunk in Chunks)
            //{
            //    chunk.Multiply(factor);
            //}
        }

        //public void MultiplyExpanded(double factor)
        //{
        //    foreach (var chunk in Chunks)
        //    {
        //        chunk.MultiplyExpanded(factor);
        //    }
        //}

        public IChunk MultiplyBy(double factor)
        {
            IChunk NewChunk = Copy();
            NewChunk.Multiply(factor);
            return NewChunk;
        }

        public IChunk Simplified()
        {
            //var Copy = CascadeSimplify(this);

            var AllChunks = Chunks.SelectMany(c => Chunker.SumGetChunksOf(c)).Select(c => c.Simplified());

            #region Old

            //List<IChunk> chunks = new List<IChunk>();

            //foreach (var c in AllChunks)
            //{
            //    var s = c.Simplified();
            //    if (s != null) chunks.Add(s);
            //}

            //if (chunks.Count == 0) return null;
            //if (chunks.Count == 1) return chunks[0];

            //var Sum = Copy.Chunks.Where(c => c is SumChunk).Select(c => (SumChunk)c).ToList();
            //#region Sum
            //Copy.Chunks.AddRange( Sum.SelectMany(c => c.Chunks));
            //#endregion

            #region Base
            //var Base = chunks
            //    .OfType<BaseChunk>()
            //    .GroupBy(c => new { c.Var, c.Exp })
            //    .Select(g => new { Sum = g.Sum(c => c.Coeff), g.Key.Var, g.Key.Exp })
            //    .Where(x => x.Sum != 0)
            //    .Select(x => new BaseChunk(x.Sum, x.Var, x.Exp));

            //var Base = chunks
            //    .OfType<BaseChunk>()
            //    .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
            //    .Select(g => { var Sum = g.Sum(c => c.Coeff); return new BaseChunk(Sum, ((BaseChunk)g.Key).Var, ((BaseChunk)g.Key).Exp); })
            //    .Where(x => x.Coeff != 0);

            #endregion

            #region Chain
            //var Chain = chunks
            //    .OfType<ChainChunk>()
            //    .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
            //    .Select(g => { var copy = g.Key.Copy(); copy.Coeff = g.Sum(c => c.Coeff); return copy; });

            #endregion

            #region Product
            //var Product = chunks
            //    .OfType<ProductChunk>()
            //    .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
            //    .Select(g => { var copy = g.Key.Copy(); copy.Coeff = g.Sum(c => c.Coeff); return copy; });
            #endregion

            #endregion

            List<BaseChunk> BaseChunks = new List<BaseChunk>();
            List<ChainChunk> ChainChunks = new List<ChainChunk>();
            List<FuncChunk> FuncChunks = new List<FuncChunk>();
            List<ProductChunk> ProductChunks = new List<ProductChunk>();


            foreach (var chunk in AllChunks)
            {
                switch (chunk)
                {
                    case BaseChunk b:
                        BaseChunks.Add(b);
                        break;
                    case ChainChunk c:
                        ChainChunks.Add(c);
                        break;
                    case FuncChunk f:
                        FuncChunks.Add(f);
                        break;
                    case ProductChunk p:
                        ProductChunks.Add(p);
                        break;

                    default:
                        throw new Exception($"SumChunk escaped condensing! This: {this}");
                }
            }


            List<IChunk> Base = BaseChunks
                .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
                .Select(g => Chunker.Base(g.Sum(c => c.Coeff),(g.Key as BaseChunk).Var, (g.Key as BaseChunk).Exp))
                .ToList();

            List<IChunk> Chain = ChainChunks
                .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
                .Select(g => Chunker.Chain(g.Sum(c => c.Coeff), (g.Key as ChainChunk).Chunk.Copy(), (g.Key as ChainChunk).Exp.Copy()))
                .ToList();

            List<IChunk> Func = FuncChunks
                .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
                .Select(g => Chunker.Func((g.Key as FuncChunk).Chunk.Copy(), (g.Key as FuncChunk).Function, g.Sum(c => c.Coeff), (g.Key as FuncChunk).SecondChunk.Copy()))
                .ToList();

            List<IChunk> Product = ProductChunks
                .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
                .Select(g => Chunker.Product((g.Key as ProductChunk).Chunks.Select(c => c.Copy()).ToList(), g.Sum(c => c.Coeff)))
                .ToList();





            //var Funcs = chunks.OfType<FuncChunk>().ToList();

            List<IChunk> returnchunks = new List<IChunk>();
            //returnchunks.AddRange(bdist.SelectMany(c => c.SelectMany(c => c))); //OLD
            returnchunks.AddRange(Base);
            returnchunks.AddRange(Chain);
            returnchunks.AddRange(Func);
            returnchunks.AddRange(Product);
            //returnchunks = returnchunks.Where(c => c.Coeff != 0).ToList();
            //if (returnchunks.Count == 0) return new BaseChunk(0);
            //if (returnchunks.Count == 1) return returnchunks.First();

            return Chunker.Sum(returnchunks);

        }

        public double Subs(Dictionary<string, double> Values)
        {
            double returnVar = 0;
            foreach (var chunk in Chunks)
            {
                returnVar += chunk.Subs(Values);
            }
            return returnVar * Coeff;
        }

        public IChunk Replace(Dictionary<string, IChunk> Values)
        {
            List<IChunk> newChunks = new List<IChunk>();

            foreach (var chunk in Chunks)
            {
                newChunks.Add(chunk.Replace(Values));
            }
            return Chunker.Sum(newChunks, Coeff);
        }


        private static SumChunk CascadeSimplify(SumChunk Chunk)
        {
            var Copy = (SumChunk)Chunk.Copy();

            var RedundantChains = Copy.Chunks.Where(ch => ch is ChainChunk c && c.ExpIsNum && c.ExpAsNum == 1);
            List<IChunk> InnerChunks = new List<IChunk>();
            foreach (var item in RedundantChains)
            {
                var InnerCopy = (item as ChainChunk).Chunk.Copy();
                InnerCopy.Multiply(item.Coeff);
                InnerChunks.Add(InnerCopy);
            }

            RedundantChains.ToList().ForEach(c => Copy.Chunks.Remove(c));
            Copy.Chunks.AddRange(InnerChunks);

            Copy.Chunks.ForEach(c => c.Multiply(Copy.Coeff));
            Copy.Coeff = 1;
            var Sum = Copy.Chunks.OfType<SumChunk>().ToList();
            Sum.ForEach(c => Copy.Chunks.Remove(c));
            Copy.Chunks.AddRange(Sum.Select(c => CascadeSimplify(c)).SelectMany(c => c.Chunks));
            return Copy;
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
                newChunks.Add(Chunker.Product(Expanded , new BaseChunk(Coeff)));
            }
            //if (newChunks.Count == 1) return newChunks[0];

            return Chunker.Sum(newChunks);
        }

        public string ToLatex()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() : string.Empty;
            if (coeff == "-1") coeff = "-";
            return $@"{coeff}\left(" + string.Join("+", Chunks.Select(c => c.ToLatex())) + @"\right)";
        }

        public IUnit GetUnit(Dictionary<string, IUnit> VariableUnitPairs)
        {
            List<IUnit> UnitList = new List<IUnit>();
            foreach (var Chunk in Chunks)
            {
                UnitList.Add(Chunk.GetUnit(VariableUnitPairs));
            }

            var simp = UnitList.Select(c => c.Simplify());
            var DistinctCount = simp.Distinct(new IUnitComparer()).Count();
            if (DistinctCount == 0) return new BaseUnit(Units.None);

            if (DistinctCount != 1) throw new Exception($"Can not add different units: {string.Join(" + ", UnitList.Distinct().Select(c => c.ToString()))}");
            return UnitList[0];
        }

        public IChunk Antiderivative(string Var)
        {
            List<IChunk> newChunks = new List<IChunk>();
            foreach (var chunk in Chunks)
            {
                newChunks.Add(chunk.Antiderivative(Var));
            }
            return Chunker.Sum(newChunks, Coeff);
        }

        public List<string> GetVariables()
        {
            return Chunks.SelectMany(c => c.GetVariables()).Distinct().ToList();
        }

        public string ToCode()
        {
            return $"new SumChunk(new List<IChunk>(){{{string.Join(",",Chunks.Select(c => c.ToCode()))}}}, {Coeff})";
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
