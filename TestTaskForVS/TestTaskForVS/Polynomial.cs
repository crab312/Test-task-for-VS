using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionParser
{
    public class Polynomial
    {
        public class Term
        {
            public const string varNames = "abcdefghijklmnopqrstuvwxyz";
            static int firstVarValue = (int)varNames[0];
            static int lastVarValue = (int)varNames[varNames.Length - 1];

            public double coeff = 1;
            int[] vars = new int[varNames.Length];

            public static bool IsCorrectVarName(string varName)
            {
                var varIndex = GetVarIndex(varName[0]);
                return varIndex >= 0 && varIndex < varNames.Length;
            }

            static int GetVarIndex(char varName)
            {
                return (int)varName - firstVarValue;
            }

            /// <summary>
            /// Add variable to term. If variable already exists in term then power of variable will be modified
            /// </summary>
            /// <param name="varName">Name of variable</param>
            /// <param name="pow">Power of variable</param>
            public void AddVar(string varName, int pow)
            {
                vars[GetVarIndex(varName[0])] += pow;
            }

            /// <summary>
            /// Returns true if two terms have the same sets of variables
            /// </summary>
            /// <param name="otherTerm">Other term to check</param>
            /// <returns></returns>
            public bool IsIdenticalTo(Term otherTerm)
            {
                for (var i = 0; i < varNames.Length; i++)
                {
                    if (this.vars[i] != otherTerm.vars[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public override string ToString()
            {
                var res = "";
                if (coeff < 0)
                {
                    res += "-";
                }
                return res + ToStringUnsigned();
            }

            /// <summary>
            /// Stringify terms without specifying sign
            /// </summary>
            /// <returns></returns>
            public string ToStringUnsigned()
            {
                var res = "";
                var absCoeff = Math.Abs(coeff);
                for (int i = 0; i < varNames.Length; i++)
                {
                    if (vars[i] > 0)
                    {
                        res += varNames[i];
                        if (vars[i] != 1)
                        {
                            res += '^' + vars[i].ToString();
                        }
                    }
                }

                if (res == String.Empty || absCoeff != 1) 
                {
                    /* 
                     * 1) have vars, but coeff==1 -> do not show coeff
                     * 2) empty res -> no vars -> show 1 
                     */
                    res = absCoeff.ToString(CultureInfo.InvariantCulture) + res;
                }

                return res;
            }

            /// <summary>
            /// Get maximum power from set oo variables in term
            /// </summary>
            /// <returns></returns>
            public int GetMaxPow()
            {
                var res = Int32.MinValue;
                for (int i = 0; i < varNames.Length; i++)
                {
                    if (vars[i] > res)
                    {
                        res = vars[i];
                    }
                }
                return res;
            }

            public double Calculate(List<VarValue> varValues)
            {
                double res = coeff;
                foreach (var varValue in varValues)
                {
                    var varIndx = GetVarIndex(varValue.varName[0]);
                    if(vars[varIndx] != 0)
                    {
                        res = coeff * Math.Pow(varValue.value, vars[varIndx]);
                    }
                }
                return res;
            }
        }

        public struct VarValue
        {
            public string varName;
            public double value;
        }

        /// <summary>
        /// Set of terms, which are included in polynomial
        /// </summary>
        public List<Term> terms {get; private set;}

        public Polynomial()
        {
            terms = new List<Term>();
        }

        /// <summary>
        /// Appends other polynomial
        /// </summary>
        /// <param name="pol">Polynomial to append</param>
        public void AppendPolynomial(Polynomial pol)
        {
            foreach (var term in pol.terms)
            {
                AddTerm(term);
            }
        }

        /// <summary>
        /// Adds term to polynomial
        /// </summary>
        /// <param name="coeff">Coefficient</param>
        /// <param name="vars">Array of variables. Array item value determines power of variable</param>
        public void AddTerm(Term term)
        {
            bool haveIdentical = false;
            foreach (var ourTerm in this.terms)
            {
                if (ourTerm.IsIdenticalTo(term))
                {
                    ourTerm.coeff += term.coeff;
                    haveIdentical = true;
                    break;
                }
            }
            if (!haveIdentical)
            {
                this.terms.Add(term);
            }
        }

        /// <summary>
        /// Inverse sign of coefficients of each tearm in polynomial
        /// </summary>
        public void InverseSigns()
        {
            foreach(var term in this.terms)
            {
                term.coeff *= -1;
            }
        }

        /// <summary>
        /// Calculate numeric value of Polynomial with given variables values
        /// </summary>
        /// <param name="varValues">List of values of variables</param>
        /// <returns>Return result of calculation</returns>
        public double Calculate(List<VarValue> varValues)//double a=0, double b=0, double c=0
        {
            foreach (var t in varValues)
            {
                if (!Term.IsCorrectVarName(t.varName))
                {
                    throw new ArgumentException("Invalid variable name: " + t.varName);
                }
            }
            double res = 0;
            foreach(var term in this.terms)
            {
                res += term.Calculate(varValues);
            }
            return res;
        }

        /// <summary>
        /// Stringify Polynomial in "Standard" form e.g. "ax^2 + bx + c = 0"
        /// </summary>
        /// <returns>String representation of Polynomial</returns>
        public string ToStandardForm()
        {

            var terms = (from term in this.terms
                         where Math.Abs(term.coeff) > 0.00000001 // filter out zero terms 
                         orderby term.GetMaxPow() descending
                         select term).ToList();

            StringBuilder sr = new StringBuilder();
            for (int i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                if (term.coeff < 0)
                {
                    sr.Append(" - ");
                }
                else if (i != 0)
                {
                    sr.Append(" + ");
                }
                sr.Append(terms[i].ToStringUnsigned());
            }

            var res = sr.ToString();
            if (res == "")
            {
                res += "0";
            }
            res += " = 0";

            return res;

            /* 
             Строим дерево выражения
             * обход дерева позволяет легко вычислить
             * вычисление - это не получения числа, а получение некой структуры данных, хранящих полином
             * соотв. должна быть реализована операция сложения и вычитания полиномовъ
             * множество переменных от a до z
             * Степень переменных - любое целое число больше 0.
             
             * Вопрос, как организовать структуру хранения полинома, чтобы она позволяла их складывать - быстро находя соответствующие степени.
             
             ! в полиноме могут быть числа без переменных, их надо просто вычислять !
             
             * у каждой переменной в элементе полинома только одна степень
             ! надо обрабатывать случаи, когда написано x^2 * x^10, это дает ведь x^12
             */
        }

    }
}
