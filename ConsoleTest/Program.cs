﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            //ClassWithClassReference cr = new ClassWithClassReference() { name = "Jalopnik", age = 43, isMale = false, pet = new Animal() { name = "Tortuga", ferocity = Ferocity.Docile }, temperature = 98.6f };
            //string js = Jsonizer.Serialize(cr);
            //Console.WriteLine(js);

            //var sc = Jsonizer.Deserialize<ClassWithClassReference>(js);
            //if (sc != null)
            //    Console.WriteLine(sc.ToString());

            JSONEncoder.SetTypeEncoder(typeof(Bounds), (obj, builder) =>
            {
                var type = obj.GetType();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                builder.BeginObject();
                foreach (var field in fields)
                {
                    builder.EncodePair(field.Name, field.GetValue(obj));
                    builder.Comma();
                    builder.LineBreak();
                }
                builder.EndObject();
            });

            Bounds b = new Bounds(23.5f, 54.0f, 120.0f, 64.1f);
            string bounds = JSONEncoder.Encode(b);
            Console.WriteLine(bounds);

            //TestJsonParser(@"..\..\..\TestJsonFiles\TestClass.json");
            //TestJsonParser(@"..\..\..\TestJsonFiles\ClassWithClassReference.json");
            //TestJsonParser(@"..\..\..\TestJsonFiles\random.json");
            //TestJsonParser(@"..\..\..\TestJsonFiles\largeArray.json");

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

            var sc = JSONDecoder.Decode<ClassWithClassReference>(json);
            if (sc != null)
                Console.WriteLine(sc.ToString());
        }

        private static void TestJsonParser(string filename)
        {
            Console.WriteLine("Parsing {0}", filename);

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
            else if (!jparser.Successful)
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
