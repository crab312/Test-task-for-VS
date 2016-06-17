using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionParser
{
    public enum Operation { plus, minus, mult, divide, power, equals };
    public class Tree
    {
        public interface Item
        {
            Polynomial Process();
        }

        public class UnaryOperation : Item
        {
            public Operation op;
            public Item a;
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

        public class BinaryOperation : Item
        {
            public Operation op;
            public Item a;
            public Item b;

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

        public class Term : Item
        {
            public Polynomial.Term term;
            public Polynomial Process()
            {
                Polynomial pol = new Polynomial();
                pol.AddTerm(term);
                return pol;
            }
        }

        
    }


   
}
