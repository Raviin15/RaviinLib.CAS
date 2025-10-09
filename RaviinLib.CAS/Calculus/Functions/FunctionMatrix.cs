using MathNet.Numerics.LinearAlgebra;

using System;
using System.Linq;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public class FunctionMatrix
        {

            public int Height { get => Matrix.Length; }
            public int Width { get => Matrix[0].Length; }
            public Function[][] Matrix { get; private set; }

            public FunctionMatrix(Function[][] Matrix)
            {
                this.Matrix = Matrix;
            }
            public FunctionMatrix(FunctionRowVector[] Matrix)
            {
                this.Matrix = Matrix.Select(v => v.Vector).ToArray();
            }

            public FunctionMatrix Replace(Dictionary<string, IChunk> Values)
            {
                List<FunctionRowVector> Vectors = new List<FunctionRowVector>();
                foreach (var Vector in Matrix)
                {
                    Vectors.Add((FunctionRowVector)(((FunctionRowVector)Vector).Replace(Values)));
                }
                return Vectors.ToArray();
            }

            public Matrix<double> Subs(Dictionary<string, double> Values) //double[][]
            {
                List<double[]> Vectors = new List<double[]>();
                foreach (var Vector in Matrix)
                {
                    Vectors.Add(((FunctionRowVector)Vector).Subs(Values));
                }
                return MathNet.Numerics.LinearAlgebra.Double.Matrix.Build.DenseOfRows(Vectors);

                //return Vectors.ToArray();
            }

            public FunctionMatrix Copy()
            {
                List<Function[]> NewMtx = new List<Function[]>();
                foreach (var Row in Matrix)
                {
                    List<Function> NewRow = new List<Function>();
                    foreach (var Function in Row)
                    {
                        NewRow.Add(Function.Copy());
                    }
                    NewMtx.Add(NewRow.ToArray());
                }

                return new FunctionMatrix(NewMtx.ToArray());
            }

            public FunctionColVector[] GetColVect()
            {
                List<FunctionColVector> rows = new List<FunctionColVector>();
                for (int i = 0; i < Matrix.Length; i++)
                {
                    List<Function> rowfuncs = new List<Function>();
                    for (int j = 0; j < Matrix.Length; j++)
                    {
                        rowfuncs.Add(Matrix[j][i]);
                    }
                    rows.Add((FunctionColVector)rowfuncs.ToArray());
                }
                return rows.ToArray();
            }


            public static FunctionMatrix GetZero(int Width, int? Height = null)
            {
                Function zero = 0;

                List<FunctionRowVector> Matrix = new List<FunctionRowVector>();
                for (int i = 0; i < Width; i++)
                {
                    List<Function> RowFunctions = new List<Function>();
                    for (int j = 0; j < (Height ?? Width); j++)
                    {
                        RowFunctions.Add(zero.Copy());
                    }
                    Matrix.Add((FunctionRowVector)RowFunctions.ToArray());
                }
                return new FunctionMatrix(Matrix.ToArray());

            }
            public static FunctionMatrix GetIdentity(int Width)
            {
                Function zero = 0;
                Function one = 1;

                List<FunctionRowVector> Matrix = new List<FunctionRowVector>();
                for (int i = 0; i < Width; i++)
                {
                    List<Function> RowFunctions = new List<Function>();
                    for (int j = 0; j < Width; j++)
                    {
                        if (i == j) RowFunctions.Add(one.Copy());
                        else RowFunctions.Add(zero.Copy());
                    }
                    Matrix.Add((FunctionRowVector)RowFunctions.ToArray());
                }
                return new FunctionMatrix(Matrix.ToArray());

            }

            public static FunctionMatrix Diag(Function[] Funcs)
            {
                Function zero = 0;
                Function one = 1;

                List<FunctionRowVector> Matrix = new List<FunctionRowVector>();
                for (int i = 0; i < Funcs.Length; i++)
                {
                    List<Function> RowFunctions = new List<Function>();
                    for (int j = 0; j < Funcs.Length; j++)
                    {
                        if (i == j) RowFunctions.Add(Funcs[i]);
                        else RowFunctions.Add(zero.Copy());
                    }
                    Matrix.Add((FunctionRowVector)RowFunctions.ToArray());
                }
                return new FunctionMatrix(Matrix.ToArray());
            }




            public override string ToString()
            {
                string returnstring = string.Empty;
                foreach (Function[] item in Matrix)
                {
                    returnstring += (FunctionRowVector)item + "\n";
                }
                return returnstring;
            }

            public static implicit operator FunctionMatrix(Function[][] a)
            {
                return new FunctionMatrix(a);
            }

            public static implicit operator FunctionMatrix(FunctionRowVector[] a)
            {
                return new FunctionMatrix(a);
            }


            public static FunctionMatrix operator *(FunctionMatrix a, FunctionMatrix b)
            {
                if (a.Width != b.Height) throw new Exception("Size Mismatch");

                throw new Exception("NotImplementated");
            }
            public static FunctionColVector operator *(FunctionMatrix a, FunctionColVector b)
            {
                if (a.Width != b.Length) throw new Exception("Size Mismatch");
                var rows = a.Matrix;

                List<Function> Funcs = new List<Function>();
                foreach (var row in rows)
                {
                    Function func = 0;
                    for (int i = 0; i < row.Length; i++)
                    {
                        func += (b[i] * row[i]);
                    }
                    Funcs.Add(func);
                }
                return new FunctionColVector(Funcs.ToArray());
            }
            public static FunctionRowVector operator *(FunctionRowVector a, FunctionMatrix b)
            {
                if (a.Length != b.Height) throw new Exception("Size Mismatch");
                var cols = b.GetColVect();

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


        }
}
