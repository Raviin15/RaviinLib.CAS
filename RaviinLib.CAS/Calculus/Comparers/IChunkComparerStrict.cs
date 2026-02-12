using System;
using System.Linq;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public class IChunkComparerStrict : IEqualityComparer<IChunk>
    {
        public bool Equals(IChunk a, IChunk b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;

            if (a.GetType() != b.GetType()) return false;

            switch (a)
            {
                case BaseChunk b1 when b is BaseChunk b2:
                    return (b1.Coeff == b2.Coeff) && (b1.Exp == b2.Exp) && (b1.Var == b2.Var);
                case ChainChunk c1 when b is ChainChunk c2:
                    return (c1.Coeff == c2.Coeff) && Equals(c1.Exp, c2.Exp) && Equals(c1.Chunk, c2.Chunk);
                case FuncChunk f1 when b is FuncChunk f2:
                    return (f1.Coeff == f2.Coeff) && (f1.Function == f2.Function) && Equals(f1.Chunk, f2.Chunk) && ((f1.SecondChunk == null && f2.SecondChunk == null) || Equals(f1.SecondChunk, f2.SecondChunk));
                case ProductChunk p1 when b is ProductChunk p2:
                    if (p1.Coeff != p2.Coeff || p1.Chunks.Count != p2.Chunks.Count) return false;
                    // Order-insensitive comparison of chunk sets
                    var paChunksGrouped = p1.Chunks.GroupBy(x => x, this);
                    var pbChunksGrouped = p2.Chunks.GroupBy(x => x, this);
                    foreach (var groupA in paChunksGrouped)
                    {
                        int countA = groupA.Count();
                        int countB = pbChunksGrouped.FirstOrDefault(g => Equals(g.Key, groupA.Key))?.Count() ?? -1;
                        if (countA != countB)
                            return false;
                    }
                    return true;
                //return (p1.Coeff == p2.Coeff) && ((Equals(p1.Chunk1, p2.Chunk1)) && (Equals(p1.Chunk2, p2.Chunk2)) || (Equals(p1.Chunk1, p2.Chunk2)) && (Equals(p1.Chunk2, p2.Chunk1)));
                case SumChunk s1 when b is SumChunk s2:
                    if (s1.Coeff != s2.Coeff || s1.Chunks.Count != s2.Chunks.Count) return false;
                    // Order-insensitive comparison of chunk sets
                    var saChunksGrouped = s1.Chunks.GroupBy(x => x, this);
                    var sbChunksGrouped = s2.Chunks.GroupBy(x => x, this);
                    foreach (var groupA in saChunksGrouped)
                    {
                        int countA = groupA.Count();
                        int countB = sbChunksGrouped.FirstOrDefault(g => Equals(g.Key, groupA.Key))?.Count() ?? -1;
                        if (countA != countB)
                            return false;
                    }
                    return true;

                default:
                    return false;
            }

        }

        public int GetHashCode(IChunk obj)
        {
            if (obj is null) return 0;

            switch (obj)
            {
                case BaseChunk b:
                    return HashCode.Combine(b.Coeff, b.Exp, b.Var);

                case ProductChunk p:
                    {
                        var chunkHashes = p.Chunks.Select(GetHashCode).OrderBy(h => h);
                        return chunkHashes.Aggregate(p.Coeff.GetHashCode(), (acc, h) => HashCode.Combine(acc, h));
                        //int h1 = GetHashCode(p.Chunk1);
                        //int h2 = GetHashCode(p.Chunk2);
                        //int minHash = Math.Min(h1, h2);
                        //int maxHash = Math.Max(h1, h2);
                        //return HashCode.Combine(p.Coeff, minHash, maxHash);
                    }

                case ChainChunk c:
                    {
                        int chunkHash = GetHashCode(c.Chunk);
                        return HashCode.Combine(c.Coeff, GetHashCode(c.Exp), chunkHash);
                    }

                case SumChunk s:
                    {
                        var chunkHashes = s.Chunks.Select(GetHashCode).OrderBy(h => h);
                        return chunkHashes.Aggregate(s.Coeff.GetHashCode(), (acc, h) => HashCode.Combine(acc, h));
                    }

                case FuncChunk f:
                    {
                        int chunkHash = GetHashCode(f.Chunk);
                        if (f.SecondChunk != null)
                        {
                            int secondChunkHash = GetHashCode(f.SecondChunk);
                            return HashCode.Combine(f.Coeff, f.Function, chunkHash, secondChunkHash);
                        }
                        else
                        {
                            return HashCode.Combine(f.Coeff, f.Function, chunkHash);
                        }
                    }

                default:
                    return obj.GetHashCode();
            }
        }
    }
}
