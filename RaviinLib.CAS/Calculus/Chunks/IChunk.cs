using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public interface IChunk
    {
        double Coeff { get; set; }

        IChunk Derivative(string Var);
        IChunk Antiderivative(string Var);
        IChunk Copy();
        IChunk Simplified();

        /// <summary>
        /// Modifies in place.
        /// </summary>
        /// <param name="factor"></param>
        void Multiply(double factor);

        /// <summary>
        /// Returns a copy multiplied by factor.
        /// </summary>
        /// <param name="factor"></param>
        IChunk MultiplyBy(double factor);

        void MultiplyExpanded(double factor);
        double Subs(Dictionary<string, double> Values);
        IChunk Replace(Dictionary<string, IChunk> Values);

        IChunk Expanded();

        IUnit GetUnit(Dictionary<string, IUnit> VariableUnitPairs);

        List<string> GetVariables();

        string ToString();
        string ToLatex();
        string ToCode();

        bool IsOne();
        bool IsZero();

        /// <summary>
        /// Is the chunk == BaseChunk(a,null,b)
        /// </summary>
        bool IsNumber();

        /// <summary>
        /// Is the chunk == BaseChunk(a, null, 1)
        /// </summary>
        bool IsSimpleNumber();

        /// <summary>
        /// Does the chunk contain no variables.
        /// </summary>
        bool IsConstant();
    }
}
