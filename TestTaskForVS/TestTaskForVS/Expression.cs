using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExpressionParser;

namespace TestTaskForVS
{
    public class Expression
    {
        Tree.Item exprTree;

        void BuildExpressionTree(string stringExpression)
        {
            var parser = new Parser();
            exprTree = parser.Parse(stringExpression);

            var temp = 0;
        }

        public Expression(string stringExpression)
        {
            BuildExpressionTree(stringExpression);
        }

        public Polynomial Process()
        {
            return exprTree.Process();
        }

    }
}
