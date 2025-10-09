using MathNet.Numerics.LinearAlgebra;

using System.Collections.Generic;
using System;

namespace RaviinLib.CAS
{
    public class FunctionRowVector : FunctionVector
        {

            public FunctionRowVector(Function[] Vector) : base(Vector)
            {
            }

            public FunctionColVector Transpose()
            {
                return new FunctionColVector(Vector);
            }

            public override string ToString()
            {
                string returnstring = string.Empty;
                foreach (Function item in Vector)
                {
                    returnstring += item.ToString() + "\t";
                }
                return returnstring;
            }

            public static explicit operator FunctionRowVector(Function[] a)
            {
                return new FunctionRowVector(a);
            }

            public static FunctionRowVector operator *(FunctionRowVector a, Matrix<double> b)
            {
                if (a.Length != b.RowCount) throw new Exception("Size Mismatch");
                var cols = b.ToColumnArrays();

                List<Function> Funcs = new List<Function>();
                foreach (var col in cols)
                {
                    Function func = 0;
                    for (int i = 0; i < col.Length; i++)
                    {
                        func += (a[i] * col[i]);
                    }
                    Funcs.Add(func);
                }
                return new FunctionRowVector(Funcs.ToArray());
            }

            public static FunctionRowVector operator *(double a, FunctionRowVector b)
            {
                List<Function> funcs = new List<Function>();
                foreach (var func in b.Vector)
                {
                    funcs.Add(func.Copy() * a);
                }
                return new FunctionRowVector(funcs.ToArray());
            }

            public static FunctionRowVector operator *(FunctionRowVector b, double a)
            {
                return b * a;
            }
        }
}
