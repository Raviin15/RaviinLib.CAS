using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

using System;
using System.Linq;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public class FunctionColVector : FunctionVector
    {
        public FunctionColVector(Function[] Vector) : base(Vector)
        {
        }

        public FunctionRowVector Transpose()
        {
            return new FunctionRowVector(Vector);
        }

        public (Matrix<double> A, Vector<double> b) GetSystemForm(List<string> Variables = null)
        {
            List<Vector<double>> LHSs = new List<Vector<double>>();
            List<double> RHSs = new List<double>();
            foreach (var func in Vector)
            {
                var ret = func.GetSystemForm((Variables == null || Variables.Count != 0) ? Variables : this.Variables);
                LHSs.Add(ret.LHS);
                RHSs.Add(ret.RHS);
            }

            Matrix<double> A = Matrix.Build.DenseOfRowVectors(LHSs);
            Vector<double> b = MathNet.Numerics.LinearAlgebra.Double.Vector.Build.DenseOfEnumerable(RHSs);

            //Console.WriteLine(A);
            //Console.WriteLine(b);
            return (A, b);
        }

        public FunctionMatrix Jacobian(List<string> Variables = null)
        {
            if (Variables == null) Variables = this.Variables;

            List<Function[]> Derivatives = new List<Function[]>();

            foreach (var Func in Vector)
            {
                Derivatives.Add(Func.GetGradiant(Variables).Select(c => { c.Variables = Variables; return c; }).ToArray());
            }
            return Derivatives.ToArray();
        }

        #region Overides
        public override string ToString()
        {
            string returnstring = string.Empty;
            foreach (Function item in Vector)
            {
                returnstring += item.ToString() + "\n";
            }
            return returnstring;
        }

        public static explicit operator FunctionColVector(Function[] a)
        {
            return new FunctionColVector(a);
        }

        public static Function operator *(FunctionRowVector a, FunctionColVector b)
        {
            if (a.Length != b.Length) throw new Exception("Size Mismatch");

            List<string> NewVars = new List<string>(a.Variables);
            NewVars.AddRange(b.Variables);

            Function Func = new Function("0", NewVars.Distinct().ToList());

            for (int i = 0; i < a.Length; i++)
            {
                Func += (a[i] * b[i]);
            }

            return Func;
        }
        public static FunctionMatrix operator *(FunctionColVector a, FunctionRowVector b)
        {
            if (a.Length != b.Length) throw new Exception("Size Mismatch");

            List<FunctionRowVector> MatrixFuncs = new List<FunctionRowVector>();
            foreach (var aFunc in a.Vector)
            {
                List<Function> RowFuncs = new List<Function>();
                foreach (var bFunc in b.Vector)
                {
                    RowFuncs.Add(aFunc * bFunc);
                }
                MatrixFuncs.Add((FunctionRowVector)(RowFuncs.ToArray()));
            }

            return new FunctionMatrix(MatrixFuncs.ToArray());
        }

        public static Function operator *(Vector a, FunctionColVector b)
        {
            if (a.Count != b.Length) throw new Exception("Size Mismatch");

            Function Func = new Function("0", b.Variables);

            for (int i = 0; i < a.Count; i++)
            {
                Func += (a[i] * b[i]);
            }

            return Func;
        }
        public static FunctionMatrix operator *(FunctionColVector a, Vector b)
        {
            if (a.Length != b.Count) throw new Exception("Size Mismatch");

            List<FunctionRowVector> MatrixFuncs = new List<FunctionRowVector>();
            foreach (var aFunc in a.Vector)
            {
                List<Function> RowFuncs = new List<Function>();
                foreach (var bFunc in b.ToArray())
                {
                    RowFuncs.Add(aFunc * bFunc);
                }
                MatrixFuncs.Add((FunctionRowVector)(RowFuncs.ToArray()));
            }

            return new FunctionMatrix(MatrixFuncs.ToArray());
        }

        public static FunctionColVector operator *(Matrix<double> a, FunctionColVector b)
        {
            if (a.ColumnCount != b.Length) throw new Exception("Size Mismatch");
            var rows = a.ToRowArrays();

            List<Function> Funcs = new List<Function>();
            foreach (var row in rows)
            {
                Function func = 0;
                for (int i = 0; i < row.Length; i++)
                {
                    var tmp = (b[i] * row[i]);
                    func += tmp;
                }
                Funcs.Add(func);
            }
            return new FunctionColVector(Funcs.ToArray());
        }

        public static FunctionColVector operator *(double a, FunctionColVector b)
        {
            List<Function> funcs = new List<Function>();
            foreach (var func in b.Vector)
            {
                funcs.Add(func.Copy() * a);
            }
            return new FunctionColVector(funcs.ToArray());
        }
        public static FunctionColVector operator *(FunctionColVector b, double a)
        {
            return b * a;
        }
        public static FunctionColVector operator +(FunctionColVector a, FunctionColVector b)
        {
            if (a.Length != b.Length) throw new Exception("Size Mismatch");

            List<Function> funcs = new List<Function>();
            for (int i = 0; i < a.Length; i++)
            {
                funcs.Add(a[i] + b[i]);
            }

            return new FunctionColVector(funcs.ToArray());
        }

        public static FunctionColVector operator +(Vector<double> a, FunctionColVector b)
        {
            if (a.Count != b.Length) throw  new Exception("Size Mismatch");

            List<Function> funcs = new List<Function>();
            for (int i = 0; i < a.Count; i++)
            {
                funcs.Add(a[i] + b[i]);
            }

            return new FunctionColVector(funcs.ToArray());
        }

        public static FunctionColVector operator +(FunctionColVector a, Vector<double> b)
        {
            if (a.Length != b.Count) throw new Exception("Size Mismatch");

            return b + a;
        }
        #endregion
    }
}
