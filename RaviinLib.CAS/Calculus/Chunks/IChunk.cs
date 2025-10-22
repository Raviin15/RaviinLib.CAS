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
        void Multiply(double factor);
        void MultiplyExpanded(double factor);
        double Subs(Dictionary<string, double> Values);
        IChunk Replace(Dictionary<string, IChunk> Values);

        IChunk Expanded();

        IUnit GetUnit(Dictionary<string, IUnit> VariableUnitPairs);

        List<string> GetVariables();

        string ToString();
        string ToLatex();
    }
}
