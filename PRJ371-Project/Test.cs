using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRJ371_Project
{
    public class Test
    {
        public string sayHello(int value)
        {
            string sentence = "Hello to me!";
            if (value == 1)
            {
                Console.WriteLine(sentence);
            }
            return sentence;
        }
    }
}
