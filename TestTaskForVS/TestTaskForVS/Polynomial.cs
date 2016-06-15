using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionPolynomial
{
    public class Polynomial
    {
        public class Term
        {
            const string varNames = "abcdefghijklmnopqrstuvwxyz";
            const int firstVarValue = (int)varNames[0];
            const int lastVarValue = (int)varNames[varNames.Length - 1];

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

            public void AddVar(string varName, int pow)
            {
                vars[GetVarIndex(varName[0])] += pow;
            }

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


        }

        List<Term> terms = new List<Term>();

        //varItem[] coeffs = new varItem[1 + varNames.Length];

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
                if (term.IsIdenticalTo(term))
                {
                    term.coeff += term.coeff;
                    haveIdentical = true;
                }
            }
            if (!haveIdentical)
            {
                this.terms.Add(term);
            }
        }

        /// <summary>
        /// Adds constant to polynomial
        /// </summary>
        /// <param name="value"></param>
        public void AddConst(double value)
        {
            var term = new Term();
            term.coeff = value;
            AddTerm(term);
        }
    }
}
