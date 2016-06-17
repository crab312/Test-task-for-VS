﻿using System;
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
            TestNumExpression("1-2-3 = 0", -4);

            /* Some bracketed expressions */
            TestNumExpression("-(1-1-1) = 0", 1);
            TestNumExpression("10 - (2+1) = 0", 7);
            TestNumExpression("10 - (2-1) = 0", 9);
            TestNumExpression("10 - (2-1+5) = 0", 4);
            TestNumExpression("-(-(-(-1))) = 0", 1);

            /* Test floating point */
            TestNumExpression("10.10000=1.02", 9.08);

            /* Tests with 2 variables */
            TestExpressionXY("x+1=0", x: 1, y: 0, expectedResult: 2);
            TestExpressionXY("x+1=x-1", x: 1, y: 0, expectedResult: 2);
            TestExpressionXY("x+1 -(-y) = 10x", x: 100, y: 200, expectedResult: -699);
            TestExpressionXY("-x^2-2xy+y^3=y-10x", 1, 2, 11);

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
