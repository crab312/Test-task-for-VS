using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionParser
{
    public class Parser
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
            pos = 0;

            TreeBinaryOperation ti = new TreeBinaryOperation();
            ti.a = ReadExpression(UnaryAllowed: true);
            if (!HasChar() || GetChar() != '=')
            {
                throw new Exception("Expected '='");
            }
            NextChar();
            ti.op = Operation.equals;
            ti.b = ReadExpression(UnaryAllowed: true);
            return ti;
        }

        TreeItem ReadExpression(bool UnaryAllowed = false)
        {
            TreeItem leftOperand = null;
            while (HasChar())
            {
                var ch = GetChar();
                if (Char.IsWhiteSpace(ch))
                {
                    NextChar();
                }
                else if (ch == '+' || ch == '-')
                {
                    if (leftOperand == null)
                    {
                        if (UnaryAllowed)
                        {
                            NextChar();
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
                        NextChar();
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

            if (leftOperand == null)
            {
                throw new Exception("Expression expected. Found end of string.");
            }

            return leftOperand;
        }

        TreeItem ReadOperand()
        {
            TreeItem curOperand = null;
            while (HasChar())
            {
                var ch = GetChar();
                if (Char.IsWhiteSpace(ch))
                {
                    NextChar();
                }
                else if (ch == '+' || ch == '-' || ch == '=' || ch == ')')
                {
                    if (curOperand == null)
                    {
                        throw new Exception(String.Format("Char '{0}' not expected. Expected: OperandMember or bracketed expression.", ch));
                    }
                    else
                    {
                        return curOperand;
                    }
                }
                else if (ch == '(')
                {
                    NextChar();
                    var exprRes = ReadExpression(UnaryAllowed: true);
                    if (HasChar() && GetChar() != ')')
                    {
                        throw new Exception("Expected end bracket ')'");
                    }
                    if (curOperand != null)
                    {
                        curOperand = new TreeBinaryOperation()
                        {
                            a = curOperand,
                            b = exprRes,
                            op = Operation.mult,
                        };
                    }
                    else
                    {
                        curOperand = exprRes;
                    }
                    NextChar();
                }
                else
                {
                    if (curOperand == null)
                    {
                        curOperand = ReadOperandMember();
                    }
                    else
                    {
                        curOperand = new TreeBinaryOperation()
                        {
                            a = curOperand,
                            b = ReadOperandMember(),
                            op = Operation.mult,
                        };
                    }
                }
            };
            if (curOperand == null)
            {
                throw new Exception("Operand expected. Found end of string.");
            }

            return curOperand;
        }

        TreeItem ReadOperandMember()
        {
            TreeTerm operandMember = null;
            string curVarName = null;

            Action<int> AddCurrVar = (int pow) =>
            {
                if (operandMember == null)
                {
                    operandMember = new TreeTerm { term = new Polynomial.Term { coeff = 1 } };
                }
                operandMember.term.AddVar(curVarName, pow);
                curVarName = null;
            };

            Action<double> AddCoeff = (double coeff) =>
            {
                if (operandMember == null)
                {
                    operandMember = new TreeTerm { term = new Polynomial.Term { coeff = 1 } };
                }
                operandMember.term.coeff *= coeff;
            };

            while (HasChar())
            {
                var ch = GetChar();

                if (Char.IsWhiteSpace(ch))
                {
                    NextChar();
                }
                else if (ch == '+' || ch == '-' || ch == '=' || ch == '(' || ch == ')')
                {
                    if (operandMember == null)
                    {
                        throw new Exception(String.Format("Char '{0}' not expected. Expected: variable or number", ch));
                    }
                    else
                    {
                        return operandMember;
                    }
                }
                else if (Char.IsNumber(ch))
                {
                    var coeff = TryReadNumber();
                    AddCoeff(coeff);
                }
                else if (Char.IsLetter(ch))
                {
                    curVarName = ch.ToString();
                    if (!IsVariable(curVarName))
                    {
                        throw new Exception("Invalid variable name. Allowed only lower case letters from 'a' to 'z'");
                    }
                    NextChar();
                    int pow = 1;
                    if (HasChar() && GetChar() == '^')
                    {
                        NextChar();
                        pow = TryReadInteger();
                    }
                    AddCurrVar(pow);
                }
            }
            if (operandMember == null)
            {
                throw new Exception("OperandMember expected. Found end of string.");
            }
            return operandMember;
        }

        int TryReadInteger()
        {
            var numChars = new char[30];
            int numPos = 0;
            while (HasChar())
            {
                var ch = GetChar();
                if (Char.IsNumber(ch) || ch == '.')
                {
                    numChars[numPos++] = ch;
                }
                else
                {
                    break;
                }
                NextChar();
            }
            var numStr = new String(numChars, 0, numPos);
            int res = 0;
            if (Int32.TryParse(numStr, out res))
            {
                return res;
            }
            else
            {
                throw new Exception("Integer expected. Invalid integer: " + numStr);
            }
        }

        double TryReadNumber()
        {
            var numChars = new char[30];
            int numPos = 0;
            while (HasChar())
            {
                var ch = GetChar();
                if (Char.IsNumber(ch) || ch == '.')
                {
                    numChars[numPos++] = ch;
                }
                else
                {
                    break;
                }
                NextChar();
            }
            var numStr = new String(numChars, 0, numPos);
            double res = 0;
            if (double.TryParse(numStr, out res))
            {
                return res;
            }
            else
            {
                throw new Exception("Floating-point number expected. Found invalid number: " + numStr);
            }
        }

        bool IsVariable(string token)
        {
            return Polynomial.Term.IsCorrectVarName(token);
        }

        void NextChar()
        {
            pos++;
        }

        bool HasChar()
        {
            return pos < expression.Length;
        }

        char GetChar()
        {
            return expression[pos];
        }
    }


    public interface TreeItem
    {
        Polynomial Process();
    }

    public class TreeBinaryOperation : TreeItem
    {
        public Operation op;
        public TreeItem a;
        public TreeItem b;

        public Polynomial Process()
        {
            var pol1 = a.Process();
            var pol2 = b.Process();
            
            if (op == Operation.equals || op == Operation.minus)
            {
                pol2.InverseSigns();
            }

            pol1.AppendPolynomial(pol2);

            return pol1;
        }
    }

    public class TreeUnaryOperation : TreeItem
    {
        public Operation op;
        public TreeItem a;
        public Polynomial Process()
        {
            var pol = a.Process();
            if (op == Operation.minus)
            {
                pol.InverseSigns();
            }
            return pol;
        }
    }

    public class TreeTerm : TreeItem
    {
        public Polynomial.Term term;
        public Polynomial Process()
        {
            Polynomial pol = new Polynomial();
            pol.AddTerm(term);
            return pol;
        }
    }

    public enum Operation { plus, minus, mult, divide, power, equals };

}


