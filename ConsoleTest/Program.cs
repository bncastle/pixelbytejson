using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    class Program
    {
        static void Main()
        {
            //TestJsonParser();
            TestDeserialize();

            //Console.WriteLine(Environment.CurrentDirectory);
        }

        private static void TestDeserialize()
        {
            var jparser = JSONParser.ParseFile(@"..\..\..\SimpleClass.json");
            string filename = @"..\..\..\ClassWithClassReference.json";
            if (!File.Exists(filename))
                throw new FileNotFoundException();
            //Pull in the json text
            string json;
            using (var sr = new StreamReader(filename))
            {
                json = sr.ReadToEnd();
            }

            var sc = Seroz.Deserialize<ClassWithClassReference>(json);
            if (sc != null)
                Console.WriteLine(sc.ToString());
        }

        private static void TestJsonParser()
        {
            var jparser = JSONParser.ParseFile(@"..\..\..\TestClass.json");

            //Show any Tokenizer errors
            if (!jparser.Tokenizer.Successful)
            {
                foreach (var err in jparser.Tokenizer.Errors)
                {
                    Console.WriteLine(err);
                }
            }
            else if (jparser.Successful)
            {
                foreach (var err in jparser.ParserErrors)
                {
                    Console.WriteLine(err);
                }
            }
            else
            {
                Console.WriteLine("Success");
            }
        }
    }
}
