using System;
using System.Linq;
using System.Collections.Generic;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

using static RaviinLib.CAS.Chunker;

namespace RaviinLib.CAS
{
    public class Function
    {
        #region Replace Shorthand
        /// <summary>
        /// Returns a <see cref="Function"/> representing this.Replace(Variable, Val).
        /// </summary>
        /// <param name="Variable">The name of the variable to be replaced.</param>
        /// <param name="Val">The function or value to replace the specified variable with.</param>
        /// <returns></returns>
        public Function this[string Variable, Function Val] { get => Replace(Variable, Val); }
        #endregion

        #region Parameters

        #region True Parameters
        private IChunk _IFunction;
        /// <summary>
        /// Gets the inner IChunk representing the current function.
        /// </summary>
        public IChunk IFunction
        {
            get => _IFunction;
            private set
            {
                _IFunction = value;
                _Gradiant = null;
                _Hessian = null;
                _Simplified = null;
                _Expanded = null;
                _SystemForm = null;
                _Separable = null;
                _Latex = null;
            }
        }
        /// <summary>
        /// Gets the list of variables included in the function.
        /// </summary>
        public List<string> Variables { get; set; }
        #endregion

        #region Cached Parameters
        private Function[] _Gradiant = null;
        /// <summary>
        /// Gets the gradient of the function with respect to its variables.
        /// </summary>
        /// <remarks>The value is lazily computed on first access and cached for subsequent calls.</remarks>
        public Function[] Gradiant
        {
            get
            {
                if (_Gradiant == null) _Gradiant = GetGradiant();
                return _Gradiant;
            }
        }

        private Function[][] _Hessian = null;
        /// <summary>
        /// Gets the Hessian matrix, represented as a two-dimensional array of functions.
        /// </summary>
        /// <remarks>The value is lazily computed on first access and cached for subsequent calls.</remarks>
        public Function[][] Hessian
        {
            get
            {
                if (_Hessian == null) _Hessian = GetHessian();
                return _Hessian;
            }
        }


        private Function _Simplified = null;
        /// <summary>
        /// Gets the simplified representation of the function.
        /// </summary>
        /// <remarks>The value is lazily computed on first access and cached for subsequent calls.</remarks>
        public Function Simplified
        {
            get
            {
                if (_Simplified == null) _Simplified = GetSimplified();
                return _Simplified;
            }
        }

        private Function _Expanded = null;
        /// <summary>
        /// Gets the expanded representation of the function.
        /// </summary>
        /// <remarks>The value is lazily computed on first access and cached for subsequent calls.</remarks>
        public Function Expanded
        {
            get
            {
                if (_Expanded == null) _Expanded = GetExpanded();
                return _Expanded;
            }
        }

        private (Vector<double> LHS, double RHS)? _SystemForm = null;
        /// <summary>
        /// Gets the system of equations in the form of a tuple containing the left-hand side (LHS) vector  and the
        /// right-hand side (RHS) scalar.
        /// </summary>
        /// <remarks>The value is lazily computed on first access and cached for subsequent calls.</remarks>
        public (Vector<double> LHS, double RHS) SystemForm
        {
            get
            {
                if (_SystemForm == null) _SystemForm = GetSystemForm(Variables);
                return _SystemForm.Value;
            }
        }

        private Function[] _Separable = null;
        /// <summary>
        /// Gets an array of functions representing the separable components of the current object.
        /// </summary>
        /// <remarks>The value is lazily computed on first access and cached for subsequent calls.</remarks>
        public Function[] Separable
        {
            get
            {
                if (_Separable == null) _Separable = GetSeparable();
                return _Separable;
            }
        }

        private string _Latex = null;
        /// <summary>
        /// Gets the LaTeX representation of the object.
        /// </summary>
        /// <remarks>The value is lazily computed on first access and cached for subsequent calls.</remarks>
        public string Latex
        {
            get
            {
                if (_Latex == null) _Latex = ToLatex();
                return _Latex;
            }
        }
        #endregion

        #endregion

        #region Constructors
        public Function(string Fx, List<string> Variables = null)
        {
            if (Variables == null || Variables.Count == 0) Variables = GetVariables(Fx);

            IFunction = Chunckify(Fx) ?? new BaseChunk(0, null, 1);
            this.Variables = Variables;
        }
        public Function(IChunk Fx, List<string> Variables = null)
        {
            if (Variables == null || Variables.Count == 0) Variables = GetVariables(Fx.ToString());

            IFunction = Fx;
            this.Variables = Variables;
        }
        #endregion

        #region Derivative
        /// <summary>
        /// Computes the derivative of the function with respect to the specified variable.
        /// </summary>
        /// <param name="Var">The variable with respect to which the derivative is calculated.</param>
        /// <returns>A new <see cref="Function"/> representing the derivative of the current function with respect to the
        /// specified variable.</returns>
        public virtual Function Derivative(string Var)
        {
            return new Function(IFunction.Derivative(Var), Variables);
        }

        /// <summary>
        /// Returns the gradiant of the function with respect to the specified variables.
        /// </summary>
        /// <param name="Variables">Defaults to this.Variables</param>
        /// <returns>The gradiant of the function. In the form of Function[]</returns>
        public virtual Function[] GetGradiant(List<string> Variables = null)
        {
            if (Variables == null) Variables = this.Variables;

            List<Function> Funcs = new List<Function>() { Capacity = Variables.Count };
            foreach (var Var in Variables)
            {
                Funcs.Add(new Function(IFunction.Derivative(Var), Variables));
            }
            return Funcs.ToArray();
        }

        public virtual Function[][] GetHessian()
        {
            return GetGradiant().Select(F => F.GetGradiant()).ToArray();
        }
        #endregion

        #region Antiderivative

        public Function Antiderivative(string Var)
        {
            return new Function(IFunction.Antiderivative(Var), Variables);
        }

        #endregion

        #region Misc Methods

        /// <summary>
        /// Creates a new <see cref="Function"/> instance that is a copy of the current instance.
        /// </summary>
        /// <returns>A new <see cref="Function"/> object that contains the same internal state as the current instance.</returns>
        public Function Copy()
        {
            return new Function(IFunction.Copy(), new List<string>(Variables));
        }

        /// <summary>
        /// Creates a new <see cref="Function"/> instance with the expanded form of the current function.
        /// </summary>
        /// <returns>A new <see cref="Function"/> instance representing the expanded form of the current function.</returns>
        public Function GetExpanded()
        {
            return new Function(IFunction.Expanded(), new List<string>(Variables));
        }

        /// <summary>
        /// Converts the current function into a linear system representation in the form of a left-hand side (LHS)
        /// vector and a right-hand side (RHS) scalar.
        /// </summary>
        /// <remarks>This method assumes that the function is linear. If the function is not
        /// linear, an exception is thrown.  The method processes the function by simplifying it and extracting
        /// coefficients for the specified variables.</remarks>
        /// <returns>A tuple containing: <list type="bullet"><item> <description><see cref="Vector{double}"/> LHS: A dense
        /// vector where each element corresponds to the coefficient of the respective variable in this.Variables/>.
        /// </description> </item> <item> <description> <see langword="double"/> RHS: The constant term of the
        /// equation, negated. </description> </item> </list></returns>
        public (Vector<double> LHS, double RHS) GetSystemForm()
        {
            var simp = IFunction.Simplified();
            if (simp == null) return (Vector.Build.Dense(1, 0), 0);
            if (simp is SumChunk)
            {
                var Func = (SumChunk)simp;

                //var Base = Func.Chunks.Where(c=> c is BaseChunk b && b.Var == null).FirstOrDefault(new BaseChunk(0,null,1));

                List<double> Num = new List<double>();
                Dictionary<string, double> Vars = new Dictionary<string, double>();
                foreach (var item in Func.Chunks)
                {

                    if (!(item is BaseChunk b) || b.Exp != 1) throw new Exception("Function is not Linear.");

                    if (b.Var != null && Vars.ContainsKey(b.Var)) throw new Exception();

                    if (b.Var != null) Vars.Add(b.Var, b.Coeff);
                    else Num.Add(b.Coeff);
                }

                List<double> Values = new List<double>();
                foreach (var var in Variables)
                {
                    if (!Vars.ContainsKey(var)) Values.Add(0);
                    else Values.Add(Vars[var]);
                }

                return (Vector.Build.DenseOfEnumerable(Values), Num.Sum() * -1);
            }
            else if (simp is BaseChunk)
            {
                var Func = (BaseChunk)simp;

                List<double> Num = new List<double>();
                Dictionary<string, double> Vars = new Dictionary<string, double>();

                if (Func.Exp != 1) throw new Exception("Function is not Linear.");

                if (Func.Var != null && Vars.ContainsKey(Func.Var)) throw new Exception();

                if (Func.Var != null) Vars.Add(Func.Var, Func.Coeff);
                else Num.Add(Func.Coeff);

                List<double> Values = new List<double>();
                foreach (var var in Variables)
                {
                    if (!Vars.ContainsKey(var)) Values.Add(0);
                    else Values.Add(Vars[var]);
                }

                return (Vector.Build.DenseOfEnumerable(Values), Num.Sum() * -1);
            }
            throw new Exception("Function is not Linear.");
        }
        /// <summary>
        /// Converts the current function into a linear system representation in the form of a left-hand side (LHS)
        /// vector and a right-hand side (RHS) scalar.
        /// </summary>
        /// <remarks>This method assumes that the function is linear. If the function is not
        /// linear, an exception is thrown.  The method processes the function by simplifying it and extracting
        /// coefficients for the specified variables.</remarks>
        /// <param name="Variables">A list of variable names to include in the LHS vector. The order of variables in this list determines
        /// the order of coefficients in the resulting vector.</param>
        /// <returns>A tuple containing: <list type="bullet"><item> <description><see cref="Vector{double}"/> LHS: A dense
        /// vector where each element corresponds to the coefficient of the respective variable in <paramref
        /// name="Variables"/>.  If a variable is not present in the function, its coefficient will be 0.
        /// </description> </item> <item> <description> <see langword="double"/> RHS: The constant term of the
        /// equation, negated. </description> </item> </list></returns>
        public (Vector<double> LHS, double RHS) GetSystemForm(List<string> Variables = null)
        {
            if (Variables == null) Variables = this.Variables;

            var simp = IFunction.Simplified();
            if (simp == null) return (Vector.Build.Dense(1, 0), 0);
            if (simp is SumChunk)
            {
                var Func = (SumChunk)simp;

                //var Base = Func.Chunks.Where(c=> c is BaseChunk b && b.Var == null).FirstOrDefault(new BaseChunk(0,null,1));

                List<double> Num = new List<double>();
                Dictionary<string, double> Vars = new Dictionary<string, double>();
                foreach (var item in Func.Chunks)
                {

                    if (!(item is BaseChunk b) || b.Exp != 1) throw new Exception("Function is not Linear.");

                    if (b.Var != null && Vars.ContainsKey(b.Var)) throw new Exception();

                    if (b.Var != null) Vars.Add(b.Var, b.Coeff);
                    else Num.Add(b.Coeff);
                }

                List<double> Values = new List<double>();
                foreach (var var in Variables)
                {
                    if (!Vars.ContainsKey(var)) Values.Add(0);
                    else Values.Add(Vars[var]);
                }

                return (Vector.Build.DenseOfEnumerable(Values), Num.Sum() * -1);
            }
            else if (simp is BaseChunk)
            {
                var Func = (BaseChunk)simp;

                List<double> Num = new List<double>();
                Dictionary<string, double> Vars = new Dictionary<string, double>();

                if (Func.Exp != 1) throw new Exception("Function is not Linear.");

                if (Func.Var != null && Vars.ContainsKey(Func.Var)) throw new Exception();

                if (Func.Var != null) Vars.Add(Func.Var, Func.Coeff);
                else Num.Add(Func.Coeff);

                List<double> Values = new List<double>();
                foreach (var var in Variables)
                {
                    if (!Vars.ContainsKey(var)) Values.Add(0);
                    else Values.Add(Vars[var]);
                }

                return (Vector.Build.DenseOfEnumerable(Values), Num.Sum() * -1);
            }
            throw new Exception("Function is not Linear.");
        }

        /// <summary>
        /// Generates a piecewise linear approximation of a separable function over a specified range and step size.
        /// </summary>
        /// <remarks>This method constructs a piecewise linear approximation by evaluating the
        /// separable function at discrete points within the specified range. Each segment of the approximation
        /// corresponds to a linear combination of coefficients derived from the function's evaluation.</remarks>
        /// <param name="LowerBound">The lower bound of the range over which the approximation is calculated.</param>
        /// <param name="Upperbound">The upper bound of the range over which the approximation is calculated.</param>
        /// <param name="StepSize">The step size used to divide the range into discrete intervals for the approximation. Must be positive.</param>
        /// <returns>A tuple containing the following: <list type="bullet"> <item> <description> <see cref="Function"/>: A
        /// piecewise linear function representing the approximation of the separable function. </description>
        /// </item> <item> <description> <see cref="Dictionary{string, Function}"/>: A dictionary mapping the
        /// original variables of the separable function to their corresponding linear approximations.
        /// </description> </item> </list></returns>
        public (Function LinearFunc, Dictionary<string, Function> OrigVarEquivilents) GetPiecewiseLinearAprox(double LowerBound, double Upperbound, double StepSize)
        {
            List<IChunk> SeparationAproxs = new List<IChunk>();
            List<string> Variables = new List<string>();
            Dictionary<string, Function> OrigVarEquivilents = new Dictionary<string, Function>();

            var index = 1;
            foreach (var SepIFunc in GetSeparable())
            {
                string OrigVarEquivilent = "";
                List<IChunk> BaseChunks = new List<IChunk>();
                List<string> LocalVariables = new List<string>();
                for (double i = LowerBound; i <= Upperbound; i += StepSize)
                {

                    var coeff = SepIFunc.Subs(SepIFunc.GetDict(new double[] { i })); //Enumerable.Repeat(i, Variables.Count)
                    string var = $"δ{index}";

                    BaseChunks.Add(new BaseChunk(coeff, var, 1));
                    LocalVariables.Add(var);
                    OrigVarEquivilent += $"+ {i}{var}";
                    index++;
                }
                Variables.AddRange(LocalVariables);
                SeparationAproxs.Add(new SumChunk(BaseChunks));
                OrigVarEquivilents[SepIFunc.Variables.First()] = new Function(OrigVarEquivilent.Remove(0, 2), LocalVariables);
            }

            return (new Function(new SumChunk(SeparationAproxs), Variables), OrigVarEquivilents);
        }

        /// <summary>
        /// Analyzes the current function and attempts to decompose it into separable components.
        /// </summary>
        /// <remarks>This method identifies whether the current function can be expressed as a sum
        /// of independent sub-functions,  each depending on a distinct variable or no variable at all. If the
        /// function is separable, it returns an array  of these sub-functions. If the function is not separable, an
        /// exception is thrown.</remarks>
        /// <returns>An array of <see cref="Function"/> objects, where each object represents a separable component of the
        /// original function.</returns>
        public Function[] GetSeparable()
        {
            if (IFunction is SumChunk s)
            {
                Dictionary<string, List<IChunk>> DistincVariables = new Dictionary<string, List<IChunk>>();

                foreach (var c in s.Chunks)
                {
                    var Var = GetVariables(c.ToString());

                    if (Var.Count > 1) throw new Exception("Function is not separable.");

                    string Key = "";
                    if (Var.Count == 1) Key = Var.First();

                    if (!DistincVariables.ContainsKey(Key)) DistincVariables.Add(Key, new List<IChunk>());
                    DistincVariables[Key].Add(c);
                }

                return DistincVariables.Select(c => new Function(new SumChunk(c.Value), (c.Key == null) ? new List<string>() : new List<string>() { c.Key })).ToArray();
            }
            else if (IFunction is ProductChunk p)
            {

            }
            throw new Exception("Function is not separable.");
        }


        public Function GetTaylorAproximation(double InitialValue, int Order = 10, bool AproximateFactorial = false)
        {
            if (Variables.Count > 1) throw new Exception("Can not get the aproximation with respect to mroe than one variable.");

            var Comparer = new IChunkComparer();

            Func<double, double> Factorial = (double n) => // Dalton Helped
            {
                if (n < 0)
                    throw new ArgumentException("Negative numbers are not allowed.");

                double result = n;
                for (double i = n - 1; i >= 1; i--)
                    result *= i;

                return result;
            };

            Func<double, double> FactorialAprox = (double n) =>
            {
                return Math.Sqrt((2 * Math.PI * n) * (Math.Pow(n / Math.E, n)));
            };

            Function init = Subs(InitialValue);

            var prevDeriv = Copy(); //Anything
            int LoopCount = 1;
            Func<double, double> FactorialFunc = (AproximateFactorial) ? FactorialAprox : Factorial;
            do
            {
                prevDeriv = prevDeriv.Derivative(Variables[0]);
                var subsVal = prevDeriv.Subs(InitialValue);
                var fact = FactorialFunc(LoopCount);
                init += (subsVal / fact) * ((new Function(Variables[0]) - InitialValue) ^ LoopCount);
                LoopCount++;
            } while (LoopCount <= Order); // (!Comparer.Equals(prevDeriv.IFunction, new BaseChunk(0,null,1)) &&

            return init;
        }

        #endregion

        #region Subs
        public double Subs(double Val)
        {
            return Subs(GetDict(new double[] { Val }));
        }
        public double Subs(IEnumerable<double> Vals)
        {
            return Subs(GetDict(Vals));
        }
        public double Subs(Dictionary<string, double> Vals)
        {
            return IFunction.Subs(Vals);
        }
        public double SubsAll(double Val)
        {
            return Subs(GetDict(Enumerable.Repeat(Val, Variables.Count)));
        }
        public double[] SubsGrad(double Val)
        {
            return SubsGrad(GetDict(new double[] { Val }));
        }
        public double[] SubsGrad(IEnumerable<double> Vals)
        {
            var Dict = GetDict(Vals);
            return SubsGrad(Dict);
        }
        public double[] SubsGrad(Dictionary<string, double> Vals)
        {
            List<double> doubles = new List<double>();
            foreach (var Gx in Gradiant)
            {
                doubles.Add(Gx.Subs(Vals));
            }

            return doubles.ToArray();
        }
        public double[] SubsGradAll(double Val)
        {
            var Dict = GetDict(Enumerable.Repeat(Val, Variables.Count));
            return SubsGrad(Dict);
        }
        public double[][] SubsHess(double Val)
        {
            return SubsHess(GetDict(new double[] { Val }));
        }
        public double[][] SubsHess(IEnumerable<double> Vals)
        {
            var Dict = GetDict(Vals);
            return SubsHess(Dict);
        }
        public double[][] SubsHess(Dictionary<string, double> Vals)
        {
            List<double[]> doubles = new List<double[]>();
            foreach (var Hx in Hessian)
            {
                List<double> innerdoubles = new List<double>();
                foreach (var Gx in Hx)
                {
                    innerdoubles.Add(Gx.Subs(Vals));
                }
                doubles.Add(innerdoubles.ToArray());
            }

            return doubles.ToArray();
        }
        public double[][] SubsHessAll(double Val)
        {
            var Dict = GetDict(Enumerable.Repeat(Val, Variables.Count));
            return SubsHess(Dict);
        }

        private Dictionary<string, double> GetDict(IEnumerable<double> Vals)
        {
            if (Vals.Count() != Variables.Count) throw new Exception("Invalid Number of Values.");

            Dictionary<string, double> Dict = new Dictionary<string, double>();

            for (int i = 0; i < Variables.Count; i++)
            {
                Dict.Add(Variables[i], Vals.ElementAt(i));
            }
            return Dict;
        }
        private Dictionary<string, IChunk> GetDict(IEnumerable<Function> Vals)
        {
            if (Vals.Count() != Variables.Count) throw new Exception("Invalid Number of Values.");

            Dictionary<string, IChunk> Dict = new Dictionary<string, IChunk>();

            for (int i = 0; i < Variables.Count; i++)
            {
                Dict.Add(Variables[i], Vals.ElementAt(i).IFunction);
            }
            return Dict;
        }
        #endregion

        #region Replace

        public Function Replace(string Variable, Function Val)
        {
            return Replace(new Dictionary<string, IChunk>() { { Variable, Val.IFunction } });
        }
        public Function Replace(IEnumerable<Function> Vals)
        {
            return Replace(GetDict(Vals));
        }
        public Function Replace(Dictionary<string, Function> Vals)
        {
            return Replace(Vals.ToDictionary(p => p.Key, p => p.Value.IFunction));
        }
        public Function Replace(Dictionary<string, IChunk> Vals)
        {
            return new Function(IFunction.Replace(Vals), Variables);
        }
        public Function[] ReplaceGrad(string Variable, Function Val)
        {
            return ReplaceGrad(new Dictionary<string, IChunk>() { { Variable, Val.IFunction } });
        }
        public Function[] ReplaceGrad(IEnumerable<Function> Vals)
        {
            var Dict = GetDict(Vals);

            return ReplaceGrad(Dict);
        }
        public Function[] ReplaceGrad(Dictionary<string, Function> Vals)
        {
            return ReplaceGrad(Vals.ToDictionary(p => p.Key, p => p.Value.IFunction));
        }
        public Function[] ReplaceGrad(Dictionary<string, IChunk> Vals)
        {
            List<Function> doubles = new List<Function>();
            foreach (var Gx in Gradiant)
            {
                doubles.Add(Replace(Vals));
            }

            return doubles.ToArray();
        }
        public Function[][] ReplaceHess(string Variable, Function Val)
        {
            return ReplaceHess(new Dictionary<string, IChunk>() { { Variable, Val.IFunction } });
        }
        public Function[][] ReplaceHess(IEnumerable<Function> Vals)
        {
            var Dict = GetDict(Vals);
            return ReplaceHess(Dict);
        }
        public Function[][] ReplaceHess(Dictionary<string, Function> Vals)
        {
            return ReplaceHess(Vals.ToDictionary(p => p.Key, p => p.Value.IFunction));
        }
        public Function[][] ReplaceHess(Dictionary<string, IChunk> Vals)
        {
            List<Function[]> doubles = new List<Function[]>();
            foreach (var Hx in Hessian)
            {
                doubles.Add(ReplaceGrad(Vals));
            }

            return doubles.ToArray();
        }

        #endregion

        #region Simplify
        /// <summary>
        /// Sets this.IFunction property to the simplified version of itself.
        /// Forces quick properties to be recalculated.
        /// </summary>
        public void Simplify()
        {
            IFunction = Simplified.IFunction.Copy();
        }

        /// <summary>
        /// Recalculates and returns a simplified version of this function without modifying the original function.
        /// </summary>
        /// <returns>The simplified function.</returns>
        public Function GetSimplified() => new Function(IFunction.Simplified() ?? new BaseChunk(0, null, 1), new List<string>(Variables));
        #endregion

        #region Overides
        /// <summary>
        /// Returns a string representation of the current object.
        /// </summary>
        /// <remarks>If the underlying function is a <see cref="SumChunk"/>, the returned string
        /// may have its outer parentheses removed,  depending on the format of the string representation.
        /// Otherwise, the default string representation of the function  is returned, or "RaviinLib.CAS.Function+IChunk"
        /// if no representation is available.</remarks>
        /// <returns>A string that represents the current object. The format of the string depends on the type of the
        /// underlying function.</returns>
        public override string ToString()
        {
            string String = IFunction.ToString() ?? "RaviinLib.CAS.Function+IChunk";

            if (IFunction is SumChunk) return (String.StartsWith("(") && String.EndsWith(")")) ? String.Substring(1, String.Length - 2) : String;

            return String;
                
        }

        /// <summary>
        /// Converts the function into LaTex format.
        /// </summary>
        /// <returns>Latex form of the function</returns>
        public string ToLatex()
        {
            string Latex = IFunction.ToLatex();

            if (IFunction is SumChunk) return (Latex.StartsWith(@"\left(") && Latex.EndsWith(@"\right)")) ? Latex.Substring(6, Latex.Length - 13) : Latex;

            return Latex;
        }

        #region Function,Function
        public static Function operator *(Function a, Function b)
        {
            List<string> NewVars = new List<string>(a.Variables);
            NewVars.AddRange(b.Variables);
            return new Function(new ProductChunk(a.IFunction.Copy(), b.IFunction.Copy()), NewVars.Distinct().ToList());
        }
        public static Function operator /(Function a, Function b)
        {
            List<string> NewVars = new List<string>(a.Variables);
            NewVars.AddRange(b.Variables);
            return new Function(new ProductChunk(a.IFunction.Copy(), new ChainChunk(1,b.IFunction.Copy(), new BaseChunk(-1,null,1))), NewVars.Distinct().ToList());
        }
        public static Function operator +(Function a, Function b)
        {
            List<string> NewVars = new List<string>(a.Variables);
            NewVars.AddRange(b.Variables);
            return new Function(new SumChunk(new List<IChunk>() { a.IFunction.Copy(), b.IFunction.Copy() }), NewVars.Distinct().ToList());
        }
        public static Function operator -(Function a, Function b)
        {
            List<string> NewVars = new List<string>(a.Variables);
            NewVars.AddRange(b.Variables);

            var bcopy = b.IFunction.Copy();
            bcopy.Coeff *= -1;
            return new Function(new SumChunk(new List<IChunk>() { a.IFunction.Copy(), bcopy }), NewVars.Distinct().ToList());
        }
        #endregion

        #region double,Function
        public static Function operator *(double a, Function b)
        {
            var bcopy = b.IFunction.Copy();
            bcopy.Coeff *= a;

            return new Function(bcopy, new List<string>(b.Variables));
        }
        public static Function operator /(double a, Function b)
        {
            return new Function(new ProductChunk(new BaseChunk(a,null,1),new ChainChunk(1, b.IFunction.Copy(), new BaseChunk(-1, null, 1))), new List<string>(b.Variables));
        }
        public static Function operator +(double a, Function b)
        {
            return new Function(new SumChunk(new List<IChunk>() { new BaseChunk(a, null, 1), b.IFunction.Copy() }), new List<string>(b.Variables));
        }
        public static Function operator -(double a, Function b)
        {
            var bcopy = b.IFunction.Copy();
            bcopy.Coeff *= -1;
            return new Function(new SumChunk(new List<IChunk>() { new BaseChunk(a, null, 1), bcopy }), new List<string>(b.Variables));
        }
        #endregion

        #region Function,double
        public static Function operator *(Function a, double b)
        {
            var acopy = a.IFunction.Copy();
            acopy.Coeff *= b;

            return new Function(acopy, new List<string>(a.Variables));
        }
        public static Function operator /(Function a, double b)
        {
            var acopy = a.IFunction.Copy();
            acopy.Coeff /= b;

            return new Function(acopy, new List<string>(a.Variables));
        }
        public static Function operator +(Function a, double b)
        {
            return new Function(new SumChunk(new List<IChunk>() { new BaseChunk(b, null, 1), a.IFunction.Copy() }), new List<string>(a.Variables));
        }
        public static Function operator -(Function a, double b)
        {
            return new Function(new SumChunk(new List<IChunk>() { new BaseChunk(-b, null, 1), a.IFunction.Copy() }), new List<string>(a.Variables));
        }
        public static Function operator ^(Function a, double b)
        {
            return new Function( new ChainChunk(1,a.IFunction.Copy(), new BaseChunk(b, null, 1)), new List<string>(a.Variables));
        }
        #endregion

        #region string,Function
        public static Function operator *(string a, Function b)
        {
            return new Function(a) * b;
        }
        public static Function operator /(string a, Function b)
        {
            return new Function(a) / b;
        }
        public static Function operator +(string a, Function b)
        {
            return new Function(a) + b;
        }
        public static Function operator -(string a, Function b)
        {
            return new Function(a) - b;
        }
        #endregion

        #region Function,string
        public static Function operator *(Function a, string b)
        {
            return a * new Function(b);
        }
        public static Function operator /(Function a, string b)
        {
            return a / new Function(b);
        }
        public static Function operator +(Function a, string b)
        {
            return a + new Function(b);
        }
        public static Function operator -(Function a, string b)
        {
            return a - new Function(b);
        }
        #endregion

        #region Implicit Conversion
        public static implicit operator Function(string a)
        {
            return new Function(a);
        }
        public static implicit operator Function(double a)
        {
            return new Function(a.ToString());
        }
        #endregion

        #endregion

    }
}
