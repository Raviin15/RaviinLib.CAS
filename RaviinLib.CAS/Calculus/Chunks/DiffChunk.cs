using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaviinLib.CAS
{
    public class DiffChunk
    {
        #region Diffchunk
        //public class DiffChunk(params List<IChunk> Chunks) : IChunk
        //{
        //    public double Coeff { get; set; } = 1;
        //    public List<IChunk> Chunks { get; set; } = Chunks;

        //    public IChunk Copy()
        //    {
        //        List<IChunk> NewChunks = new();
        //        foreach (var Chunk in Chunks)
        //        {
        //            NewChunks.Add(Chunk.Copy());
        //        }
        //        return new DiffChunk(NewChunks,Coeff);
        //    }

        //    public IChunk Derivative(string Var)
        //    {
        //        var copy = (DiffChunk)this.Copy();
        //        copy.Chunks.Add(new BaseChunk(1,Var,1));
        //        return copy;
        //    }

        //    public void Multiply(double factor)
        //    {
        //        Chunks.ForEach( c=> c.Multiply(factor));
        //    }

        //    public double Subs(Dictionary<string, double> Values)
        //    {
        //        List<double> InnerVals = new();
        //        foreach (var Chunk in Chunks)
        //        {
        //            InnerVals.Add(Chunk.Subs(Values));
        //        }

        //        List<double> newvals = new();
        //        for (int i = 1; i < Chunks.Count; i++)
        //        {
        //            newvals.Add(InnerVals[i] - InnerVals[i-1]);
        //        }

        //        if (newvals.Count == 0) throw new("No vals to return.");

        //        return newvals.Last();
        //    }

        //    public IChunk Replace(Dictionary<string, double> Values)
        //    {
        //        List<IChunk> NewChunks = new();
        //        foreach (var Chunk in Chunks)
        //        {
        //            NewChunks.Add(Chunk.Replace(Values));
        //        }
        //        return new DiffChunk(NewChunks, Coeff);
        //    }

        //    public IChunk? Simplified()
        //    {
        //        List<IChunk> NewChunks = new();
        //        foreach (var Chunk in Chunks)
        //        {
        //            var s = Chunk.Simplified();
        //            if (s != null) NewChunks.Add(s);
        //        }
        //        return new DiffChunk(NewChunks, Coeff);
        //    }

        //}
        #endregion

    }
}
