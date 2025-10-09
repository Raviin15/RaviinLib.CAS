using System.Linq;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public class FunctionVector
    {
        public int Length { get => Vector.Length; }
        public Function this[int index] { get => Vector[index]; }
        public Function[] Vector { get; private set; }

        private int? PrevLength = null;
        private List<string> _Variables = null;
        public List<string> Variables
        {
            get
            {
                if (PrevLength == null || PrevLength != Length)
                {
                    List<string> NewVars = new List<string>();

                    foreach (var Func in Vector)
                    {
                        NewVars.AddRange(Func.Variables);
                    }

                    _Variables = NewVars.Distinct().ToList();
                }
                return _Variables;
            }
        }

        public FunctionVector(Function[] Vector)
        {
            this.Vector = Vector;
        }
        public double[] Subs(Dictionary<string, double> Values)
        {
            List<double> Vals = new List<double>();
            foreach (var Func in Vector)
            {
                Vals.Add(Func.Subs(Values));
            }
            return Vals.ToArray();
        }

        public FunctionVector Replace(Dictionary<string, IChunk> Values)
        {
            List<Function> Funcs = new List<Function>();
            foreach (var Func in Vector)
            {
                Funcs.Add(Func.Replace(Values));
            }
            return new FunctionVector(Funcs.ToArray());
        }

        public Function MergedSum()
        {
            Function sum = 0;

            foreach (var func in Vector)
            {
                sum += func;
            }
            return sum;
        }
    }
}
