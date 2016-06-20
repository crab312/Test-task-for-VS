using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestTaskForVS;
using ExpressionParser;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestNormalExpressions()
        {
            /* Simple expressions */
            TestNumExpression("1 + 2 = 0", 3);
            TestNumExpression("1 - 2 = 0", -1);
            TestNumExpression("-1 - 2 = 0", -3);
            TestNumExpression("-5000 + 1900 + 2100 = 0", -1000);
            TestNumExpression("1-2-3 = 0", -4);
            TestNumExpression("-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1-1=2+2+2+2+2+2+2+2+2+2+2+2", -49);


            /* Some bracketed expressions */
            TestNumExpression("-(1-1-1) = 0", 1);
            TestNumExpression("10 - (2+1) = 0", 7);
            TestNumExpression("10 - (2-1) = 0", 9);
            TestNumExpression("10 - (2-1+5) = 0", 4);
            TestNumExpression("-(-(-(-1))) = 0", 1);

            /* Test floating point */
            TestNumExpression("10.10000=1.02", 9.08);
            TestNumExpression("0.01=0", 0.01);

            /* Tests with 2 variables */
            TestExpressionXY("x+1=0", x: 1, y: 0, expectedResult: 2);
            TestExpressionXY("x+1=x-1", x: 1, y: 0, expectedResult: 2);
            TestExpressionXY("x+1 -(-y) = 10x", x: 100, y: 200, expectedResult: -699);
            TestExpressionXY("-x^2-2xy+y^3=y-10x", 1, 2, 11);

            /* Test polynom standartization */
            CheckPolynom("x + 2.1x^2 = 100x - 4x^2", "6.1x^2 - 99x = 0");
            CheckPolynom("10 - x^3 + 8x^4 = 10x - 1 + 5x^3", "8x^4 - 6x^3 - 10x + 11 = 0");
        }


        [TestMethod]
        public void TestBracketedExpressions()
        {
            CheckPolynom("(x+y)(x-y)=0", "x^2 - y^2 = 0");
            CheckPolynom("(x+y)(x+y)=0", "x^2 + y^2 + 2xy = 0");
            CheckPolynom("(x-y)(x-y)=0", "x^2 + y^2 - 2xy = 0");

            TestExpressionXY("(x+y)(x+y)(x+y)=0", 2, 3, 125);
            TestExpressionXY("(x+y)(x-y)(x+y)(x-y)(x+y)(x-y)(x+y)(x-y)(x+y)(x-y)(x+y)(x-y)=0", 2,3, 15625);
        }

        [TestMethod]
        public void TestWeirdExpressions()
        {
            /* Test syntax stuff */
            CheckPolynom("- x =     + x ", "-2x=0");
            
            /* Test unexpected stuff */
            CheckPolynom("0x = 0", "0=0");
            CheckPolynom("0 = x1", "-x=0");
            CheckPolynom("x12 = 0", "12x=0");

            CheckPolynom("0=2x^2+4y^100 - 0x", "-4y^100-2x^2=0");

            /* Test errors handling */
            TestIncorrectExpression("==");

        }

        [TestMethod]
        public void TestWeirdVariableName()
        {
            CheckPolynom("xxx=-xxx", "2x^3=0");

            TestIncorrectExpression("X=0");
        }

        [TestMethod]
        public void TestNumExpression(string expr, double expectedResult)
        {
            var o = new Expression(expr);
            var varValues = new List<Polynomial.VarValue>();
            var polynom = o.Process();
            var calcRes = polynom.Calculate(varValues);
            Assert.AreEqual(expectedResult, calcRes);
        }

        [TestMethod]
        public void TestExpressionXY(string expr, double x, double y, double expectedResult)
        {
            var o = new Expression(expr);
            var varValues = new List<Polynomial.VarValue>();
            varValues.Add(new Polynomial.VarValue { varName = "x", value = x});
            varValues.Add(new Polynomial.VarValue { varName = "y", value = y});
            var polynom = o.Process();
            var calcRes = polynom.Calculate(varValues);
            Assert.AreEqual(expectedResult, calcRes);
        }

        [TestMethod]
        public void CheckPolynom(string expr, string expectedResult)
        {
            var exprObj = new Expression(expr);
            var res = exprObj.Process().ToStandardForm();
            res = res.Replace(" ", "");
            expectedResult = expectedResult.Replace(" ","");
            Assert.AreEqual(expectedResult, res);
        }

        [TestMethod]
        public void TestIncorrectExpression(string expr)
        {
            try
            {
                var exprObj = new Expression(expr);
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }
    }
}
