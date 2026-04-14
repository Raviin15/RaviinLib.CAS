using System;
using System.Linq;
using System.Collections.Generic;
using RaviinLib.CAS;

namespace RaviinLib.CAS.Unused
{
    #region Variable Implementations

    public enum Units
    {
        None,
        Meter,
        Second,
        Kilogram,
        Ampere,
        Kelvin,
        Mole,
        Candela,
        Foot,
        Inch,
        Yard,
        Mile,
        Pound,
        Ounce,
        Gram,
        Liter,
        Milliliter,
        Gallon,
        Newton,
        Joule,
        Watt,
        Pascal,
        Hertz,
        Coulomb,
        Volt,
        Ohm,
        Farad,
        Tesla,
        Weber,
        Henry,
        Lumen,
        Lux,
        Becquerel,
        Gray,
        Sievert,
        Radian,
        Steradian
    }

    #region ScientificFunction
    public class Variable
    {


        public string Var { get; private set; }
        public string Unit { get; private set; } = null;
        public int? Power { get; private set; } = null;
        public Units UnitEnum { get; private set; } = Units.None;

        #region Constructors
        public Variable(string Var)
        {
            if (Chunker.DissalowedVarCharsExcludingNumbers.Any(c => Var.Contains(c))) throw new Exception("Variable contains dissallowed characters.");
            this.Var = Var;
        }
        public Variable(string Var, string Unit)
        {
            if (Chunker.DissalowedVarCharsExcludingNumbers.Any(c => Var.Contains(c))) throw new Exception("Variable contains dissallowed characters.");
            this.Var = Var;
            this.Unit = Unit;
        }
        public Variable(string Var, Units Unit, int Power)
        {
            if (Chunker.DissalowedVarCharsExcludingNumbers.Any(c => Var.Contains(c))) throw new Exception("Variable contains dissallowed characters.");
            this.Var = Var;
            if (Unit != Units.None) this.Unit = Unit.ToString();
            this.UnitEnum = Unit;
            this.Power = Power;
        }
        #endregion

        public void ConvertToUnit(Units NewUnit)
        {
            if (NewUnit == Units.None) { this.Unit = null; this.UnitEnum = NewUnit; return; }

            switch (NewUnit)
            {
                case Units.None:
                    break;
                case Units.Meter:
                    break;
                case Units.Second:
                    break;
                case Units.Kilogram:
                    break;
                case Units.Ampere:
                    break;
                case Units.Kelvin:
                    break;
                case Units.Mole:
                    break;
                case Units.Candela:
                    break;
                case Units.Foot:
                    break;
                case Units.Inch:
                    break;
                case Units.Yard:
                    break;
                case Units.Mile:
                    break;
                case Units.Pound:
                    break;
                case Units.Ounce:
                    break;
                case Units.Gram:
                    break;
                case Units.Liter:
                    break;
                case Units.Milliliter:
                    break;
                case Units.Gallon:
                    break;
                case Units.Newton:
                    break;
                case Units.Joule:
                    break;
                case Units.Watt:
                    break;
                case Units.Pascal:
                    break;
                case Units.Hertz:
                    break;
                case Units.Coulomb:
                    break;
                case Units.Volt:
                    break;
                case Units.Ohm:
                    break;
                case Units.Farad:
                    break;
                case Units.Tesla:
                    break;
                case Units.Weber:
                    break;
                case Units.Henry:
                    break;
                case Units.Lumen:
                    break;
                case Units.Lux:
                    break;
                case Units.Becquerel:
                    break;
                case Units.Gray:
                    break;
                case Units.Sievert:
                    break;
                case Units.Radian:
                    break;
                case Units.Steradian:
                    break;
                default:
                    break;
            }
            this.Unit = NewUnit.ToString();
            this.UnitEnum = NewUnit;
        }

        #region Overides

        #region double,Variable
        public static ScientificFunction operator *(double a, Variable b)
        {
            return new ScientificFunction(new BaseChunk(a, b), b.Unit, new List<Variable>() { b });
        }
        public static ScientificFunction operator /(double a, Variable b)
        {
            return new ScientificFunction(new BaseChunk(1 / a, b), b.Unit, new List<Variable>() { b });
        }
        public static ScientificFunction operator +(double a, Variable b)
        {
            return new ScientificFunction(new SumChunk(new List<IChunk>() { new BaseChunk(a), new BaseChunk(b) }), b.Unit, new List<Variable>() { b });
        }
        public static ScientificFunction operator -(double a, Variable b)
        {
            return new ScientificFunction(new SumChunk(new List<IChunk>() { new BaseChunk(a), new BaseChunk(-1, b) }), b.Unit, new List<Variable>() { b });
        }
        #endregion

        #region Variable,double
        public static ScientificFunction operator *(Variable a, double b)
        {
            return new ScientificFunction(new BaseChunk(b, a), a.Unit, new List<Variable>() { a });
        }
        public static ScientificFunction operator /(Variable a, double b)
        {
            return new ScientificFunction(new BaseChunk(1 / b, a), a.Unit, new List<Variable>() { a });
        }
        public static ScientificFunction operator +(Variable a, double b)
        {
            return new ScientificFunction(new SumChunk(new List<IChunk>() { new BaseChunk(b), new BaseChunk(a) }), a.Unit, new List<Variable>(){a });
        }
        public static ScientificFunction operator -(Variable a, double b)
        {
            return new ScientificFunction(new SumChunk(new List<IChunk>() { new BaseChunk(b), new BaseChunk(-1, a) }), a.Unit, new List<Variable>() { a });
        }
        public static ScientificFunction operator ^(Variable a, double b)
        {
            return new ScientificFunction(new ChainChunk(1, new BaseChunk(a.Var), new BaseChunk(b)), (a.Unit == null) ? a.Unit : $"(({a.Unit})^{b})", new List<Variable>() { a });
        }
        #endregion

        #region Variable,Variable
        public static ScientificFunction operator *(Variable a, Variable b)
        {
            return new ScientificFunction(new ProductChunk(new BaseChunk(a), new BaseChunk(b)), CombineUnits(a, b, '*'), new List<Variable>() { a, b });
        }
        public static ScientificFunction operator /(Variable a, Variable b)
        {
            return new ScientificFunction(new ProductChunk(new BaseChunk(a), new ChainChunk(1, new BaseChunk(b), new BaseChunk(-1))), CombineUnits(a, b, '/'), new List<Variable>() { a, b });
        }
        public static ScientificFunction operator +(Variable a, Variable b)
        {
            if (a.Unit != b.Unit) throw new Exception("Unit Mismatch");
            return new ScientificFunction(new SumChunk(new List<IChunk>() { new BaseChunk(a), new BaseChunk(b) }), a.Unit, new List<Variable>() { a, b });
        }
        public static ScientificFunction operator -(Variable a, Variable b)
        {
            if (a.Unit != b.Unit) throw new Exception("Unit Mismatch");
            return new ScientificFunction(new SumChunk(new List<IChunk>() { new BaseChunk(a), new BaseChunk(-1, b)}), a.Unit, new List<Variable>() { a, b });
        }
        #endregion

        #region Implicit

        public static implicit operator Function(Variable a)
        {
            return new Function(new BaseChunk(a.Var));
        }

        public static implicit operator string(Variable a)
        {
            return a.Var;
        }

        public static implicit operator Variable(string a)
        {
            return new Variable(a);
        }

        #endregion

        #endregion

        #region Static

        public static bool SameUnit(string a, string b)
        {
            if (a == null || b == null) return true;
            return a == b;
        }
        public static bool SameUnit(ScientificFunction a, Variable b)
        {
            return SameUnit(a.RawUnit, b.Unit);
        }
        public static bool SameUnit(Variable a, ScientificFunction b)
        {
            return SameUnit(b, a);
        }


        public static string CombineUnits(Variable a, Variable b, char Operator)
        {
            if (a.Unit == null && b.Unit == null) return null;

            if (Operator == '*' || Operator == '/') return (a.Unit == null) ? b.Unit : (b.Unit == null) ? a.Unit : a.Unit + Operator + b.Unit;

            return a.Unit ?? b.Unit;
        }
        public static string CombineUnits(string a, string b, char Operator)
        {
            if (a == null && b == null) return null;

            if (Operator == '*') return (a == null) ? b : (b == null) ? a : a + Operator + b;
            if (Operator == '/') return (a == null) ? b : (b == null) ? a : $"(({a})" + Operator + $"({b}))";

            return a ?? b;
        }

        #endregion

    }

    public class ScientificFunction
    {

        public Function Function { get; set; }
        public List<Variable> Variables { get; set; }

        public string Unit => new Function(RawUnit).GetSimplified().ToString();

        private string _RawUnit = null;
        public string RawUnit
        {
            get => _RawUnit;
            set
            {
                if (value == null) _RawUnit = value;
                else _RawUnit = new Function(value).GetSimplified().ToString();
            }
        }

        #region Replace Shorthand
        ///// <summary>
        ///// Returns a <see cref="Function"/> representing this.Replace(Variable, Val).
        ///// </summary>
        ///// <param name="Variable">The variable to be replaced.</param>
        ///// <param name="Val">The function or value to replace the specified variable with.</param>
        ///// <returns></returns>
        //public Function this[Variable Variable, Function Val] { get => Function.Replace(Variable, Val); }
        #endregion

        #region Constructors
        //public ScientificFunction(Function Fx, List<Variable> Variables)
        //{
        //    this.Function = Fx;
        //    this.Variables = Variables;
        //}
        public ScientificFunction(IChunk Fx, string Unit, List<Variable> Variables)
        {
            this.Function = new Function(Fx);
            this.Variables = Variables;
            this.RawUnit = Unit;
        }
        #endregion

        #region Overides

        public override string ToString()
        {
            return Function.ToString();
        }

        #region Variable,ScientificFunction
        public static ScientificFunction operator *(Variable a, ScientificFunction b)
        {
            return new ScientificFunction((a * b.Function).IFunction, Variable.CombineUnits(a.Unit, b.RawUnit, '*'), b.Variables.Append(a).Distinct().ToList());
        }
        public static ScientificFunction operator /(Variable a, ScientificFunction b)
        {
            return new ScientificFunction((a / b.Function).IFunction, Variable.CombineUnits(a.Unit, b.RawUnit, '/'), b.Variables.Append(a).Distinct().ToList());
        }
        public static ScientificFunction operator +(Variable a, ScientificFunction b)
        {
            if (!Variable.SameUnit(a.Unit, b.RawUnit)) throw new Exception("Unit Mismatch");
            return new ScientificFunction((a + b.Function).IFunction, a.Unit, b.Variables.Append(a).Distinct().ToList());
        }
        public static ScientificFunction operator -(Variable a, ScientificFunction b)
        {
            if (!Variable.SameUnit(a.Unit, b.RawUnit)) throw new Exception("Unit Mismatch");
            return new ScientificFunction((a - b.Function).IFunction, a.Unit, b.Variables.Append(a).Distinct().ToList());
        }
        #endregion

        #region ScientificFunction,Variable
        public static ScientificFunction operator *(ScientificFunction a, Variable b)
        {
            return new ScientificFunction((a.Function * b).IFunction, Variable.CombineUnits(a.RawUnit, b.Unit, '*'), a.Variables.Append(b).Distinct().ToList());
        }
        public static ScientificFunction operator /(ScientificFunction a, Variable b)
        {
            return new ScientificFunction((a.Function / b).IFunction, Variable.CombineUnits(a.RawUnit, b.Unit, '/'), a.Variables.Append(b).Distinct().ToList());
        }
        public static ScientificFunction operator +(ScientificFunction a, Variable b)
        {
            if (!Variable.SameUnit(a.RawUnit, b.Unit)) throw new Exception("Unit Mismatch");
            return new ScientificFunction((a.Function + b).IFunction, a.RawUnit, a.Variables.Append(b).Distinct().ToList());
        }
        public static ScientificFunction operator -(ScientificFunction a, Variable b)
        {
            if (!Variable.SameUnit(a.RawUnit, b.Unit)) throw new Exception("Unit Mismatch");
            return new ScientificFunction((a.Function - b).IFunction, a.RawUnit, a.Variables.Append(b).Distinct().ToList());
        }
        #endregion

        #region ScientificFunction,ScientificFunction
        public static ScientificFunction operator *(ScientificFunction a, ScientificFunction b)
        {
            return new ScientificFunction((a.Function * b.Function).IFunction, Variable.CombineUnits(a.RawUnit, b.RawUnit, '*'), new List<Variable>(a.Variables).Concat(b.Variables).Distinct().ToList());
        }
        public static ScientificFunction operator /(ScientificFunction a, ScientificFunction b)
        {
            return new ScientificFunction((a.Function / b.Function).IFunction, Variable.CombineUnits(a.RawUnit, b.RawUnit, '/'), new List<Variable>(a.Variables).Concat(b.Variables).Distinct().ToList());
        }
        public static ScientificFunction operator +(ScientificFunction a, ScientificFunction b)
        {
            if (!Variable.SameUnit(a.RawUnit, b.RawUnit)) throw new Exception("Unit Mismatch");
            return new ScientificFunction((a.Function + b.Function).IFunction, a.RawUnit, new List<Variable>(a.Variables).Concat(b.Variables).Distinct().ToList());
        }
        public static ScientificFunction operator -(ScientificFunction a, ScientificFunction b)
        {
            if (!Variable.SameUnit(a.RawUnit, b.RawUnit)) throw new Exception("Unit Mismatch");
            return new ScientificFunction((a.Function - b.Function).IFunction, a.RawUnit, new List<Variable>(a.Variables).Concat(b.Variables).Distinct().ToList());
        }
        #endregion

        #region ScientificFunction,double
        public static ScientificFunction operator *(ScientificFunction a, double b)
        {
            return new ScientificFunction((a.Function * b).IFunction, a.RawUnit, new List<Variable>(a.Variables));
        }
        public static ScientificFunction operator /(ScientificFunction a, double b)
        {
            return new ScientificFunction((a.Function / b).IFunction, a.RawUnit, new List<Variable>(a.Variables));
        }
        public static ScientificFunction operator +(ScientificFunction a, double b)
        {
            return new ScientificFunction((a.Function + b).IFunction, a.RawUnit, new List<Variable>(a.Variables));
        }
        public static ScientificFunction operator -(ScientificFunction a, double b)
        {
            return new ScientificFunction((a.Function - b).IFunction, a.RawUnit, new List<Variable>(a.Variables));
        }
        public static ScientificFunction operator ^(ScientificFunction a, double b)
        {
            return new ScientificFunction((a.Function ^ b).IFunction, $"(({a.Unit})^{b})", new List<Variable>(a.Variables));
        }

        #endregion

        #endregion
    }
    #endregion

    public interface IUnit
    {
        IUnit Copy();

        IUnit Simplify();

        IUnit Exponentiate(double Pow);

        string ToString();

        (double, IUnit) TryConvertTo(Units NewUnit);
    }

    public class Unit : IUnit
    {
        public List<IUnit> Numerator { get; set; }
        public List<IUnit> Denominator { get; set; }

        public Unit(List<IUnit> Numerator, List<IUnit> Denominator)
        {
            this.Numerator = Numerator;
            this.Denominator = Denominator;
        }

        public IUnit Copy()
        {
            List<IUnit> Num = new List<IUnit>();
            foreach (var item in Numerator)
            {
                Num.Add(item.Copy());
            }
            List<IUnit> Den = new List<IUnit>();
            foreach (var item in Denominator)
            {
                Den.Add(item.Copy());
            }

            return new Unit(Num, Den);
        }

        public IUnit Simplify()
        {
            List<IUnit> Num = new List<IUnit>();
            foreach (var item in Numerator)
            {
                var simp = item.Simplify();
                if (!(simp is BaseUnit b && b.Unit == Units.None)) Num.Add(simp);
            }
            List<IUnit> Den = new List<IUnit>();
            foreach (var item in Denominator)
            {
                var simp = item.Simplify();
                if (!(simp is BaseUnit b && b.Unit == Units.None)) Den.Add(simp);
            }

            var numBaseUnits = Num.OfType<BaseUnit>().GroupBy(c => c, new IUnitComparerIgnorePower());

            List<IUnit> numUnits = new List<IUnit>();
            foreach (var group in numBaseUnits)
            {
                var Pow = group.Select(c => c.Power).Aggregate((a, b) => a + b);
                var Key = (BaseUnit)group.Key;
                numUnits.Add(new BaseUnit(Key.Unit, Pow));
            }

            var denBaseUnits = Den.OfType<BaseUnit>().GroupBy(c => c, new IUnitComparerIgnorePower());

            List<IUnit> denUnits = new List<IUnit>();
            foreach (var group in denBaseUnits)
            {
                var Pow = group.Select(c => c.Power).Aggregate((a, b) => a + b);
                var Key = (BaseUnit)group.Key;
                denUnits.Add(new BaseUnit(Key.Unit, Pow));
            }

            var denCopyies = denUnits.Select(c => c.Copy().Exponentiate(-1));

            var Combined = denCopyies.Concat(numUnits).OfType<BaseUnit>().GroupBy(c => c, new IUnitComparerIgnorePower());


            List<IUnit> combUnits = new List<IUnit>();
            foreach (var group in Combined)
            {
                var Pow = group.Select(c => c.Power).Aggregate((a, b) => a + b);
                if (Pow == 0) continue;
                var Key = (BaseUnit)group.Key;
                combUnits.Add(new BaseUnit(Key.Unit, Pow));
            }

            List<IUnit> numRet = numUnits.OfType<Unit>().Cast<IUnit>().ToList();
            List<IUnit> denRet = denUnits.OfType<Unit>().Cast<IUnit>().ToList();
            foreach (var item in combUnits)
            {
                if (((BaseUnit)item).Power > 0) numRet.Add(item);
                else denRet.Add(item.Exponentiate(-1));
            }

            if (numRet.Count == 0 && denRet.Count == 0) return new BaseUnit(Units.None);
            else if (numRet.Count == 1 && denRet.Count == 0) return numRet[0];
            else if (numRet.Count == 0 && denRet.Count == 1) return denRet[0].Exponentiate(-1);

            return new Unit(numRet, denRet);
        }

        public IUnit Exponentiate(double Pow)
        {
            List<IUnit> Num = new List<IUnit>();
            List<IUnit> Den = new List<IUnit>();
            foreach (var item in Numerator)
            {
                var exp = item.Exponentiate(Pow);
                if (exp is BaseUnit b && b.Power < 0)
                {
                    b.Power *= -1;
                    Den.Add(b);
                }
                else Num.Add(exp);
            }
            foreach (var item in Denominator)
            {
                var exp = item.Exponentiate(-1).Exponentiate(Pow);
                if (exp is BaseUnit b && b.Power < 0)
                {
                    b.Power *= -1;
                    Den.Add(b);
                }
                else Num.Add(exp);
            }

            return new Unit(Num, Den);
        }

        public override string ToString()
        {
            IEnumerable<string> NumInnerUnits = Numerator.Select(c => c.ToString()).Where(c => c != "Unitless");
            IEnumerable<string> DenInnerUnits = Denominator.Select(c => c.ToString()).Where(c => c != "Unitless");

            if (NumInnerUnits.Count() == 0 && DenInnerUnits.Count() == 0) return "Unitless";

            var numRet = (NumInnerUnits.Count() == 0) ? "1" : string.Join(" * ", NumInnerUnits);

            var denRet = (DenInnerUnits.Count() == 0) ? string.Empty : $" / {string.Join(" * ", DenInnerUnits)}";

            return $"{numRet}{denRet}";
        }

        public (double, IUnit) TryConvertTo(Units NewUnit)
        {
            double numConversion = 1;
            List<IUnit> Num = new List<IUnit>();
            foreach (var item in Numerator)
            {
                try
                {
                    (double d, IUnit u) = item.TryConvertTo(NewUnit);
                    Num.Add(u);
                    numConversion *= d;
                }
                catch (Exception)
                {
                    Num.Add(item);
                }
            }
            double denConversion = 1;
            List<IUnit> Den = new List<IUnit>();
            foreach (var item in Numerator)
            {
                try
                {
                    (double d, IUnit u) = item.TryConvertTo(NewUnit);
                    Den.Add(u);
                    denConversion *= d;
                }
                catch (Exception)
                {
                    Den.Add(item);
                }
            }

            return (numConversion / denConversion, new Unit(Num, Den).Simplify());
        }
    }

    public class BaseUnit : IUnit
    {
        public static Dictionary<(Units, Units), double> ConversionTable = new Dictionary<(Units, Units), double>()
        {

        };

        public double Power { get; set; }
        public Units Unit { get; set; }

        public BaseUnit(Units Unit, double Power = 1)
        {
            this.Unit = Unit;
            this.Power = Power;
        }

        public IUnit Copy()
        {
            return new BaseUnit(Unit, Power);
        }

        public IUnit Simplify()
        {
            var Copy = (BaseUnit)this.Copy();
            if (Unit == Units.None) Copy.Power = 1;
            return Copy;
        }

        public override string ToString()
        {
            if (Unit == Units.None) return "Unitless";

            if (Power < 0) return $"1 / {Unit}^{Power * -1}";

            string pow = (Power == 1) ? string.Empty : $"^{Power}";

            return Unit.ToString() + pow;
        }

        public IUnit Exponentiate(double Pow)
        {
            var Copy = (BaseUnit)this.Copy();
            Copy.Power *= Pow;
            return Copy;
        }

        public (double, IUnit) TryConvertTo(Units NewUnit)
        {
            //if (ConversionTable.TryGetValue((this, NewUnit), out var Convertion)) return Convertion;
            if (ConversionTable.TryGetValue((this.Unit, NewUnit), out var Convertion)) return (Math.Pow(Convertion, Power), new BaseUnit(NewUnit));

            throw new Exception($"No conversion available: {this}=>{NewUnit}");
        }
    }

    public class IUnitComparer : IEqualityComparer<IUnit>
    {
        public bool Equals(IUnit a, IUnit b)
        {
            if (a == null || b == null) return false;

            if (a is BaseUnit ab && b is BaseUnit bb)
            {
                return (ab.Unit == bb.Unit) && (ab.Power == bb.Power);
            }
            else if (a is Unit au && b is Unit bu)
            {
                if (au.Numerator.Count != bu.Numerator.Count || au.Denominator.Count != bu.Denominator.Count) return false;

                // Order-insensitive comparison of chunk sets
                var aNumChunksGrouped = au.Numerator.GroupBy(x => x, this);
                var bNumChunksGrouped = bu.Numerator.GroupBy(x => x, this);

                foreach (var groupA in aNumChunksGrouped)
                {
                    int countA = groupA.Count();
                    int countB = bNumChunksGrouped.FirstOrDefault(g => Equals(g.Key, groupA.Key))?.Count() ?? -1;
                    if (countA != countB)
                        return false;
                }

                // Order-insensitive comparison of chunk sets
                var aDenChunksGrouped = au.Denominator.GroupBy(x => x, this);
                var bDenChunksGrouped = bu.Denominator.GroupBy(x => x, this);

                foreach (var groupA in aDenChunksGrouped)
                {
                    int countA = groupA.Count();
                    int countB = bDenChunksGrouped.FirstOrDefault(g => Equals(g.Key, groupA.Key))?.Count() ?? -1;
                    if (countA != countB)
                        return false;
                }

                return true;
            }

            return false;
        }

        public int GetHashCode( IUnit obj)
        {
            switch (obj)
            {
                case Unit u:
                    if (u.Numerator.Count == 0 && u.Denominator.Count == 0) return 0;
                    else if (u.Numerator.Count == 0) return u.Denominator.Select(GetHashCode).OrderBy(h => h).Aggregate((acc, h) => HashCode.Combine(acc, h));
                    else if (u.Denominator.Count == 0) return u.Numerator.Select(GetHashCode).OrderBy(h => h).Aggregate((acc, h) => HashCode.Combine(acc, h));

                    var numHashes = u.Numerator.Select(GetHashCode).OrderBy(h => h);
                    var numAgg = numHashes.Aggregate((acc, h) => HashCode.Combine(acc, h));
                    var denHashes = u.Denominator.Select(GetHashCode).OrderBy(h => h);
                    var denAgg = denHashes.Aggregate((acc, h) => HashCode.Combine(acc, h));
                    return HashCode.Combine(numAgg, denAgg);

                case BaseUnit b:
                    if (b.Unit == Units.None) return b.Unit.GetHashCode();
                    return HashCode.Combine(b.Power.GetHashCode(), b.Unit.GetHashCode());
                default:
                    return obj.GetHashCode();
            }
        }
    }

    public class IUnitComparerIgnorePower : IEqualityComparer<IUnit>
    {
        public bool Equals(IUnit a, IUnit b)
        {
            if (a == null || b == null) return false;

            if (a is BaseUnit ab && b is BaseUnit bb)
            {
                return (ab.Unit == bb.Unit);
            }
            else if (a is Unit au && b is Unit bu)
            {
                if (au.Numerator.Count != bu.Numerator.Count || au.Denominator.Count != bu.Denominator.Count) return false;

                // Order-insensitive comparison of chunk sets
                var aNumChunksGrouped = au.Numerator.GroupBy(x => x, this);
                var bNumChunksGrouped = bu.Numerator.GroupBy(x => x, this);

                foreach (var groupA in aNumChunksGrouped)
                {
                    int countA = groupA.Count();
                    int countB = bNumChunksGrouped.FirstOrDefault(g => Equals(g.Key, groupA.Key))?.Count() ?? -1;
                    if (countA != countB)
                        return false;
                }

                // Order-insensitive comparison of chunk sets
                var aDenChunksGrouped = au.Denominator.GroupBy(x => x, this);
                var bDenChunksGrouped = bu.Denominator.GroupBy(x => x, this);

                foreach (var groupA in aDenChunksGrouped)
                {
                    int countA = groupA.Count();
                    int countB = bDenChunksGrouped.FirstOrDefault(g => Equals(g.Key, groupA.Key))?.Count() ?? -1;
                    if (countA != countB)
                        return false;
                }

                return true;
            }

            return false;
        }

        public int GetHashCode( IUnit obj)
        {
            switch (obj)
            {
                case Unit u:
                    if (u.Numerator.Count == 0 && u.Denominator.Count == 0) return 0;
                    else if (u.Numerator.Count == 0) return u.Denominator.Select(GetHashCode).OrderBy(h => h).Aggregate((acc, h) => HashCode.Combine(acc, h));
                    else if (u.Denominator.Count == 0) return u.Numerator.Select(GetHashCode).OrderBy(h => h).Aggregate((acc, h) => HashCode.Combine(acc, h));

                    var numHashes = u.Numerator.Select(GetHashCode).OrderBy(h => h);
                    var numAgg = numHashes.Aggregate((acc, h) => HashCode.Combine(acc, h));
                    var denHashes = u.Denominator.Select(GetHashCode).OrderBy(h => h);
                    var denAgg = denHashes.Aggregate((acc, h) => HashCode.Combine(acc, h));
                    return HashCode.Combine(numAgg, denAgg);

                case BaseUnit b:
                    return b.Unit.GetHashCode();
                default:
                    return obj.GetHashCode();
            }
        }
    }

    #endregion
}

namespace RaviinLib.CAS
{
    public class Unit
    {

        #region Unit Conversion

        #region Lengths
        public static HashSet<string> Lengths = new HashSet<string>()
        {
            "mm",
            "cm",
            "m",
            "km",
            "ft",
            "in",
            "yd",
            "mi"
        };

        public static Dictionary<string, double> LengthConversionsToMeter = new Dictionary<string, double>()
        {
            { "mm", 0.001 },
            { "cm", 0.01 },
            { "m", 1 },
            { "km", 1000 },
            { "ft", 0.3048 },
            { "in", 0.0254 },
            { "yd", 0.9144 },
            { "mi", 1609.34 }
        };

        public static double ConvertLength(double Value, string FromUnit, string ToUnit)
        {
            if (!Lengths.Contains(FromUnit) || !Lengths.Contains(ToUnit)) throw new Exception("Invalid length unit.");
            double valueInMeters = Value * LengthConversionsToMeter[FromUnit];
            return valueInMeters / LengthConversionsToMeter[ToUnit];
        }
        #endregion

        #region Masses
        public static HashSet<string> Masses = new HashSet<string>()
        {
            "mg",
            "g",
            "kg",
            "oz",
            "lb"
        };

        public static Dictionary<string, double> MassConversionsToGram = new Dictionary<string, double>()
        {
            { "mg", 0.001 },
            { "g", 1 },
            { "kg", 1000 },
            { "oz", 28.3495 },
            { "lb", 453.592 }
        };

        public static double ConvertMass(double Value, string FromUnit, string ToUnit)
        {
            if (!Masses.Contains(FromUnit) || !Masses.Contains(ToUnit)) throw new Exception("Invalid mass unit.");
            double valueInGrams = Value * MassConversionsToGram[FromUnit];
            return valueInGrams / MassConversionsToGram[ToUnit];
        }
        #endregion

        #region Times
        public static HashSet<string> Times = new HashSet<string>()
        {
            "ms",
            "s",
            "min",
            "hr"
        };

        public static Dictionary<string, double> TimeConversionsToSecond = new Dictionary<string, double>()
        {
            { "ms", 0.001 },
            { "s", 1 },
            { "min", 60 },
            { "hr", 3600 }
        };

        public static double ConvertTime(double Value, string FromUnit, string ToUnit)
        {
            if (!Times.Contains(FromUnit) || !Times.Contains(ToUnit)) throw new Exception("Invalid time unit.");
            double valueInSeconds = Value * TimeConversionsToSecond[FromUnit];
            return valueInSeconds / TimeConversionsToSecond[ToUnit];
        }
        #endregion

        #region Temperatures
        public static HashSet<string> Temperatures = new HashSet<string>()
        {
            "C",
            "F",
            "K"
        };

        public static Dictionary<string, Func<double, double>> TemperatureToCelsius = new Dictionary<string, Func<double, double>>()
        {
            { "C", x => x },
            { "F", x => (x - 32) * 5 / 9 },
            { "K", x => x - 273.15 }
        };

        public static double ConvertTemperature(double Value, string FromUnit, string ToUnit)
        {
            if (!Temperatures.Contains(FromUnit) || !Temperatures.Contains(ToUnit)) throw new Exception("Invalid temperature unit.");
            double valueInCelsius = TemperatureToCelsius[FromUnit](Value);
            if (ToUnit == "C") return valueInCelsius;
            else if (ToUnit == "F") return valueInCelsius * 9 / 5 + 32;
            else if (ToUnit == "K") return valueInCelsius + 273.15;
            else throw new Exception("Invalid temperature unit.");
        }
        #endregion

        #region Amounts
        public static HashSet<string> Amounts = new HashSet<string>()
        {
            "mol"
        };

        public static Dictionary<string, double> AmountConversionsToMole = new Dictionary<string, double>()
        {
            { "mol", 1 }
        };

        public static double ConvertAmount(double Value, string FromUnit, string ToUnit)
        {
            if (!Amounts.Contains(FromUnit) || !Amounts.Contains(ToUnit)) throw new Exception("Invalid amount unit.");
            double valueInMoles = Value * AmountConversionsToMole[FromUnit];
            return valueInMoles / AmountConversionsToMole[ToUnit];
        }
        #endregion

        #region Unit Definitions
        public static Dictionary<string, HashSet<string>> UnitCategories = new Dictionary<string, HashSet<string>>()
        {
            { "Length", Lengths },
            { "Mass", Masses },
            { "Time", Times },
            { "Temperature", Temperatures },
            { "Amount", Amounts }
        };

        public static Dictionary<string, string> UnitPriorities = new Dictionary<string, string>()
        {
            { "Length", "m" },
            { "Mass", "g" },
            { "Time", "s" },
            { "Temperature", "C" },
            { "Amount", "mol" }
        };

        public static Dictionary<string, Func<double, string, string, double>> CategoryConversions = new Dictionary<string, Func<double, string, string, double>>()
        {
            { "Length", ConvertLength },
            { "Mass", ConvertMass },
            { "Time", ConvertTime },
            { "Temperature", ConvertTemperature },
            { "Amount", ConvertAmount }
        };
        #endregion

        #region Helper Methods
        public static string GetUnitCategory(string Unit)
        {
            foreach (var category in UnitCategories)
            {
                if (category.Value.Contains(Unit)) return category.Key;
            }
            return null;
        }

        public static string GetPriorityUnit(string Category)
        {
            if (UnitPriorities.TryGetValue(Category, out string Value)) return Value;
            throw new Exception($"Invalid unit category: {Category}");
        }

        public static double ConvertUnit(double Value, string FromUnit, string ToUnit)
        {
            string fromCategory = GetUnitCategory(FromUnit);
            string toCategory = GetUnitCategory(ToUnit);
            if (fromCategory == null) throw new Exception($"Invalid unit: {FromUnit}");
            if (toCategory == null) throw new Exception($"Invalid unit: {ToUnit}");

            if (fromCategory != toCategory) throw new Exception($"Incompatible units: {FromUnit}|{ToUnit}");

            return CategoryConversions[fromCategory](Value, FromUnit, ToUnit);
        }
        #endregion

        #region Adding Units and Categories
        public static void AddUnit(string Category, string Unit, double ConversionToBase)
        {
            if (!UnitCategories.ContainsKey(Category)) throw new Exception("Invalid unit category.");
            switch (Category)
            {
                case "Length":
                    Lengths.Add(Unit);
                    LengthConversionsToMeter[Unit] = ConversionToBase;
                    break;
                case "Mass":
                    Masses.Add(Unit);
                    MassConversionsToGram[Unit] = ConversionToBase;
                    break;
                case "Time":
                    Times.Add(Unit);
                    TimeConversionsToSecond[Unit] = ConversionToBase;
                    break;
                case "Temperature":
                    Temperatures.Add(Unit);
                    // Temperature conversion requires a function, not a simple multiplier
                    break;
                case "Amount":
                    Amounts.Add(Unit);
                    AmountConversionsToMole[Unit] = ConversionToBase;
                    break;
                default:
                    throw new Exception("Invalid unit category.");
            }
        }

        public static void AddUnit(List<(string Category, string Unit, double ConversionToBase)> Units)
        {
            foreach (var (Category, Unit, ConversionToBase) in Units)
            {
                AddUnit(Category, Unit, ConversionToBase);
            }
        }

        public static void AddCategory(string Category, List<string> Units, string PriorityUnit, Func<double, string, string, double> ConversionFunc)
        {
            AddCategory(Category, new HashSet<string>(Units), PriorityUnit, ConversionFunc);
        }

        public static void AddCategory(string Category, HashSet<string> Units, string PriorityUnit, Func<double, string, string, double> ConversionFunc)
        {
            if (UnitCategories.ContainsKey(Category)) throw new Exception($"Category already exists: {Category}");
            UnitCategories[Category] = new HashSet<string>(Units);
            UnitPriorities[Category] = PriorityUnit;
            CategoryConversions[Category] = ConversionFunc;
        }
        #endregion

        #endregion


        private Function FUnit { get; set; }

        public bool IsUnitless => FUnit == null;

        public Unit()
        {
            this.FUnit = null;
        }

        public Unit(Function Unit)
        {
            this.FUnit = Unit;
        }

        public Unit(string Unit)
        {
            if (string.IsNullOrEmpty(Unit)) this.FUnit = null;
            else this.FUnit = new Function(Unit);
        }

        #region Misc Methods

        public (Unit NewUnit, double ConversionRatio) GetConversion(Unit UnitToConvertTo)
        {
            var BaseUnits = UnitToConvertTo.FUnit.Variables;

            var categories = BaseUnits.GroupBy(c => GetUnitCategory(c));

            Dictionary<string, string> PriorityUnits = new Dictionary<string, string>();

            foreach (var category in categories)
            {
                if (category.Count() == 1) PriorityUnits[category.Key] = category.First();
                else PriorityUnits[category.Key] = GetPriorityUnit(category.Key);
            }

            Dictionary<string, Function> ReplaceVals = new Dictionary<string, Function>();
            Dictionary<string, double> subsVals = new Dictionary<string, double>();

            foreach (var unit in FUnit.Variables)
            {
                var Cat = Unit.GetUnitCategory(unit);
                var PriorityUnit = PriorityUnits[Cat];
                var Convers = Unit.ConvertUnit(1, unit, PriorityUnit);
                ReplaceVals[unit] = new Function(PriorityUnit);
                subsVals[unit] = Convers;
            }

            var NewUnit = FUnit.Replace(ReplaceVals);
            var ConversionRatio = FUnit.Subs(subsVals);

            if (NewUnit != UnitToConvertTo.FUnit) throw new Exception($"Units are not compatible: {this} => {UnitToConvertTo}");

            return (new Unit(NewUnit), ConversionRatio);
        }

        public Unit Simplified()
        {
            if (FUnit == null) return new Unit();
            return new Unit(FUnit.GetSimplified());
        }

        #endregion

        #region Overides

        public override string ToString()
        {
            return FUnit?.ToString() ?? "Unitless";
        }

        public static Unit operator +(Unit a, Unit b)
        {
            throw new Exception("Units cannot be added!");
        }
        public static Unit operator -(Unit a, Unit b)
        {
            throw new Exception("Units cannot be subtracted!");
        }
        public static Unit operator *(Unit a, Unit b)
        {
            return new Unit(a.FUnit * b.FUnit);
        }
        public static Unit operator /(Unit a, Unit b)
        {
            return new Unit(a.FUnit / b.FUnit);
        }
        public static Unit operator ^(Unit a, double b)
        {
            return new Unit(a.FUnit ^ b);
        }

        public static bool operator ==(Unit a, Unit b)
        {            
            return a.FUnit == b.FUnit;
        }
        public static bool operator !=(Unit a, Unit b)
        {
            return !(a == b);
        }


        #endregion
    }
}