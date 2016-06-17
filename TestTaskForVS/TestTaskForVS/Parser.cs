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


        public Tree.Item Parse(string expression)
        {
            this.expression = expression;
            pos = 0;

            Tree.BinaryOperation ti = new Tree.BinaryOperation();
            ti.a = ReadExpression(UnaryAllowed: true);
            if (!HasChar() || GetChar() != '=')
            {
                throw new ArgumentException("Expected '='");
            }
            NextChar();
            ti.op = Operation.equals;
            ti.b = ReadExpression(UnaryAllowed: true);
            return ti;
        }

        Tree.Item ReadExpression(bool UnaryAllowed = false)
        {
            Tree.Item leftOperand = null;
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
                            leftOperand = new Tree.UnaryOperation()
                            {
                                op = ch == '+' ? Operation.plus : Operation.minus,
                                a = ReadOperand(),
                            };
                        }
                        else
                        {
                            throw new ArgumentException(String.Format("Char '{0}' not expected. Expected: operand.", ch));
                        }
                    }
                    else
                    {
                        NextChar();
                        return new Tree.BinaryOperation()
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
                        throw new ArgumentException(String.Format("Char '{0}' not expected. Expression must appear before '{0}'", ch));
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
                        throw new ArgumentException("Expected one of {'+','-','='})");
                    }
                }
            };

            if (leftOperand == null)
            {
                throw new ArgumentException("Expression expected. Found end of string.");
            }

            return leftOperand;
        }

        Tree.Item ReadOperand()
        {
            Tree.Item curOperand = null;
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
                        throw new ArgumentException(String.Format("Char '{0}' not expected. Expected: OperandMember or bracketed expression.", ch));
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
                        throw new ArgumentException("Expected end bracket ')'");
                    }
                    if (curOperand != null)
                    {
                        curOperand = new Tree.BinaryOperation()
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
                        curOperand = new Tree.BinaryOperation()
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
                throw new ArgumentException("Operand expected. Found end of string.");
            }

            return curOperand;
        }

        Tree.Item ReadOperandMember()
        {
            Tree.Term operandMember = null;
            string curVarName = null;

            Action<int> AddCurrVar = (int pow) =>
            {
                if (operandMember == null)
                {
                    operandMember = new Tree.Term { term = new Polynomial.Term { coeff = 1 } };
                }
                operandMember.term.AddVar(curVarName, pow);
                curVarName = null;
            };

            Action<double> AddCoeff = (double coeff) =>
            {
                if (operandMember == null)
                {
                    operandMember = new Tree.Term { term = new Polynomial.Term { coeff = 1 } };
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
                        throw new ArgumentException(String.Format("Char '{0}' not expected. Expected: variable or number", ch));
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
                        throw new ArgumentException("Invalid variable name. Allowed only lower case letters from 'a' to 'z'");
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
                throw new ArgumentException("OperandMember expected. Found end of string.");
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
                throw new ArgumentException("Integer expected. Invalid integer: " + numStr);
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
                throw new ArgumentException("Floating-point number expected. Found invalid number: " + numStr);
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


}


