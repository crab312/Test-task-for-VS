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
        public void TestMethod1()
        {
            /* Simple expressions */
            TestNumExpression("1 + 2 = 0", 3);
            TestNumExpression("1 - 2 = 0", -1);
            TestNumExpression("-1 - 2 = 0", -3);
            TestNumExpression("-5000 + 1900 + 2100 = 0", -1000);
            TestNumExpression("1-2-3 = 0", 4);

            /* Some bracketed expressions */
            TestNumExpression("-(1-1-1) = 0", 1);
            TestNumExpression("10 - (2+1) = 0", 7);
            TestNumExpression("10 - (2-1) = 0", 9);
            TestNumExpression("10 - (2-1+5) = 0", 3);
            TestNumExpression("-(-(-(-1))) = 0", 1);

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
    }
}
