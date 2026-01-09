using System;
using System.Linq;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public class IChunkComparer : IEqualityComparer<IChunk>
        {
            public bool Equals(IChunk a, IChunk b)
            {
                if (a == null || b == null) return false;

                var newa = a.Simplified();
                var newb = b.Simplified();

                if (newa == null && newb == null) return true;

                if (newa is ProductChunk pa && newb is ProductChunk pb)
                {
                    return (pa.Coeff == pb.Coeff) && ((Equals(pa.Chunk1, pb.Chunk1)) && (Equals(pa.Chunk2, pb.Chunk2)) || (Equals(pa.Chunk1, pb.Chunk2)) && (Equals(pa.Chunk2, pb.Chunk1)));
                }
                else if (newa is ChainChunk ca && newb is ChainChunk cb)
                {
                    return (ca.Coeff == cb.Coeff) && Equals(ca.Exp, cb.Exp) && Equals(ca.Chunk, cb.Chunk);
                }
                else if (newa is SumChunk sa && newb is SumChunk sb)
                {
                    if (sa.Coeff != sb.Coeff || sa.Chunks.Count != sb.Chunks.Count) return false;

                    // Order-insensitive comparison of chunk sets
                    var aChunksGrouped = sa.Chunks.GroupBy(x => x, this);
                    var bChunksGrouped = sb.Chunks.GroupBy(x => x, this);

                    foreach (var groupA in aChunksGrouped)
                    {
                        int countA = groupA.Count();
                        int countB = bChunksGrouped.FirstOrDefault(g => Equals(g.Key, groupA.Key))?.Count() ?? -1;
                        if (countA != countB)
                            return false;
                    }

                    return true;

                    //OLD//return (sa.Coeff == sb.Coeff) && (sa.Chunks.Count == sb.Chunks.Count) && sa.Chunks.SequenceEqual(sb.Chunks, new IChunkComparer());
                }
                else if (newa is FuncChunk fa && newb is FuncChunk fb)
                {
                    return (fa.Coeff == fb.Coeff) && (fa.Function == fb.Function) && (Equals(fa.Chunk, fb.Chunk)) && ((fa.SecondChunk == null && fb.SecondChunk == null) || Equals(fa.SecondChunk, fb.SecondChunk));
                }
                else if (newa is BaseChunk ba && newb is BaseChunk bb)
                {
                    return (ba.Coeff == bb.Coeff) && (ba.Exp == bb.Exp) && (ba.Var == bb.Var);
                }
                else return false;

            }

            public int GetHashCode(IChunk obj)
            {
                var simplified = obj.Simplified();

                switch (simplified)
                {
                    case ProductChunk p:
                        {
                            int h1 = GetHashCode(p.Chunk1);
                            int h2 = GetHashCode(p.Chunk2);
                            int minHash = Math.Min(h1, h2);
                            int maxHash = Math.Max(h1, h2);
                            return HashCode.Combine(p.Coeff, minHash, maxHash);
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

                    case BaseChunk b:
                        return HashCode.Combine(b.Coeff, b.Exp, b.Var);

                    default:
                        return obj.Coeff.GetHashCode();
                }
            }
        }
}
