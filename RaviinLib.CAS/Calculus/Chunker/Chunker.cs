
using System;
using System.Linq;
using System.Collections.Generic;

namespace RaviinLib.CAS
{
    public static class Chunker
    {
        #region Char Variables
        public static readonly List<char> DissalowedVarChars = new List<char>()
        {
            ' ', '.', ',','+', '-', '*', '/', '(', ')', '^',  'E', //'e',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'Ç', 'Ë', 'Ê', 'ü', 'é', 'â', 'ä', 'à', 'å', 'ç', 'ê', 'ë',
            'è', 'ï', 'î', 'ì', 'Ä', 'Å', 'É', 'æ', 'Æ', 'ô',
            'ö', 'ò', 'û', 'ù', 'ÿ', 'Þ', 'Ý', 'ß', 'Ŋ', 'Ü'
        };
        public static readonly List<char> DissalowedVarCharsExcludingNumbers = DissalowedVarChars.Where(c => !int.TryParse(c.ToString(), out _)).ToList();
        public static List<string> GetVariables(string Function)
        {
            Function = Function.Replace(" ", "");

            #region Replace functions to char equivilents
            for (int i = 0; i < FunctionStrings.Count; i++)
            {
                string charEquiv = new string(FunctionChars[i], 1);
                Function = Function.Replace(FunctionStrings[i] + '(', charEquiv + '(');
            }
            Function = Function.Replace("π", Math.PI.ToString());
            //Function = Function.Replace("e", Math.E.ToString());
            Function = Function.Replace("E-", "*10^-");
            Function = Function.Replace("E+", "*10^");
            #endregion

            List<string> Variables = new List<string>();

            for (int i = 0; i < Function.Length; i++)
            {
                if (!DissalowedVarChars.Contains(Function[i]))
                {
                    int end = i;
                    while (end + 1 != Function.Length && !DissalowedVarCharsExcludingNumbers.Contains(Function[end + 1]))
                    {
                        end++;
                    }
                    Variables.Add(Function.Substring(i, end - i + 1));
                    i += end - i + 1;
                }
            }
            return Variables.Distinct().ToList();


            //var a = Function.Split(DissalowedVarCharsExcludingNumbers.ToArray());
            //return a.Where(s => s != string.Empty).Distinct().ToList();

            //return Function.ToCharArray().Distinct().Where(c => !DissalowedVarChars.Contains(c)).ToList();
        }
        #endregion

        #region Function Lists
        private static readonly List<string> FunctionStrings = new List<string>() //Order denotes search order
        {
            //"Diff",
            "Max", "Min", "Abs",
            "sqrt","cbrt",
            "Exp",  "ln", "log",
            "ACoth", "ACsch", "ASech",
            "ATanh", "ACosh", "ASinh",
            "ACot", "ACsc", "ASec",
            "ATan", "ACos", "ASin",
            "Coth", "Csch", "Sech",
            "Tanh", "Cosh", "Sinh",
            "Cot", "Csc", "Sec",
            "Tan", "Cos", "Sin",
        };
        private static readonly List<char> FunctionChars = new List<char>() //Order must match order of FunctionStrings
        {
            'Ü', 'Ŋ', 'ß',
            'Ý', 'Þ',
            'ÿ', 'Ë', 'Ê',
            'ù', 'û', 'ò',
            'ö', 'ô', 'Æ',
            'æ', 'É', 'Å',
            'Ä', 'ì', 'î',
            'ï', 'è', 'ë',
            'ê', 'ç', 'å',
            'à', 'ä', 'â',
            'é', 'ü', 'Ç'
        };
        

        #endregion

        public static IChunk Chunckify(string Fx, List<string> Variables = null)
        {
            if (Fx == string.Empty) return null; //throw new("Fx has no length.");

            Fx = Fx.Replace(" ", "");

            #region Replace functions to char equivilents
            for (int i = 0; i < FunctionStrings.Count; i++)
            {
                string charEquiv = new string(FunctionChars[i], 1);
                Fx = Fx.Replace(FunctionStrings[i] + '(', charEquiv + '(');
            }
            Fx = Fx.Replace("π", Math.PI.ToString());
            //Fx = Fx.Replace("e", Math.E.ToString());
            Fx = Fx.Replace("E-", "*10^-");
            Fx = Fx.Replace("E+", "*10^");
            #endregion

            if (Variables == null || Variables.Count == 0) Variables = GetVariables(Fx);

                return SubChunk(Fx, Variables);
            try
            {
            }
            catch (Exception e)
            {
                throw new Exception("Failed to Parse String.", e);
            }
        }

        [Obsolete("Chunkify no longer needs special formatting.")]
        private static (string Formatted, List<string> Variables) FormatString(string Fx, List<string> Variables)
        {
            Fx = Fx.Replace(" ", "");
            if (Fx == string.Empty) return (string.Empty, new List<string>());

            #region Replace functions to char equivilents
            for (int i = 0; i < FunctionStrings.Count; i++)
            {
                string charEquiv = new string(FunctionChars[i], 1);
                Fx = Fx.Replace(FunctionStrings[i], charEquiv);
            }
            #endregion

            if (Variables.Count == 0) Variables = GetVariables(Fx);

            #region Format Coeff and Exp

            for (int i = 0; i < Fx.Length; i++)
            {
                if (!DissalowedVarChars.Contains(Fx[i]))
                {
                    int start = i;
                    int end = i;
                    while (end + 1 != Fx.Length && !DissalowedVarCharsExcludingNumbers.Contains(Fx[end + 1]) && (Variables.Contains(Fx.Substring(start, end - start + 2)) || !Variables.Contains($"{Fx[end]}")))
                    {
                        i++;
                        end++;
                    }


                    if (start == 0 || !int.TryParse(Fx[start - 1].ToString(), out _))
                    {
                        Fx = Fx.Insert(start, "1");
                        i++;
                        end++;
                    }
                    if (end + 1 == Fx.Length || Fx[end + 1] != '^')
                    {
                        Fx = Fx.Insert(end + 1, "^1");
                        i += 2;
                    }
                }
                if (FunctionChars.Contains(Fx[i]))
                {
                    if (i == 0 || !int.TryParse(Fx[i - 1].ToString(), out _))
                    {
                        Fx = Fx.Insert(i, "1");
                    }
                }
                if (Fx[i] == '(')
                {
                    if (i == 0 || !int.TryParse(Fx[i - 1].ToString(), out _))
                    {
                        if (i == 0 || !FunctionChars.Contains(Fx[i - 1]))
                        {
                            Fx = Fx.Insert(i, "1");
                            i++;
                        }
                    }
                }
                if (Fx[i] == ')')
                {
                    if (i + 1 == Fx.Length || Fx[i + 1] != '^')
                    {
                        Fx = Fx.Insert(i + 1, "^1");
                        i += 2;
                    }
                }
                if (Fx[i] == '-')
                {
                    if (i != 0 && Fx[i - 1] != '^' && Fx[i - 1] != '+' && Fx[i - 1] != '*' && Fx[i - 1] != '(' && Fx[i - 1] != '-')
                    {
                        Fx = Fx.Insert(i, "+");
                        i++;
                    }
                    else if (i != 0 && Fx[i - 1] == '-')
                    {
                        Fx = Fx.Remove(i - 1, 2);
                        i -= 2;
                    }
                }
            }
            #endregion

            return (Fx, Variables);
        }

        private static IChunk SubChunk(string Fx, List<string> Variables)
        {
            var s = CheckSumChunk(Fx);
            if (s.IsSum)
            {
                List<IChunk> chunks = new List<IChunk>();
                foreach (var SubString in s.Substrings)
                {
                    chunks.Add(SubChunk(SubString, Variables));
                }
                return new SumChunk(chunks);
            }

            var p = CheckProdChunk(Fx);
            if (p.IsProd)
            {
                List<IChunk> chunks = new List<IChunk>();
                foreach (var SubString in p.Substrings)
                {
                    chunks.Add(SubChunk(SubString.Substring, Variables));
                }

                if (chunks.Count == 1 || chunks.Count == 0) throw new Exception("Failed");

                IChunk Prod = chunks[0];

                for (int i = 1; i < chunks.Count; i++)
                {
                    if (p.Substrings[i - 1].IsNextQuotient == false)
                    {
                        Prod = new ProductChunk(Prod, chunks[i]);
                    }
                    else Prod = new ProductChunk(Prod, new ChainChunk(1, chunks[i], new BaseChunk(-1, null, 1)));
                }

                return Prod;
            }

            var f = CheckFuncChunk(Fx);
            if (f.IsFunc)
            {
                if (double.TryParse(f.Substrings.Exp,out double Exp))
                {
                    IChunk SecondChunk = (f.Substrings.SecondChunk == "") ? null : SubChunk(f.Substrings.SecondChunk, Variables);

                    if (double.TryParse(f.Substrings.Coeff, out double Coeff))
                    {
                        if (Exp != 1)
                        {
                            return new ChainChunk(Coeff, new FuncChunk(SubChunk(f.Substrings.Chunk, Variables), f.Substrings.Func) { SecondChunk = SecondChunk }, new BaseChunk(Exp, null, 1));
                        }

                        return new FuncChunk(SubChunk(f.Substrings.Chunk, Variables), f.Substrings.Func, Coeff) { SecondChunk = SecondChunk };
                    }
                    else
                    {
                        IChunk ICoeff = SubChunk(f.Substrings.Coeff, Variables);

                        if (Exp != 1)
                        {
                            return new ProductChunk(ICoeff, new ChainChunk(1, new FuncChunk(SubChunk(f.Substrings.Chunk, Variables), f.Substrings.Func) { SecondChunk = SecondChunk }, new BaseChunk(Exp, null, 1)));
                        }

                        return new ProductChunk(ICoeff, new FuncChunk(SubChunk(f.Substrings.Chunk, Variables), f.Substrings.Func) { SecondChunk = SecondChunk });
                        
                    }
                }
                else
                {
                    IChunk IExp = SubChunk(f.Substrings.Exp, Variables);

                    IChunk SecondChunk = (f.Substrings.SecondChunk == "") ? null : SubChunk(f.Substrings.SecondChunk, Variables);

                    if (double.TryParse(f.Substrings.Coeff, out double Coeff))
                    {
                        return new ChainChunk(Coeff, new FuncChunk(SubChunk(f.Substrings.Chunk, Variables), f.Substrings.Func) { SecondChunk = SecondChunk }, IExp);
                    }
                    else
                    {
                        IChunk ICoeff = SubChunk(f.Substrings.Coeff, Variables);

                        return new ProductChunk(ICoeff, new ChainChunk(1, new FuncChunk(SubChunk(f.Substrings.Chunk, Variables), f.Substrings.Func) { SecondChunk = SecondChunk }, IExp));
                    }
                }

                
            }

            var c = CheckChainChunk(Fx);
            if (c.IsChain)
            {
                if (double.TryParse(c.Substrings.Coeff, out double Coeff))
                {
                    IChunk Exp = SubChunk(c.Substrings.Exp, Variables);
                    //double Exp = double.Parse(c.Substrings.Exp);

                    return new ChainChunk(Coeff, SubChunk(c.Substrings.Chunk, Variables), Exp);
                }
                else
                {
                    try
                    {
                        IChunk ICoeff = SubChunk(c.Substrings.Coeff, Variables);
                        IChunk Exp = SubChunk(c.Substrings.Exp, Variables);

                        return new ProductChunk(ICoeff,new ChainChunk(1, SubChunk(c.Substrings.Chunk, Variables), Exp));
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            var IsBase = TryParseBaseChunk(Fx, Variables, out IChunk b);
            if (IsBase)
            {
                return b;
            }

            if (double.TryParse(Fx, out _))
            {
                return new BaseChunk(double.Parse(Fx), null, 1);
            }

            throw new Exception($"Failed to parse \"{Fx}\"");
            //return new BaseChunk(0, null, 1);
        }

        private static (bool IsSum, List<string> Substrings) CheckSumChunk(string Chunk)
        {
            List<string> Substrings = new List<string>();

            int LastPlus = 0;

            int skip = 0;
            for (int i = 0; i < Chunk.Length; i++)
            {
                if (Chunk[i] == '+' && skip == 0)
                {
                    Substrings.Add(Chunk.Substring(LastPlus, i - LastPlus));

                    LastPlus = i + 1;
                }
                else if (Chunk[i] == '-' && skip == 0 && i != 0 && Chunk[i - 1] != '+' && Chunk[i - 1] != '^' && Chunk[i - 1] != '*' && Chunk[i - 1] != '/')
                {
                    Substrings.Add(Chunk.Substring(LastPlus, i - LastPlus));

                    LastPlus = i;
                }
                else if (Chunk[i] == '(') skip++;
                else if (Chunk[i] == ')') skip--;
            }

            if (Substrings.Count > 0)
            {
                Substrings.Add(Chunk.Substring(LastPlus, Chunk.Length - LastPlus));
            }

            return (Substrings.Count > 0, Substrings);
        }

        private static (bool IsProd, List<(string Substring, bool IsNextQuotient)> Substrings) CheckProdChunk(string Chunk)
        {
            List<(string, bool)> Substrings = new List<(string, bool)>();

            int LastPlus = -1;

            int skip = 0;
            for (int i = 0; i < Chunk.Length; i++)
            {
                if (Chunk[i] == '*' && skip == 0)
                {
                    Substrings.Add((Chunk.Substring(LastPlus + 1, i - LastPlus - 1), false));

                    LastPlus = i;
                }
                else if (Chunk[i] == '/' && skip == 0)
                {
                    Substrings.Add((Chunk.Substring(LastPlus + 1, i - LastPlus - 1), true));

                    LastPlus = i;
                }
                else if (Chunk[i] == '(') skip++;
                else if (Chunk[i] == ')') skip--;
            }

            if (Substrings.Count > 0)
            {
                Substrings.Add((Chunk.Substring(LastPlus + 1, Chunk.Length - LastPlus - 1), false));
                if (Substrings.Last().Item1 == "")
                {

                }
            }

            return (Substrings.Count > 0, Substrings);
        }

        private static (bool IsFunc, (string Coeff, string Chunk, string SecondChunk, string Exp, Functions Func) Substrings) CheckFuncChunk(string Chunk)
        {
            List<string> Substrings = new List<string>();

            int OpenIndex = Chunk.IndexOf('(');
            if (OpenIndex <= 0 || !FunctionChars.Contains(Chunk[OpenIndex - 1])) return (false, (null,null,null,null,Functions.Abs));

            int CloseIndex = OpenIndex;


            int skip = 0;
            for (int i = OpenIndex; i < Chunk.Length; i++)
            {
                if (Chunk[i] == '(') skip++;
                else if (Chunk[i] == ')')
                {
                    skip--;
                    if (skip == 0)
                    {
                        CloseIndex = i;
                        break;
                    }
                }
            }

            (string Coeff, string Chunk, string SecondChunk, string Exp, Functions Func) Return = (null,null,null,null,Functions.Abs);

            if (OpenIndex != CloseIndex)
            {
                Return.Coeff = (OpenIndex == 1) ? "1" : Chunk.Substring(0, OpenIndex - 1);
                Return.Coeff = (Return.Coeff == "-") ? "-1" : Return.Coeff;
                Return.Func = (Functions)Chunk[OpenIndex - 1];


                Return.Chunk = Chunk.Substring(OpenIndex + 1, CloseIndex - (OpenIndex + 1));
                if (Return.Chunk.Contains(','))
                {
                    var Halfs = Return.Chunk.Split(',');
                    Return.Chunk = Halfs[0];
                    Return.SecondChunk = Halfs[1];
                }
                else
                {
                    Return.SecondChunk = "";
                }

                Return.Exp = (CloseIndex + 2 >= Chunk.Length) ? "1" : Chunk.Substring(CloseIndex + 2, (Chunk.Length - 1) - (CloseIndex + 1));
            }

            return (OpenIndex != CloseIndex, Return);
        }

        private static (bool IsChain, (string Coeff, string Chunk, string Exp) Substrings) CheckChainChunk(string Chunk)
        {
            List<string> Substrings = new List<string>();

            int OpenIndex = Chunk.IndexOf('(');
            if (OpenIndex == -1) return (false, (null,null,null));

            int CloseIndex = OpenIndex;


            int skip = 0;
            for (int i = OpenIndex; i < Chunk.Length; i++)
            {
                if (Chunk[i] == '(') skip++;
                else if (Chunk[i] == ')')
                {
                    skip--;
                    if (skip == 0)
                    {
                        CloseIndex = i;
                        break;
                    }
                }
            }

            (string Coeff, string Chunk, string Exp) Return = (null,null,null);

            if (OpenIndex != CloseIndex)
            {
                Return.Coeff = (OpenIndex == 0) ? "1" : Chunk.Substring(0, OpenIndex);
                Return.Coeff = (Return.Coeff == "-") ? "-1" : Return.Coeff;
                Return.Chunk = Chunk.Substring(OpenIndex + 1, CloseIndex - (OpenIndex + 1));
                Return.Exp = (CloseIndex == Chunk.Length - 1) ? "1" : Chunk.Substring(CloseIndex + 2, (Chunk.Length - 1) - (CloseIndex + 1));

                if (Return.Coeff == "") //!double.TryParse(Return.Coeff, out _)
                {
                    Return.Coeff = "1";
                }
                if (Return.Exp == "") //!double.TryParse(Return.Exp, out _)
                {
                    Return.Exp = "1";
                }

            }

            return (OpenIndex != CloseIndex, Return);
        }

        private static bool IsNumber(char c)
        {
            return (c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9');
        }
        public static bool TryParseBaseChunk(string Fx, List<string> Variables, out IChunk b)
        {
            b = null;

            int varIndex = -1;//Variables.Select(c => Fx.IndexOf(c)).Where(c => c != -1);

            foreach (var var in Variables)
            {
                var ind = Fx.IndexOf(var);
                if (ind != -1 && ind < varIndex) { varIndex = ind; continue; }
                if (ind != -1 && varIndex == -1) varIndex = ind;
            }

            //for (int i = 0; i < Fx.Length; i++)
            //{
            //    if (!DissalowedVarChars.Contains(Fx[i]))
            //    {
            //        int start = i;
            //        int end = i;

            //        var wasd1 = !DissalowedVarCharsExcludingNumbers.Contains(Fx[end + 1]);
            //        var subst = Fx.Substring(start, end - start + 1);
            //        var wasd2 = (!Variables.Contains(subst));

            //        while (end + 1 != Fx.Length && (wasd1 && wasd2) ) // || !Variables.Contains($"{Fx[end]}")
            //        {


            //            i++;
            //            end++;

            //            var p = end + 1 != Fx.Length;
            //            var y = !Variables.Contains(Fx.Substring(start, end - start + 1));
            //            var z = !Variables.Contains($"{Fx[end]}");
            //        }

            //        varIndexs.Add(start);
            //        //string Var = Fx.Substring(start, end - start + 1);
            //    }
            //}


            if (Variables.Contains(Fx)) 
            { 
                b = new BaseChunk(Fx);
                return true;
            }

            //bool ContainsVariable = varIndexs.Count() > 0;
            bool ContainsVariable = varIndex != -1;
            var powIndex = Fx.IndexOf('^');

            if (powIndex != -1)
            {
                var split = Fx.Split('^');
                if (split.Length == 2 && Variables.Contains(Fx))
                {
                    b = new BaseChunk(1,split[0], double.Parse(split[1]));
                    return true;
                }
            }

            if (powIndex != -1) // a^() or ax^()
            {
                if (double.TryParse(Fx.Substring(0, powIndex), out double Num)) //2^2 or 2^()
                {
                    if (double.TryParse(Fx.Substring(powIndex + 1), out double NumExp)) //2^2
                    {
                        b = new BaseChunk(Num, null, NumExp);
                        return true;
                    }
                    else // 2^()
                    {
                        try
                        {
                            IChunk IChunkExp = SubChunk(Fx.Substring(powIndex + 1), Variables);
                            b = new ChainChunk(1, new BaseChunk(Num, null, 1), IChunkExp);
                            return true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else //x^b or ax^b or ax^() or x^()
                {
                    //int varStartIndex = varIndexs.OrderBy(c => c).First();
                    int varStartIndex = varIndex;
                    string var = Fx.Substring(varStartIndex, powIndex - varStartIndex);
                    string coeff = Fx.Substring(0, varStartIndex);
                    coeff = (coeff == "") ? "1" : (coeff == "-") ? "-1" : coeff;

                    if (double.TryParse(coeff, out double NumCoeff)) //Has Valid Coeff
                    {
                        if (double.TryParse(Fx.Substring(powIndex + 1), out double NumExp)) //x^b or ax^b
                        {
                            b = new BaseChunk(NumCoeff, var, NumExp);
                            return true;
                        }
                        else // ax^() or x^()
                        {
                            try
                            {
                                IChunk IChunkExp = SubChunk(Fx.Substring(powIndex + 1), Variables);
                                b = new ChainChunk(1, new BaseChunk(NumCoeff, var, 1), IChunkExp);
                                return true;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }


            }
            else if (powIndex == -1) //a or ax
            {
                if (!ContainsVariable && double.TryParse(Fx, out double Num)) //2
                {
                    b = new BaseChunk(Num, null, 1);
                    return true;
                }
                else if (varIndex == 0) //x
                {
                    b = new BaseChunk(1, Fx, 1);
                    return true;
                }
                else if (double.TryParse((Fx.Substring(0, varIndex) == "-") ? "-1" : Fx.Substring(0, varIndex), out Num))//2x
                {
                    b = new BaseChunk(Num, Fx.Substring(varIndex), 1);
                    return true;
                }
            }

            b = null;
            return false;
        }
    }
}
