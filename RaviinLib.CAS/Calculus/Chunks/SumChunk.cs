using System;
using System.Linq;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public class SumChunk: IChunk
    {
        public double Coeff { get; set; } = 1;
        public List<IChunk> Chunks { get; set; }

        public SumChunk(List<IChunk> Chunks)
        {
            this.Chunks = Chunks;
        }

        public IChunk Derivative(string Var)
        {
            List<IChunk> chunks = new List<IChunk>();

            foreach (var Chunk in Chunks)
            {
                var d = Chunk.Derivative(Var);
                if ((d is BaseChunk b && b.Var == null && b.Exp == 1 && b.Coeff == 0)) continue;//(d is BaseChunk b && b.Coeff == 0) ||
                chunks.Add(d);
            }

            if (chunks.Count == 0) return new BaseChunk(0);
            if (chunks.Count == 1) return chunks[0];
            return new SumChunk(chunks);
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
            foreach (var chunk in Chunks)
            {
                chunk.Multiply(factor);
            }
        }
        public void MultiplyExpanded(double factor)
        {
            foreach (var chunk in Chunks)
            {
                chunk.MultiplyExpanded(factor);
            }
        }

        public static SumChunk operator *(SumChunk chunk, double coeff)
        {
            var copy = (SumChunk)chunk.Copy();
            copy.Multiply(coeff);
            return copy;
        }
        public static SumChunk operator *(double coeff, SumChunk chunk)
        {
            return chunk * coeff;
        }

        public IChunk Simplified()
        {
            if (Coeff == 0) return null;

            var Copy = CascadeSimplify(this);

            List<IChunk> chunks = new List<IChunk>();

            foreach (var c in Copy.Chunks)
            {
                var s = c.Simplified();
                if (s != null) chunks.Add(s);
            }

            if (chunks.Count == 0) return null;
            if (chunks.Count == 1) return chunks[0];

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

            var Base = chunks
                .OfType<BaseChunk>()
                .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
                .Select(g => { var Sum = g.Sum(c => c.Coeff); return new BaseChunk(Sum, ((BaseChunk)g.Key).Var, ((BaseChunk)g.Key).Exp); })
                .Where(x => x.Coeff != 0);
            
            #endregion

            #region Chain
            var Chain = chunks
                .OfType<ChainChunk>()
                .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
                .Select(g => { var copy = g.Key.Copy(); copy.Coeff = g.Sum(c => c.Coeff); return copy; });

            #endregion

            #region Product
            var Product = chunks
                .OfType<ProductChunk>()
                .GroupBy(c => c, new IChunkComparerIgnoreCoeff())
                .Select(g => { var copy = g.Key.Copy(); copy.Coeff = g.Sum(c => c.Coeff); return copy; });
            #endregion



            var Funcs = chunks.OfType<FuncChunk>().ToList();

            List<IChunk> returnchunks = new List<IChunk>();
            returnchunks.AddRange(Base);
            //returnchunks.AddRange(bdist.SelectMany(c => c.SelectMany(c => c))); //OLD
            returnchunks.AddRange(Chain);
            returnchunks.AddRange(Product);
            returnchunks.AddRange(Funcs);
            returnchunks = returnchunks.Where(c => c.Coeff != 0).ToList();
            if (returnchunks.Count == 0) return null;
            if (returnchunks.Count == 1) return returnchunks.First();

            return new SumChunk(returnchunks);

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
            return new SumChunk(newChunks) { Coeff = Coeff };
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
                if (Expanded != null)
                {
                    if (Expanded is SumChunk s)
                    {
                        s.MultiplyExpanded(s.Coeff);
                        newChunks.AddRange(s.Chunks);
                    }
                    else newChunks.Add(Expanded);

                }
            }
            if (newChunks.Count == 1) return newChunks[0];

            return new SumChunk(newChunks) { Coeff = Coeff };
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
            return new SumChunk(newChunks) { Coeff = Coeff };
        }

        public List<string> GetVariables()
        {
            return Chunks.SelectMany(c => c.GetVariables()).Distinct().ToList();
        }
    }
}
