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
            //TestDeserialize(); 

            //SimpleClass sc = new SimpleClass() { name = "Fredrick", age = 23, temperature = 36.3476437f };
            //string json = Jsonizer.Ser(sc);
            //Console.WriteLine(json);

            ClassWithClassReference cr = new ClassWithClassReference() { name = "Jalopnik", age = 43, isMale = false, pet = new Animal() { name = "Tortuga", ferocity = Ferocity.Docile }, temperature = 98.6f };
            string js = Jsonizer.Ser(cr);
            Console.WriteLine(js);

            var sc = Jsonizer.Deserialize<ClassWithClassReference>(js);
            if (sc != null)
                Console.WriteLine(sc.ToString());

            //TestJsonParser(@"..\..\..\TestClass.json");
            //TestJsonParser(@"..\..\..\random.json");
            //TestJsonParser(@"..\..\..\largeArray.json");

            //var parser = JSONParser.Parse("{\"Fred\" : \"Ted}");
            //Console.WriteLine(parser.Tokenizer.AllErrors);
            //Console.WriteLine(parser.AllErrors);

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

            var sc = Jsonizer.Deserialize<ClassWithClassReference>(json);
            if (sc != null)
                Console.WriteLine(sc.ToString());
        }

        private static void TestJsonParser(string filename)
        {
            var jparser = JSONParser.ParseFile(filename);

            //Show any Tokenizer errors
            if (!jparser.Tokenizer.Successful)
            {
                Console.WriteLine("====Tokenizer Errors====");
                foreach (var err in jparser.Tokenizer.Errors)
                {
                    Console.WriteLine(err);
                }
            }
            else if (jparser.Successful)
            {
                Console.WriteLine("====Parser Errors====");
                foreach (var err in jparser.Errors)
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
