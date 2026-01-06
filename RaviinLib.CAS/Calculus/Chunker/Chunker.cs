
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public static readonly HashSet<char> DisallowedVarCharsSet = new HashSet<char>
        {
            ' ', '.', ',', '+', '-', '*', '/', '(', ')', '^', 'E',
            '0','1','2','3','4','5','6','7','8','9',
            'Ç','Ë','Ê','ü','é','â','ä','à','å','ç','ê','ë',
            'è','ï','î','ì','Ä','Å','É','æ','Æ','ô',
            'ö','ò','û','ù','ÿ','Þ','Ý','ß','Ŋ','Ü'
        };

        public static readonly HashSet<char> DissalowedVarCharsExcludingNumbersSet = new HashSet<char>
        {
            ' ', '.', ',', '+', '-', '*', '/', '(', ')', '^', 'E',
            'Ç','Ë','Ê','ü','é','â','ä','à','å','ç','ê','ë',
            'è','ï','î','ì','Ä','Å','É','æ','Æ','ô',
            'ö','ò','û','ù','ÿ','Þ','Ý','ß','Ŋ','Ü'
        };

        public static readonly List<char> DissalowedVarCharsExcludingNumbers = DissalowedVarChars.Where(c => !int.TryParse(c.ToString(), out _)).ToList();

        [Obsolete("No longer needed or maintained.", true)]
        public static List<string> GetVariables(string Function)
        {
            Function = Function.Replace(" ", "");

            #region Replace functions to char equivilents
            for (int i = 0; i < FunctionStrings.Count; i++)
            {
                string charEquiv = new string(FunctionChars[i], 1);
                Function = Function.Replace(FunctionStrings[i] + '(', charEquiv + '(');
            }
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

        public static IChunk Chunckify(string Fx)
        {
            if (Fx == string.Empty) return null; //throw new("Fx has no length.");

            Fx = Fx.Replace(" ", "");

            #region Replace functions to char equivilents
            for (int i = 0; i < FunctionStrings.Count; i++)
            {
                string charEquiv = new string(FunctionChars[i], 1);
                Fx = Fx.Replace(FunctionStrings[i] + '(', charEquiv + '(');
            }
            #endregion

            try
            {
                return SubChunk(Fx.AsSpan());
            }
            catch (Exception e)
            {
                throw new Exception("Failed to Parse String.", e);
            }
        }

        [Obsolete("Chunkify no longer needs special formatting.", true)]
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

        private static IChunk SubChunk(ReadOnlySpan<char> Fx)
        {
            var s = CheckSumChunk(Fx);
            if (s.IsSum)
            {
                List<IChunk> chunks = new List<IChunk>(s.Substrings.Count);

                foreach (var (start, length) in s.Substrings)
                {
                    var slice = Fx.Slice(start, length); // one alloc per term
                    chunks.Add(SubChunk(slice));
                }
                return new SumChunk(chunks);
            }

            var p = CheckProdChunk(Fx);
            if (p.IsProd)
            {
                var n = p.Substrings.Count;
                if (n <= 1) throw new Exception("Failed");

                var first = p.Substrings[0];
                IChunk Prod = SubChunk(Fx.Slice(first.Start, first.Length));

                for (int i = 1; i < n; i++)
                {
                    var current = p.Substrings[i];
                    var slice = Fx.Slice(current.Start, current.Length); // one alloc per term
                    var chunk = SubChunk(slice);

                    if (!p.Substrings[i-1].IsNextQuotient)
                    {
                        Prod = new ProductChunk(Prod, chunk);
                    }
                    else Prod = new ProductChunk(Prod, new ChainChunk(1, chunk, new BaseChunk(-1, null, 1)));
                }

                return Prod;
            }

            var f = CheckFuncChunk(Fx);
            if (f.IsFunc)
            {
                string coeffStr = Fx.Slice(f.Substrings.Coeff.Start, f.Substrings.Coeff.Length).ToString();
                coeffStr = (coeffStr == "") ? "1" : (coeffStr == "-") ? "-1" : coeffStr;

                string expStr = Fx.Slice(f.Substrings.Exp.Start, f.Substrings.Exp.Length).ToString();
                expStr = (expStr == "") ? "1" : (expStr == "-") ? "-1" : expStr;

                IChunk SecondChunk = (f.Substrings.SecondChunk.Start == 0 && f.Substrings.SecondChunk.Length == 0) ? null : SubChunk(Fx.Slice(f.Substrings.SecondChunk.Start, f.Substrings.SecondChunk.Length));

                if (double.TryParse(expStr, out double Exp))
                {
                    if (double.TryParse(coeffStr, out double Coeff))
                    {
                        if (Exp != 1)
                        {
                            return new ChainChunk(Coeff, new FuncChunk(SubChunk(Fx.Slice(f.Substrings.Chunk.Start, f.Substrings.Chunk.Length)), f.Substrings.Func) { SecondChunk = SecondChunk }, new BaseChunk(Exp, null, 1));
                        }

                        return new FuncChunk(SubChunk(Fx.Slice(f.Substrings.Chunk.Start, f.Substrings.Chunk.Length)), f.Substrings.Func, Coeff) { SecondChunk = SecondChunk };
                    }
                    else
                    {
                        IChunk ICoeff = SubChunk(coeffStr.AsSpan());

                        if (Exp != 1)
                        {
                            return new ProductChunk(ICoeff, new ChainChunk(1, new FuncChunk(SubChunk(Fx.Slice(f.Substrings.Chunk.Start, f.Substrings.Chunk.Length)), f.Substrings.Func) { SecondChunk = SecondChunk }, new BaseChunk(Exp, null, 1)));
                        }

                        return new ProductChunk(ICoeff, new FuncChunk(SubChunk(Fx.Slice(f.Substrings.Chunk.Start, f.Substrings.Chunk.Length)), f.Substrings.Func) { SecondChunk = SecondChunk });
                        
                    }
                }
                else
                {
                    IChunk IExp = SubChunk(expStr.AsSpan());
                    
                    if (double.TryParse(coeffStr, out double Coeff))
                    {
                        return new ChainChunk(Coeff, new FuncChunk(SubChunk(Fx.Slice(f.Substrings.Chunk.Start, f.Substrings.Chunk.Length)), f.Substrings.Func) { SecondChunk = SecondChunk }, IExp);
                    }
                    else
                    {
                        IChunk ICoeff = SubChunk(coeffStr.AsSpan());

                        return new ProductChunk(ICoeff, new ChainChunk(1, new FuncChunk(SubChunk(Fx.Slice(f.Substrings.Chunk.Start, f.Substrings.Chunk.Length)), f.Substrings.Func) { SecondChunk = SecondChunk }, IExp));
                    }
                }

                
            }

            var c = CheckChainChunk(Fx);
            if (c.IsChain)
            {
                string coeffStr = Fx.Slice(c.Substrings.Coeff.Start, c.Substrings.Coeff.Length).ToString();
                coeffStr = (coeffStr == "") ? "1" : (coeffStr == "-") ? "-1" : coeffStr;

                string expStr = Fx.Slice(c.Substrings.Exp.Start, c.Substrings.Exp.Length).ToString();
                expStr = (expStr == "") ? "1" : (expStr == "-") ? "-1" : expStr;

                if (double.TryParse(coeffStr, out double Coeff))
                {
                    IChunk Exp = SubChunk(expStr.AsSpan());
                    //double Exp = double.Parse(c.Substrings.Exp);

                    return new ChainChunk(Coeff, SubChunk(Fx.Slice(c.Substrings.Chunk.Start, c.Substrings.Chunk.Length)), Exp);
                }
                else
                {
                    try
                    {
                        IChunk ICoeff = SubChunk(coeffStr.AsSpan());
                        IChunk Exp = SubChunk(expStr.AsSpan());

                        return new ProductChunk(ICoeff,new ChainChunk(1, SubChunk(Fx.Slice(c.Substrings.Chunk.Start, c.Substrings.Chunk.Length)), Exp));
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            if (TryParseBaseChunk(Fx, out IChunk b))
            {
                return b;
            }

            throw new Exception($"Failed to parse \"{Fx.ToString()}\"");
            //return new BaseChunk(0, null, 1);
        }

        private static (bool IsSum, List<(int Start, int Length)> Substrings) CheckSumChunk(ReadOnlySpan<char> Chunk)
        {
            List<(int Start, int Length)> Substrings = new List<(int Start, int Length)>(4);

            int LastSign = 0;
            int skip = 0;

            for (int i = 0; i < Chunk.Length; i++)
            {
                char c = Chunk[i];
                char cPrev;
                if (i == 0) cPrev = ' ';
                else cPrev = Chunk[i - 1];

                if (Chunk[i] == '(') { skip++; continue; }
                if (Chunk[i] == ')') { skip--; continue; }

                if (skip == 0)
                {

                    if (c == '+' && cPrev != 'E')
                    {
                        Substrings.Add((LastSign, i - LastSign));

                        LastSign = i + 1;
                        continue;
                    }

                    if (c == '-' &&
                        i != 0 && 
                        cPrev != '+' && 
                        cPrev != '^' && 
                        cPrev != '*' &&
                        cPrev != '/' &&
                        cPrev != 'E')
                    {
                        Substrings.Add((LastSign, i - LastSign));

                        LastSign = i;
                        continue;
                    }
                }
            }

            if (Substrings.Count > 0)
            {
                Substrings.Add((LastSign, Chunk.Length - LastSign));
            }

            return (Substrings.Count > 0, Substrings);
        }

        private static (bool IsProd, List<(int Start, int Length, bool IsNextQuotient)> Substrings) CheckProdChunk(ReadOnlySpan<char> Chunk)
        {
            List<(int Start, int Length, bool IsNextQuotient)> Substrings = new List<(int Start, int Length, bool IsNextQuotient)>(4);

            int LastSign = 0;

            int skip = 0;

            for (int i = 0; i < Chunk.Length; i++)
            {
                char c = Chunk[i];

                if (c == '(') { skip++; continue; }
                if (c == ')') { skip--; continue; }

                if (skip == 0)
                {
                    if (c == '*')
                    {
                        Substrings.Add((LastSign, i - LastSign, false));

                        LastSign = i + 1;
                        continue;
                    }

                    if (c == '/')
                    {
                        Substrings.Add((LastSign, i - LastSign, true));

                        LastSign = i + 1;
                        continue;

                    }
                }
                
            }

            if (Substrings.Count > 0)
            {
                Substrings.Add((LastSign, Chunk.Length - LastSign, false));
            }

            return (Substrings.Count > 0, Substrings);
        }

        private static 
            (
                bool IsFunc, 
                (
                    (int Start, int Length) Coeff, 
                    (int Start, int Length) Chunk, 
                    (int Start, int Length) SecondChunk, 
                    (int Start, int Length) Exp, 
                    Functions Func
                ) Substrings 
            ) CheckFuncChunk(ReadOnlySpan<char> Chunk)
        {

            int OpenIndex = Chunk.IndexOf('(');
            if (OpenIndex <= 0 || !FunctionChars.Contains(Chunk[OpenIndex - 1])) 
                return (false, ((0,0), (0, 0), (0, 0), (0, 0), Functions.Abs));


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

            ((int Start, int Length) Coeff, (int Start, int Length) Chunk, (int Start, int Length) SecondChunk, (int Start, int Length) Exp, Functions Func) Return = (((0, 0), (0, 0), (0, 0), (0, 0), Functions.Abs));

            if (OpenIndex != CloseIndex)
            {
                // Coeff
                int coeffStart = 0;
                int coeffLength = OpenIndex-1;
                if (coeffLength < 0) coeffLength = 0; // default to ""
                Return.Coeff = (coeffStart, coeffLength);

                //Function
                Return.Func = (Functions)Chunk[OpenIndex - 1];

                //Chunk
                Return.Chunk = (OpenIndex + 1, CloseIndex - (OpenIndex + 1));

                //Second Chunk
                var CommaIndex = Chunk.Slice(Return.Chunk.Start, Return.Chunk.Length).IndexOf(',');
                if (CommaIndex >= 0)
                {
                    Return.SecondChunk = (Return.Chunk.Start + CommaIndex + 1, Return.Chunk.Length - CommaIndex - 1);
                    Return.Chunk = (Return.Chunk.Start, CommaIndex);
                }
                else
                {
                    Return.SecondChunk = (0, 0);
                }

                //Exp
                int expStart = CloseIndex + 2;
                if (expStart < Chunk.Length)
                {
                    if (Chunk[expStart - 1] != '^') throw new Exception($"Failed to parse exponent of: {Chunk.ToString()}");
                    
                    Return.Exp = (expStart, Chunk.Length - expStart);
                }
                else
                {
                    if (CloseIndex+1 != Chunk.Length) throw new Exception($"Failed to parse exponent of: {Chunk.ToString()}");
                    Return.Exp = (0, 0);
                }
            }

            return (OpenIndex != CloseIndex, Return);
        }

        private static (bool IsChain, ((int Start, int Length) Coeff, (int Start, int Length) Chunk, (int Start, int Length) Exp) Substrings) CheckChainChunk(ReadOnlySpan<char> Chunk)
        {
            int OpenIndex = Chunk.IndexOf('(');
            if (OpenIndex == -1) return (false, ((0,0), (0, 0), (0, 0)));

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

            ((int Start, int Length) Coeff, (int Start, int Length) Chunk, (int Start, int Length) Exp) Return = ((0, 0), (0, 0), (0, 0));

            if (OpenIndex != CloseIndex)
            {
                // Coeff
                int coeffStart = 0;
                int coeffLength = OpenIndex;
                if (coeffLength < 0) coeffLength = 0; // default to ""
                Return.Coeff = (coeffStart, coeffLength);

                //Chunk
                Return.Chunk = (OpenIndex + 1, CloseIndex - (OpenIndex + 1));

                //Exp
                int expStart = CloseIndex + 2;
                if (expStart < Chunk.Length)
                {
                    Return.Exp = (expStart, Chunk.Length - expStart);
                }
                else
                {
                    Return.Exp = (0, 0);
                }

                if (CloseIndex + 1 != Chunk.Length && Chunk.Slice(CloseIndex,Chunk.Length - CloseIndex).IndexOf('^') == -1) throw new Exception($"Failed to parse {Chunk.ToString()}");
            }

            return (OpenIndex != CloseIndex, Return);
        }

        public static bool TryParseBaseChunk(ReadOnlySpan<char> Fx, out IChunk b)
        {
            b = null;

            var powIndex = Fx.IndexOf('^');
            ReadOnlySpan<char> firstVar = null;
            int varIndex = -1;
            for (int i = 0; i < Fx.Length; i++)
            {
                if (!Chunker.DisallowedVarCharsSet.Contains(Fx[i]))
                {
                    varIndex = i;
                    if (powIndex == -1)
                    {
                        firstVar = Fx.Slice(i);
                        break;
                    }
                    else
                    {
                        firstVar = Fx.Slice(i, powIndex - i);
                        break;
                    }
                }
            }

            // Fx = x
            if (firstVar != null && Fx.SequenceEqual(firstVar))
            {
                b = new BaseChunk(Fx.ToString());
                return true;
            }

            

            if (powIndex != -1)
            {
                ReadOnlySpan<char> left = Fx.Slice(0, powIndex);
                ReadOnlySpan<char> right = Fx.Slice(powIndex + 1);

                if (double.TryParse(left.ToString(),out double numCoeff))
                {
                    if (double.TryParse(right.ToString(), out double numExp))
                    {
                        // a^b
                        b = new BaseChunk(numCoeff, null, numExp);
                        return true;
                    }
                    else
                    {
                        // a^()
                        try
                        {
                            IChunk expChunk = SubChunk(right);
                            b = new ChainChunk(1, new BaseChunk(numCoeff, null, 1), expChunk);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
                {
                    // Left contains variable(s)

                    if (varIndex == -1)
                        return false; // can't parse

                    ReadOnlySpan<char> coeffSpan = Fx.Slice(0, varIndex);
                    string varName = Fx.Slice(varIndex, powIndex - varIndex).ToString();
                    double coeff = 1;

                    if (coeffSpan.Length > 0)
                    {
                        string coeffStr = coeffSpan.SequenceEqual("-".AsSpan()) ? "-1" : coeffSpan.ToString();
                        if (!double.TryParse(coeffStr, out coeff))
                            return false;
                    }


                    if (double.TryParse(right.ToString(), out double numExp))
                    {
                        // ()^b
                        b = new BaseChunk(coeff, varName, numExp);
                        return true;
                    }
                    else
                    {
                        // ()^()
                        try
                        {
                            IChunk expChunk = SubChunk(right);
                            b = new ChainChunk(1, new BaseChunk(coeff, varName, 1), expChunk);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                }
            }


            if  (varIndex == -1)
            {
                // a
                if (double.TryParse(Fx.ToString(), out double num))
                {
                    b = new BaseChunk(num, null, 1);
                    return true;
                }
            }
            else if (varIndex == 0)
            {
                // x
                b = new BaseChunk(1, Fx.ToString(), 1);
                return true;
            }
            else
            {
                // ax
                ReadOnlySpan<char> coeffSpan = Fx.Slice(0, varIndex);
                double coeff = 1;
                if (!double.TryParse(coeffSpan.SequenceEqual("-".AsSpan()) ? "-1" : coeffSpan.ToString(), out coeff))
                    return false;

                string varName = Fx.Slice(varIndex).ToString();
                b = new BaseChunk(coeff, varName, 1);
                return true;
            }

            return false;
        }
    }
}
