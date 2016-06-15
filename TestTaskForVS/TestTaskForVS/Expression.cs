using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExpressionPolynomial;

namespace TestTaskForVS
{
    class Expression
    {
        class Parser
        {
            string expression;
            int pos;

            /* 
             
             EqExpr
                Expression = Expression
             
             LeftExpression
                 operand
                 +-operand
                 +-operand+-RightExpression
             
             RightExpression
                operand
                operand+-RightExpression
                 
             Operand
                 (LeftExpression)
                 OperandMember1(LeftExpression)
                 OperandMember1 OperandMember2 OperandMember3
             
            
             OperandMember1
                Variable
                Coeff
                
             Variable
                VarName
                VarName ^ power
             

             */


            public TreeItem Parse(string expression)
            {
                this.expression = expression;
                pos = -1;

                TreeBinaryOperation ti = new TreeBinaryOperation();
                ti.a = ReadExpression(UnaryAllowed: true);
                if (ReadChar() != '=')
                {
                    throw new Exception("Expected '='");
                }
                ti.op = Operation.equals;
                ti.b = ReadExpression(UnaryAllowed: true);
                return ti;
            }

            TreeItem ReadExpression(bool UnaryAllowed = false)
            {
                TreeItem leftOperand = null;
                while(pos < expression.Length-1)
                {
                    var ch = expression[++pos];
                    if (Char.IsWhiteSpace(ch))
                    {
                        continue;
                    }
                    else if (ch == '+' || ch == '-')
                    {
                        if (leftOperand == null)
                        {
                            if (UnaryAllowed)
                            {
                                leftOperand = new TreeUnaryOperation()
                                {
                                    op = ch == '+' ? Operation.plus : Operation.minus,
                                    a = ReadExpression(),
                                };
                            }
                            else
                            {
                                throw new Exception(String.Format("Char '{0}' not expected. Expected: operand.", ch));
                            }
                        }
                        else
                        {
                            return new TreeBinaryOperation()
                            {
                                a = leftOperand,
                                b = ReadExpression(),
                                op = ch == '+' ? Operation.plus : Operation.minus,

                            };
                        }
                    }
                    else if (ch == '=' || ch == ')')
                    {
                        if (leftOperand == null)
                        {
                            throw new Exception(String.Format("Char '{0}' not expected. Expression must appear before '{0}'", ch));
                        }
                        else
                        {
                            return leftOperand;
                        }
                    }
                    else
                    {
                        if (leftOperand == null)
                        {
                            leftOperand = ReadOperand();
                        }
                        else
                        {
                            throw new Exception("Expected one of {'+','-','='})");
                        }
                    }
                };

                return null;
            }

            TreeItem ReadOperand()
            {
                TreeItem curOperandMember = null;
                while (pos < expression.Length-1)
                {
                    var ch = expression[++pos];
                    if (Char.IsWhiteSpace(ch))
                    {
                        continue;
                    }
                    else if (ch == '+' || ch == '-' || ch == '=')
                    {
                        if (curOperandMember == null)
                        {
                            throw new Exception(String.Format("Char '{0}' not expected. Expected: OperandMember or bracketed expression.", ch));
                        }
                    }
                    else if (ch == '(')
                    {
                        var exprRes = ReadExpression();
                        var bracketEnd = expression[pos];
                        if (bracketEnd != ')')
                        {
                            throw new Exception("Expected end bracket ')'");
                        }

                        if (curOperandMember != null)
                        {
                            curOperandMember = new TreeBinaryOperation()
                            {
                                a = curOperandMember,
                                b = exprRes,
                                op = Operation.mult,
                            };
                        }
                        else
                        {
                            curOperandMember = exprRes;
                        }
                    }
                    else
                    {
                        if (curOperandMember == null)
                        {
                            curOperandMember = ReadOperandMember();
                        }
                        else
                        {
                            curOperandMember = new TreeBinaryOperation()
                            {
                                a = curOperandMember,
                                b = ReadOperandMember(),
                                op = Operation.mult,
                            };
                        }
                    }
                };
                return null;
            }

            public TreeItem ReadOperandMember()
            {
                TreeTerm operandMember = null;
                string curVarName = null;
                while (pos < expression.Length-1)
                {
                    var ch = expression[++pos];
                    if (Char.IsWhiteSpace(ch))
                    {
                        continue;
                    }
                    else if (ch == '+' || ch == '-' || ch == '=' || ch == '(')
                    {
                        if (operandMember == null)
                        {
                            throw new Exception(String.Format("Char '{0}' not expected. Expected: variable or number", ch));
                        }
                    }
                    else if (ch == '^')
                    {
                        if (curVarName == null || operandMember == null)
                        {
                            throw new Exception(String.Format("Char '{0}' not expected. Expected: variable or number", ch));
                        }
                        else
                        {
                            var pow = TryReadInteger();
                            
                        }
                    }
                    else if (Char.IsNumber(ch))
                    {
                        var coeff = TryReadNumber();
                        if (operandMember == null)
                        {
                            operandMember = new TreeTerm()
                            {
                                term = new Polynomial.Term()
                                {
                                    coeff = coeff
                                }
                            };
                        }
                        else
                        {
                            operandMember.term.coeff *= coeff;
                        }
                    }
                    else if (Char.IsLetter(ch))
                    {
                        //string varName = "";
                        //int pow = 1;
                        //ReadVariable(out varName, out pow);

                        string varName = ch.ToString();
                        if (operandMember == null)
                        {
                            operandMember = new TreeTerm()
                            {
                                term = new Polynomial.Term()
                                {
                                    coeff = 1
                                }
                            };
                        }
                        operandMember.term.AddVar(varName, pow);
                    }

                }

                //НЕ СТЫКУЕТСЯ НИЧЕГО (т.к. перескакиваем на 1 символ)

                return null;
            }

            //void ReadVariable(out string varName, out int pow)
            //{
            //    varName = expression[pos].ToString();
            //    while (pos < expression.Length)
            //    {
            //        var ch = expression[++pos];
            //        if (Char.IsLetter(ch))
            //        {

            //        }
            //    }
            //}


            int TryReadInteger()
            {
                var numChars = new char[30];
                int numPos = 0;
                while (pos < expression.Length)
                {
                    var ch = expression[++pos];
                    if (Char.IsNumber(ch) || ch == '.')
                    {
                        numChars[numPos++] = ch;
                    }
                    else
                    {
                        break;
                    }
                }
                var numStr = new String(numChars, 0, numPos);
                int res = 0;
                if (Int32.TryParse(numStr, out res))
                {
                    return res;
                }
                else
                {
                    throw new Exception("Invalid integer: " + numStr);
                }
            }

            double TryReadNumber()
            {
                var numChars = new char[30];
                int numPos = 0;
                while (true)
                {
                    var ch = NextChar();
                    if (Char.IsNumber(ch) || ch == '.')
                    {
                        numChars[numPos++] = ch;
                    }else
                    {
                        break;
                    }
                }
                var numStr = new String(numChars, 0, numPos);
                double res = 0;
                if (double.TryParse(numStr, out res))
                {
                    return res;
                }
                else
                {
                    throw new Exception("Invalid number: " + numStr);
                }
            }

            bool NextChar()
            {
                if (pos < expression.Length - 1)
                {
                    pos++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            char ReadChar()
            {
                return expression[pos];
            }

            bool IsVariable(string token)
            {
                return Polynomial.Term.IsCorrectVarName(token);
            }

            bool IsOperation(char ch)
            {
                return ch == '+' || ch == '-';
            }
        }

        void Process(string stringExpression)
        {


            var token = "33434";

            //var curTerm = new Polynomial.Term();


        }


        //bool IsCorrectNumber(string token)
        //{

        //    Double.TryParse(token,
        //}

        public Expression(string stringExpression)
        {
            Process(stringExpression);
        }

        public string ToStandardForm()
        {
            return "";

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

        enum Operation { plus, minus, mult, divide, power, equals };

        

        class TreeItem
        {

        }

        class TreeBinaryOperation : TreeItem
        {
            public Operation op;
            public TreeItem a;
            public TreeItem b;
        }

        class TreeUnaryOperation : TreeItem
        {
            public Operation op;
            public TreeItem a;
        }

        class TreeTerm : TreeItem
        {
            public Polynomial.Term term;
        }




        
        public struct CalcResult
        {


        }
    }
}
