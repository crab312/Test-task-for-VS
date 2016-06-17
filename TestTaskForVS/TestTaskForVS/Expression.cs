using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExpressionParser;

namespace TestTaskForVS
{
    class Expression
    {
        TreeItem exprTree;

        void BuildExpressionTree(string stringExpression)
        {
            var parser = new Parser();
            exprTree = parser.Parse(stringExpression);

            var temp = 0;
        }




        //bool IsCorrectNumber(string token)
        //{

        //    Double.TryParse(token,
        //}

        public Expression(string stringExpression)
        {
            BuildExpressionTree(stringExpression);
        }

        public string ToStandardForm()
        {
            var pol = exprTree.Process();

            var terms = (from term in pol.terms
                        where Math.Abs(term.coeff) > 0.00000001 // filter out zero terms 
                        orderby term.GetMaxPow() descending
                        select term).ToList();


            StringBuilder sr = new StringBuilder();
            for(int i=0; i<terms.Count; i++)
            {
                var term = terms[i];
                if (term.coeff < 0)
                {
                    sr.Append(" - ");
                }
                else if(i != 0)
                {
                    sr.Append(" + ");
                }
                sr.Append(terms[i].ToStringUnsigned());
            }

            var res = sr.ToString();
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
