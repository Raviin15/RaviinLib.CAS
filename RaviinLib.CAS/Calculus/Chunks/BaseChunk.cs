
using System;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
public class BaseChunk : IChunk
    {
        public double Coeff { get; set; }
        public string Var { get; set; }
        public double Exp { get; set; }


        public BaseChunk(double Coeff, string Var, double Exp)
        {
            this.Coeff = Coeff;
            this.Var = Var;
            this.Exp = Exp;
        }

        public BaseChunk(double Coeff, string Var)
        {
            this.Coeff = Coeff;
            this.Var = Var;
            Exp = 1;
        }

        public BaseChunk(string Var)
        {
            Coeff = 1;
            this.Var = Var;
            Exp = 1;
        }
        public BaseChunk(double Coeff)
        {
            this.Coeff = Coeff;
            Var = null;
            Exp = 1;
        }


        public IChunk Derivative(string Var)
        {

            if (this.Var == null || Exp == 0 || this.Var != Var)
            {
                return new BaseChunk(0, null, 1);
            }

            if (Exp == 1)
            {
                return new BaseChunk(Coeff, null, Exp);
            }

            return new BaseChunk(Coeff * Exp, this.Var, Exp - 1);
        }

        public override string ToString()
        {
            string coeff = Coeff != 1 || Var == null ? Coeff.ToString() : string.Empty;
            if (coeff == "-1" && Var != null) coeff = "-";
            string exp = Exp != 1 ? $"^{Exp}" : string.Empty;
            return $"{coeff}{Var}{exp}";
        }

        public IChunk Copy()
        {
            return new BaseChunk(Coeff, Var, Exp);
        }
        public static BaseChunk Copy(BaseChunk ChunkToCopy)
        {
            return new BaseChunk(ChunkToCopy.Coeff, ChunkToCopy.Var, ChunkToCopy.Exp);
        }

        public IChunk Simplified()
        {
            if (Exp == 0)
            {
                if (Var != null) return new BaseChunk(Coeff, null, 1);
                else return new BaseChunk(1, null, 1);
            }

            if (Coeff == 1 && Var == null) return new BaseChunk(1, null, 1);

            if (Coeff == 0) return null;
            return Copy();
        }

        public void Multiply(double factor)
        {
            Coeff *= factor;
        }
        public void MultiplyExpanded(double factor)
        {
            Multiply(factor);
        }

        public double Subs(Dictionary<string, double> Values)
        {
            if (Var == null) return Math.Pow(Coeff, Exp);

            double Value = Values[Var];
            return Coeff * Math.Pow(Value, Exp);
        }

        public IChunk Replace(Dictionary<string, IChunk> Values)
        {
            if (Var == null || !Values.ContainsKey(Var)) return Copy();

            IChunk Value = Values[Var];
            return new ChainChunk(Coeff,Value, new BaseChunk(Exp, null, 1));
                
            //double Value = Values[(string)Var];
            //return new BaseChunk(this.Coeff * Math.Pow(Value,this.Exp), null, 1);
        }

        public IChunk Expanded()
        {

            if (Exp == 0) return new BaseChunk(1, null, 1);
            else if (Exp <= 1) return Copy();
            else
            {
                BaseChunk BaseCopy = Copy() as BaseChunk;
                BaseCopy.Exp = 1;
                BaseCopy.Coeff = 1;
                IChunk ReturnChunk = BaseCopy;
                for (int i = 1; i < Exp; i++)
                {
                    ReturnChunk = new ProductChunk(ReturnChunk, BaseCopy.Copy());
                }
                ((ProductChunk)ReturnChunk).Chunk2.MultiplyExpanded(Coeff);
                return ReturnChunk.Expanded();
            }
        }

        public string ToLatex()
        {
            string coeff = Coeff != 1 || Var == null ? Coeff.ToString() : string.Empty;
            if (coeff == "-1" && Var != null) coeff = "-";
            string exp = Exp != 1 ? $"^{{{Exp}}}" : string.Empty;
            return $"{coeff}{Var}{exp}";
        }

        public IUnit GetUnit(Dictionary<string, IUnit> VariableUnitPairs)
        {
            if (Var == null || !VariableUnitPairs.TryGetValue(Var, out IUnit Unit)) return new BaseUnit(Units.None);
            return Unit.Exponentiate(Exp);
        }

        public IChunk Antiderivative(string Var)
        {
            if (Exp == -1) return new FuncChunk(new FuncChunk(new BaseChunk(Var),Functions.Abs), Functions.ln, Coeff);
            if (this.Var == null) return new BaseChunk(Math.Pow(Coeff, Exp), Var, 1);

            return new BaseChunk(Coeff / (Exp + 1), this.Var, Exp + 1);
            
        }

        public List<string> GetVariables()
        {
            if (Var == null) return new List<string>();
            return new List<string>() { Var };
        }

        public static BaseChunk operator *(BaseChunk Chunk, double Coeff)
        {
            BaseChunk NewChunk = (BaseChunk)Chunk.Copy();
            NewChunk.Coeff *= Coeff;
            return NewChunk;
        }
        public static BaseChunk operator *(double Coeff, BaseChunk Chunk)
        {
            BaseChunk NewChunk = (BaseChunk)Chunk.Copy();
            NewChunk.Coeff *= Coeff;
            return NewChunk;
        }


        public static implicit operator BaseChunk(double a)
        {
            return new BaseChunk(a, null, 1);
        }

        public static implicit operator BaseChunk(int a)
        {
            return new BaseChunk(a, null, 1);
        }

    }
}
