using System;
using System.Linq;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public class FuncChunk : IChunk
    {
        private static readonly Dictionary<Functions, Functions> InverseFunctionPairs = new Dictionary<Functions, Functions>()
        {
            { Functions.ln,     Functions.Exp },
            { Functions.Exp,    Functions.ln },

            { Functions.Sin,    Functions.ASin },
            { Functions.ASin,   Functions.Sin },

            { Functions.Cos,    Functions.ACos },
            { Functions.ACos,   Functions.Cos },

            { Functions.Tan,    Functions.ATan },
            { Functions.ATan,   Functions.Tan },

            { Functions.Csc,    Functions.ACsc },
            { Functions.ACsc,   Functions.Csc },

            { Functions.Sec,    Functions.ASec },
            { Functions.ASec,   Functions.Sec },

            { Functions.Cot,    Functions.ACot },
            { Functions.ACot,   Functions.Cot },

            { Functions.Sinh,   Functions.ASinh },
            { Functions.ASinh,  Functions.Sinh },

            { Functions.Cosh,   Functions.ACosh },
            { Functions.ACosh,  Functions.Cosh },

            { Functions.Tanh,   Functions.ATanh },
            { Functions.ATanh,  Functions.Tanh },

            { Functions.Csch,   Functions.ACsch },
            { Functions.ACsch,  Functions.Csch },

            { Functions.Sech,   Functions.ASech },
            { Functions.ASech,  Functions.Sech },

            { Functions.Coth,   Functions.ACoth },
            { Functions.ACoth,  Functions.Coth },
        };
        private static readonly Dictionary<Functions, string> FunctionToLatex = new Dictionary<Functions, string>
        {
            { Functions.sqrt     , @"\sqrt" },
            { Functions.cbrt     , @"\sqrt[3]" },

            { Functions.Exp     , @"\exp" },
            { Functions.ln      , @"\ln" },
            { Functions.log     , @"\log" },

            { Functions.ACoth   , @"\coth^{-1}" },
            { Functions.ACsch   , @"\csch^{-1}" },
            { Functions.ASech   , @"\sech^{-1}" },

            { Functions.ATanh   , @"\tanh^{-1}" },
            { Functions.ACosh   , @"\cosh^{-1}" },
            { Functions.ASinh   , @"\sinh^{-1}" },

            { Functions.ACot    , @"\cot^{-1}" },
            { Functions.ACsc    , @"\csc^{-1}" },
            { Functions.ASec    , @"\sec^{-1}" },

            { Functions.ATan    , @"\tan^{-1}" },
            { Functions.ACos    , @"\cos^{-1}" },
            { Functions.ASin    , @"\sin^{-1}" },

            { Functions.Coth    , @"\coth" },
            { Functions.Csch    , @"\csch" },
            { Functions.Sech    , @"\sech" },

            { Functions.Tanh    , @"\tanh" },
            { Functions.Cosh    , @"\cosh" },
            { Functions.Sinh    , @"\sinh" },

            { Functions.Cot     , @"\cot" },
            { Functions.Csc     , @"\csc" },
            { Functions.Sec     , @"\sec" },

            { Functions.Tan     , @"\tan" },
            { Functions.Cos     , @"\cos" },
            { Functions.Sin     , @"\sin" },

            { Functions.Abs     , @"" },
            { Functions.Min     , @"\min" },
            { Functions.Max     , @"\max" },
        };

        public double Coeff { get; set; }
        public Functions Function { get; set; }
        public IChunk Chunk { get; set; }

        public IChunk SecondChunk { get; set; } = null;

        public FuncChunk(IChunk Chunk, Functions Function, double Coeff = 1)
        {
            this.Coeff = Coeff;
            this.Function = Function;
            this.Chunk = Chunk;
        }

        public IChunk Copy()
        {
            return new FuncChunk(Chunk.Copy(), Function, Coeff) { SecondChunk = SecondChunk?.Copy()};
        }

        public IChunk Derivative(string Var)
        {
            var ChunkDeriv = Chunk.Derivative(Var);
            bool isOne = (ChunkDeriv is BaseChunk b && b.Var == null && b.Exp == 1 && b.Coeff == 1);

            switch (Function)
            {
                case Functions.Sin:
                    if (isOne) return new FuncChunk(Chunk, Functions.Cos, Coeff);
                    return new ProductChunk(ChunkDeriv, new FuncChunk(Chunk, Functions.Cos, Coeff));
                case Functions.Cos:
                    if (isOne) return new FuncChunk(Chunk, Functions.Sin, Coeff * -1);
                    return new ProductChunk(ChunkDeriv, new FuncChunk(Chunk, Functions.Sin, Coeff * -1));
                case Functions.Tan:
                    if (isOne) return new ChainChunk(Coeff, new FuncChunk(Chunk, Functions.Sec), new BaseChunk(2, null, 1));
                    return new ProductChunk(ChunkDeriv, new ChainChunk(Coeff, new FuncChunk(Chunk, Functions.Sec), new BaseChunk(2, null, 1)));

                case Functions.Sec:
                    if (isOne) return new ProductChunk(new FuncChunk(Chunk, Functions.Sec), new FuncChunk(Chunk, Functions.Tan)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ProductChunk(new FuncChunk(Chunk, Functions.Sec), new FuncChunk(Chunk, Functions.Tan))) { Coeff = Coeff };
                case Functions.Csc:
                    if (isOne) return new ProductChunk(new FuncChunk(Chunk, Functions.Csc), new FuncChunk(Chunk, Functions.Cot)) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ProductChunk(new FuncChunk(Chunk, Functions.Csc), new FuncChunk(Chunk, Functions.Cot))) { Coeff = Coeff * -1 };
                case Functions.Cot:
                    if (isOne) return new ProductChunk(new FuncChunk(Chunk, Functions.Csc), new FuncChunk(Chunk, Functions.Csc)) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ProductChunk(new FuncChunk(Chunk, Functions.Csc), new FuncChunk(Chunk, Functions.Csc))) { Coeff = Coeff * -1 };

                case Functions.Sinh:
                    if (isOne) return new FuncChunk(Chunk.Copy(), Functions.Cosh) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new FuncChunk(Chunk.Copy(), Functions.Cosh)) { Coeff = Coeff };
                case Functions.Cosh:
                    if (isOne) return new FuncChunk(Chunk.Copy(), Functions.Sinh) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new FuncChunk(Chunk.Copy(), Functions.Sinh)) { Coeff = Coeff };
                case Functions.Tanh:
                    if (isOne) return new ChainChunk(1, new FuncChunk(Chunk.Copy(), Functions.Sech), new BaseChunk(2, null, 1)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new FuncChunk(Chunk.Copy(), Functions.Sech), new BaseChunk(2, null, 1))) { Coeff = Coeff };

                case Functions.Sech:
                    if (isOne) return new ProductChunk(new FuncChunk(Chunk.Copy(), Functions.Csch), new FuncChunk(Chunk.Copy(), Functions.Coth)) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ProductChunk(new FuncChunk(Chunk.Copy(), Functions.Csch), new FuncChunk(Chunk.Copy(), Functions.Coth))) { Coeff = Coeff * -1 };
                case Functions.Csch:
                    if (isOne) return new ProductChunk(new FuncChunk(Chunk.Copy(), Functions.Sech), new FuncChunk(Chunk.Copy(), Functions.Tanh)) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ProductChunk(new FuncChunk(Chunk.Copy(), Functions.Sech), new FuncChunk(Chunk.Copy(), Functions.Tanh))) { Coeff = Coeff * -1 };
                case Functions.Coth:
                    if (isOne) return new ChainChunk(1, new FuncChunk(Chunk.Copy(), Functions.Csch), new BaseChunk(2, null, 1)) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new FuncChunk(Chunk.Copy(), Functions.Csch), new BaseChunk(2, null, 1))) { Coeff = Coeff * -1 };

                case Functions.ASin:
                    if (isOne) return new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1))) { Coeff = Coeff };
                case Functions.ACos:
                    if (isOne) return new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1))) { Coeff = Coeff * -1 };
                case Functions.ATan:
                    if (isOne) return new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-1, null, 1)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-1, null, 1))) { Coeff = Coeff };

                case Functions.ASec:
                    var ASec = Chunk.Copy();
                    ASec.Coeff = Math.Abs(ASec.Coeff);
                    if (isOne) return new ProductChunk(new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)), new ChainChunk(1, ASec, new BaseChunk(-1, null, 1))) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ProductChunk(new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)), new ChainChunk(1, ASec, new BaseChunk(-1, null, 1)))) { Coeff = Coeff };
                case Functions.ACsc:
                    var ACsc = Chunk.Copy();
                    ACsc.Coeff = Math.Abs(ACsc.Coeff);
                    if (isOne) return new ProductChunk(new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)), new ChainChunk(1, ACsc, new BaseChunk(-1, null, 1))) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ProductChunk(new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)), new ChainChunk(1, ACsc, new BaseChunk(-1, null, 1)))) { Coeff = Coeff * -1 };
                case Functions.ACot:
                    if (isOne) return new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-1, null, 1)) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(-1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-1, null, 1))) { Coeff = Coeff * -1 };

                case Functions.ASinh:
                    if (isOne) return new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1))) { Coeff = Coeff };
                case Functions.ACosh:
                    if (isOne) return new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(-1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(-1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1))) { Coeff = Coeff };
                case Functions.ATanh:
                    if (isOne) return new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-1, null, 1)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-1, null, 1))) { Coeff = Coeff };

                case Functions.ASech:
                    if (isOne) return new ProductChunk(new ChainChunk(1, Chunk.Copy(), new BaseChunk(-1, null, 1)), new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1))) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ProductChunk(new ChainChunk(1, Chunk.Copy(), new BaseChunk(-1, null, 1)), new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)))) { Coeff = Coeff * -1 };
                case Functions.ACsch:
                    var ACsch = Chunk.Copy();
                    ACsch.Coeff = Math.Abs(ACsch.Coeff);
                    if (isOne) return new ProductChunk(new ChainChunk(1, ACsch, new BaseChunk(-1, null, 1)), new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1))) { Coeff = Coeff * -1 };
                    return new ProductChunk(ChunkDeriv, new ProductChunk(new ChainChunk(1, ACsch, new BaseChunk(-1, null, 1)), new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-0.5, null, 1)))) { Coeff = Coeff * -1 };
                case Functions.ACoth:
                    if (isOne) return new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-1, null, 1)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk(1, null, 1), new ChainChunk(1, Chunk.Copy(), new BaseChunk(2, null, 1)) }), new BaseChunk(-1, null, 1))) { Coeff = Coeff };

                case Functions.Exp:
                    if (isOne) return Copy();
                    return new ProductChunk(Copy(), ChunkDeriv);
                case Functions.ln:
                    if (isOne) return new ChainChunk(1, Chunk.Copy(), new BaseChunk(-1, null, 1)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, Chunk.Copy(), new BaseChunk(-1, null, 1))) { Coeff = Coeff };
                case Functions.log:
                    if (isOne) return new ChainChunk(1, new ProductChunk(Copy(), new FuncChunk(new BaseChunk(10, null, 1), Functions.ln)), new BaseChunk(-1, null, 1)) { Coeff = Coeff };
                    return new ProductChunk(ChunkDeriv, new ChainChunk(1, new ProductChunk(Copy(), new FuncChunk(new BaseChunk(10, null, 1), Functions.ln)), new BaseChunk(-1, null, 1))) { Coeff = Coeff };

                case Functions.sqrt:
                    if (isOne) return new ChainChunk(Coeff / 2, Chunk.Copy(), new BaseChunk(-0.5, null, 1));
                    return new ProductChunk(ChunkDeriv, new ChainChunk(Coeff / 2, Chunk.Copy(), new BaseChunk(-0.5, null, 1)));
                case Functions.cbrt:
                    if (isOne) return new ChainChunk(Coeff / 3, Chunk.Copy(), new BaseChunk(-2.0 / 3.0, null, 1));
                    return new ProductChunk(ChunkDeriv, new ChainChunk(Coeff / 3, Chunk.Copy(), new BaseChunk(-2.0 / 3.0, null, 1)));

                case Functions.Abs:
                    //if (Chunk is BaseChunk b && b.Exp == 1)
                    //{
                    //    var ret = Chunk.Derivative(Var);
                    //    ret.Coeff = Math.Abs(ret.Coeff);
                    //    ret.Multiply(Coeff);
                    //    return ret;
                    //}
                    throw new Exception("Undiffernentiable (Abs)");
                //    return new DiffChunk(this.Copy(), new BaseChunk(1,Var,1)) { Coeff = Coeff };
                case Functions.Min:
                    throw new Exception("Undiffernentiable (Min)");
                //    return new DiffChunk(this.Copy(), new BaseChunk(1, Var, 1)) { Coeff = Coeff };
                case Functions.Max:
                    throw new Exception("Undiffernentiable (Max)");
                //    return new DiffChunk(this.Copy(), new BaseChunk(1, Var, 1)) { Coeff = Coeff };


                default:
                    throw new Exception("Invalid Function");
            }
        }

        public void Multiply(double factor)
        {
            Coeff *= factor;
        }
        public void MultiplyExpanded(double factor)
        {
            Multiply(factor);
        }

        public IChunk Simplified()
        {

            if (Chunk is FuncChunk)
            {
                return ReduceAntiFunctions(this);
            }

            var a = Chunk.Simplified();
            if (a == null) return new FuncChunk(new BaseChunk(0, null, 1), Function, Coeff) { SecondChunk = SecondChunk?.Simplified() };  //  || (a is BaseChunk b && b.Var == null) return new BaseChunk(this.Subs(new()),null,1);

            return new FuncChunk(a, Function, Coeff) { SecondChunk = SecondChunk?.Simplified() };
        }

        private IChunk ReduceAntiFunctions(FuncChunk Chunk)
        {
            var InnerFunc = (FuncChunk)Chunk.Chunk;

            if (IsInversePair(Chunk.Function, InnerFunc.Function))
            {
                return InnerFunc.Chunk.Copy().Simplified();
            }

            return Chunk.Copy();
        }

        private bool IsInversePair(Functions OuterFunc, Functions InnerFunc)
        {
            return InverseFunctionPairs.TryGetValue(OuterFunc, out var inverse) && inverse == InnerFunc;
        }

        public double Subs(Dictionary<string, double> Values)
        {
            switch (Function)
            {
                case Functions.Sin:
                    return Math.Sin(Chunk.Subs(Values)) * Coeff;
                case Functions.Cos:
                    return Math.Cos(Chunk.Subs(Values)) * Coeff;
                case Functions.Tan:
                    return Math.Tan(Chunk.Subs(Values)) * Coeff;

                case Functions.Sec:
                    return Coeff / Math.Cos(Chunk.Subs(Values));
                case Functions.Csc:
                    return Coeff / Math.Sin(Chunk.Subs(Values));
                case Functions.Cot:
                    return Coeff / Math.Tan(Chunk.Subs(Values));

                case Functions.Sinh:
                    return Math.Sinh(Chunk.Subs(Values)) * Coeff;
                case Functions.Cosh:
                    return Math.Cosh(Chunk.Subs(Values)) * Coeff;
                case Functions.Tanh:
                    return Math.Tanh(Chunk.Subs(Values)) * Coeff;

                case Functions.Sech:
                    return Coeff / Math.Cosh(Chunk.Subs(Values));
                case Functions.Csch:
                    return Coeff / Math.Sinh(Chunk.Subs(Values));
                case Functions.Coth:
                    return Coeff / Math.Tanh(Chunk.Subs(Values));

                case Functions.ASin:
                    return Math.Asin(Chunk.Subs(Values)) * Coeff;
                case Functions.ACos:
                    return Math.Acos(Chunk.Subs(Values)) * Coeff;
                case Functions.ATan:
                    return Math.Atan(Chunk.Subs(Values)) * Coeff;

                case Functions.ASec:
                    return Coeff * Math.Acos(1 / Chunk.Subs(Values));
                case Functions.ACsc:
                    return Coeff * Math.Asin(1 / Chunk.Subs(Values));
                case Functions.ACot:
                    return Coeff * Math.Atan(1 / Chunk.Subs(Values));

                case Functions.ASinh:
                    var asinh = Chunk.Subs(Values);
                    return Math.Log(asinh + Math.Sqrt(Math.Pow(asinh, 2) + 1)) * Coeff;
                case Functions.ACosh:
                    var acosh = Chunk.Subs(Values);
                    return Math.Log(acosh + Math.Sqrt(Math.Pow(acosh, 2)- 1)) * Coeff;
                case Functions.ATanh:
                    var atanh = Chunk.Subs(Values);
                    return (0.5 * Math.Log((1+ atanh) / (1- atanh))) * Coeff;

                case Functions.ASech:
                    return Coeff * Math.Log((1 + Math.Sqrt(1 - Math.Pow(Chunk.Subs(Values), 2))) / Chunk.Subs(Values));
                case Functions.ACsch:
                    return Coeff * Math.Log(1 / Chunk.Subs(Values) + Math.Sqrt(1 + 1 / Math.Pow(Chunk.Subs(Values), 2)));
                case Functions.ACoth:
                    return Coeff * 0.5 * Math.Log((1 + Chunk.Subs(Values)) / (1 - Chunk.Subs(Values)));

                case Functions.Exp:
                    return Math.Exp(Chunk.Subs(Values)) * Coeff;
                case Functions.ln:
                    return Math.Log(Chunk.Subs(Values)) * Coeff;
                case Functions.log:
                    return Math.Log10(Chunk.Subs(Values)) * Coeff;

                case Functions.sqrt:
                    return Math.Sqrt(Chunk.Subs(Values)) * Coeff;
                case Functions.cbrt:
                    return Math.Pow(Chunk.Subs(Values),1.0/3.0) * Coeff;

                case Functions.Abs:
                    return Math.Abs(Chunk.Subs(Values)) * Coeff;
                case Functions.Min:
                    return Math.Min(Chunk.Subs(Values), SecondChunk?.Subs(Values) ?? throw new Exception("SecondChunk is Null")) * Coeff;
                case Functions.Max:
                    return Math.Max(Chunk.Subs(Values), SecondChunk?.Subs(Values) ?? throw new Exception("SecondChunk is Null")) * Coeff;

                default:
                    throw new Exception("Invalid Function");
            }
        }

        public IChunk Replace(Dictionary<string, IChunk> Values)
        {
            return new FuncChunk(Chunk.Replace(Values), Function, Coeff) { SecondChunk = SecondChunk?.Replace(Values)};
        }

        public override string ToString()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() : string.Empty;
            if (coeff == "-1") coeff = "-";
            string InnerInner = SecondChunk != null ? $"{Chunk},{SecondChunk}" : Chunk.ToString();
            string Inner = Chunk is SumChunk ? InnerInner : $"({InnerInner})";
            return $"{coeff}{Function}{Inner}";
        }

        public IChunk Expanded()
        {
            return new FuncChunk(Chunk.Expanded(), Function, Coeff) { SecondChunk = SecondChunk?.Expanded()};                
        }

        public string ToLatex()
        {
            string coeff = Coeff != 1 ? Coeff.ToString() : string.Empty;
            string InnerInner = SecondChunk != null ? $"{Chunk.ToLatex()},{SecondChunk.ToLatex()}" : Chunk.ToLatex();
            string Inner = Chunk is SumChunk ? InnerInner : Function == Functions.Abs ? $@"\left|{InnerInner}\right|" : $@"\left({InnerInner}\right)";
            return $"{coeff}{FunctionToLatex[Function]}{Inner}";
        }

        public IUnit GetUnit(Dictionary<string, IUnit> VariableUnitPairs)
        {
            var a = Chunk.GetUnit(VariableUnitPairs);
            var b = SecondChunk?.GetUnit(VariableUnitPairs);

            switch (Function)
            {
                case Functions.Sin:
                    return new BaseUnit(Units.None);
                case Functions.Cos:
                    return new BaseUnit(Units.None);
                case Functions.Tan:
                    return new BaseUnit(Units.None);
                case Functions.Sec:
                    return new BaseUnit(Units.None);
                case Functions.Csc:
                    return new BaseUnit(Units.None);
                case Functions.Cot:
                    return new BaseUnit(Units.None);
                case Functions.Sinh:
                    return new BaseUnit(Units.None);
                case Functions.Cosh:
                    return new BaseUnit(Units.None);
                case Functions.Tanh:
                    return new BaseUnit(Units.None);
                case Functions.Sech:
                    return new BaseUnit(Units.None);
                case Functions.Csch:
                    return new BaseUnit(Units.None);
                case Functions.Coth:
                    return new BaseUnit(Units.None);
                case Functions.ASin:
                    return new BaseUnit(Units.None);
                case Functions.ACos:
                    return new BaseUnit(Units.None);
                case Functions.ATan:
                    return new BaseUnit(Units.None);
                case Functions.ASec:
                    return new BaseUnit(Units.None);
                case Functions.ACsc:
                    return new BaseUnit(Units.None);
                case Functions.ACot:
                    return new BaseUnit(Units.None);
                case Functions.ASinh:
                    return new BaseUnit(Units.None);
                case Functions.ACosh:
                    return new BaseUnit(Units.None);
                case Functions.ATanh:
                    return new BaseUnit(Units.None);
                case Functions.ASech:
                    return new BaseUnit(Units.None);
                case Functions.ACsch:
                    return new BaseUnit(Units.None);
                case Functions.ACoth:
                    return new BaseUnit(Units.None);
                case Functions.Exp:
                    return new BaseUnit(Units.None);
                case Functions.ln:
                    return new BaseUnit(Units.None);
                case Functions.log:
                    return new BaseUnit(Units.None);
                case Functions.sqrt:
                    return a.Exponentiate(0.5);
                case Functions.cbrt:
                    return a.Exponentiate(1.0/3.0);
                case Functions.Abs:
                    if (a != b) throw new Exception($"Units musts match: {Function}({a},{b})");
                    return new BaseUnit(Units.None);
                case Functions.Min:
                    if (a != b) throw new Exception($"Units musts match: {Function}({a},{b})");
                    return new BaseUnit(Units.None);
                case Functions.Max:
                    if (a != b) throw new Exception($"Units musts match: {Function}({a},{b})");
                    return new BaseUnit(Units.None);
                default:
                    throw new Exception($"{Function} is not Implemented");
            }
        }

        public IChunk Antiderivative(string Var)
        {
            throw new NotImplementedException();
        }

        public List<string> GetVariables()
        {
            if (SecondChunk == null) return Chunk.GetVariables();
            return Chunk.GetVariables().Union(SecondChunk.GetVariables()).ToList();
        }
    }
}
