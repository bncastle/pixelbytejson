using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pixelbyte.Json
{
    class Program
    {
        static void Main()
        {
            const string testList = "[{\"imageName\": null, \"itemName\": \"Chair\", \"description\": \"A Chair\", \"price\": 23}, {\"imageName\": null, \"itemName\": \"Bowling Ball\", \"description\": \"A Bowling ball\", \"price\": 34}]";
            //TestJsonParser();
            //TestDeserialize(); 

            //SimpleClass sc = new SimpleClass() { name = "Fredrick", age = 23, temperature = 36.3476437f };
            //string json = JsonEncoder.Encode(sc);
            //Console.WriteLine(json);

            //string tst = @"{""name"" : ""Forodoro"", ""age"" : 23, ""temperature"": 2.356e-10 }";
            //var dec = JsonDecoder.Decode<SimpleClass>(tst);
            //Console.WriteLine(dec.ToString());

            //ClassWithClassReference cr = new ClassWithClassReference() { name = "Jalopnik", age = 43, isMale = false, pet = new Animal() { name = "Tortuga", ferocity = Ferocity.Docile }, temperature = 98.6f };
            //string js = Jsonizer.Serialize(cr);
            //Console.WriteLine(js);

            //var sc = Jsonizer.Deserialize<ClassWithClassReference>(js);
            //if (sc != null)
            //    Console.WriteLine(sc.ToString());

            JsonEncoder.SetTypeEncoder(typeof(Bounds), (obj, builder) =>
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

            JsonDecoder.SetDecoder(typeof(Bounds), (type, jsonObj) =>
            {
                if (jsonObj == null) throw new ArgumentNullException("jsonObj");
                return new Bounds(Convert.ToSingle(jsonObj["x"]), Convert.ToSingle(jsonObj["y"]), Convert.ToSingle(jsonObj["width"]), Convert.ToSingle(jsonObj["height"]));
            });

            //List<Bounds> bb = new List<Bounds>() { Bounds.Rnd(), Bounds.Rnd(), Bounds.Rnd(), Bounds.Rnd() };
            //string boundsJson = JsonEncoder.Encode(bb, true);
            //Console.WriteLine(boundsJson);
            //var decodedBounds = JsonDecoder.Decode<List<Bounds>>(boundsJson);

            //var decodedData = JsonDecoder.Decode<List<ItemData>>(testList);

            //Bounds b = new Bounds(23.5f, 54.0f, 120.0f, 64.1f);
            //string bounds = JsonEncoder.Encode(b);
            //Console.WriteLine(bounds);

            //TestArray ta = new TestArray()
            //{
            //    theArray = new uint[] { 19078, 1934, 2067, 6590 }
            //};

            //var json = JsonEncoder.Encode(ta);
            //Console.WriteLine(json);

            //TestArray dec = JsonDecoder.Decode<TestArray>(json);
            //Console.WriteLine(dec.theArray[0]);

            //TestList tl = new TestList()
            //{
            //    stuff = new List<int>() { 67, 89, 3278 }
            //};
            //json = JsonEncoder.Encode(tl);
            //Console.WriteLine(json);

            //TestList decodedList = JsonDecoder.Decode<TestList>(json);
            //Console.WriteLine(decodedList.stuff[0]);

            var l = new List<ItemData>()
            {
                new ItemData() { description = "descript", id = "itemData", sprite = "theIcon", uses = 100, weight = 24, type = ItemType.Meleee },
                new ItemData() { description = "descript", id = "itemData", sprite = "theIcon", uses = 100, weight = 24, type = ItemType.Meleee },
                new ItemData() { description = "descript", id = "itemData", sprite = "theIcon", uses = 100, weight = 24, type = ItemType.Meleee },
                new ItemData() { description = "descript", id = "itemData", sprite = "theIcon", uses = 100, weight = 24, type = ItemType.Meleee }};

            string json = JsonEncoder.Encode(l, true);
            Console.WriteLine(json);

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
            var jparser = JsonParser.ParseFile(@"..\..\..\SimpleClass.json");
            string filename = @"..\..\..\ClassWithClassReference.json";
            if (!File.Exists(filename))
                throw new FileNotFoundException();
            //Pull in the json text
            string json;
            using (var sr = new StreamReader(filename))
            {
                json = sr.ReadToEnd();
            }

            var sc = JsonDecoder.Decode<ClassWithClassReference>(json);
            if (sc != null)
                Console.WriteLine(sc.ToString());
        }

        private static void TestJsonParser(string filename)
        {
            Console.WriteLine("Parsing {0}", filename);

            var jparser = JsonParser.ParseFile(filename);

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
