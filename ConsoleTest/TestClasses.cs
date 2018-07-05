using System;
using System.Collections.Generic;

namespace Pixelbyte.Json
{
    internal struct Bounds
    {
        private float x;
        private float y;
        private float width;
        private float height;
        public float Width { get { return width; } }
        public float Height { get { return height; } }
        public float X { get { return x; } }
        public float Y { get { return y; } }

        public Bounds(float xx, float yy, float w, float h)
        {
            x = xx;
            y = yy;
            width = w;
            height = h;
        }

        static Random rng;
        public static Bounds Rnd()
        {
            if (rng == null)
                rng = new Random();

            return new Bounds((float)rng.NextDouble() * 100, (float)rng.NextDouble() * 100, (float)rng.NextDouble() * 100, (float)rng.NextDouble() * 100);
        }
    }

    internal class SimpleClass
    {
        public string name = string.Empty;
        public int age = 0;
        //[JsonMaxDecimalPlaces(2)]
        public float temperature = 0;
        public bool isMale = false;

        public override string ToString()
        {
            return string.Format("Name: {0} Age: {1} Temp: {2} Male: {3}", name, age, temperature, isMale);
        }
    }

    internal class Animal
    {
        public string name = string.Empty;
        public float percentExtinct = 0;
        public Ferocity ferocity = Ferocity.Afraid;
        public V3 speed = new V3() { x = 34.4f, y = 98.6f, z = -0.978f };

        public override string ToString()
        {
            return string.Format("Animal: {0} Extinction: {1} Ferocity: {2}", name, percentExtinct, ferocity.ToString());
        }
    }

    public enum Ferocity { Docile, Fierce, Afraid }
    public struct V3 { public float x, y, z; }

    internal class ClassWithClassReference : IJsonDecodeCallbacks
    {
        public string name = string.Empty;
        public int age = 0;
        public DateTime birthday = new DateTime(1990, 09, 09);
        public float temperature = 0;
        public bool isMale = false;
        public Animal pet = null;
        public Dictionary<string, int> numbers = new Dictionary<string, int>() { { "fiver", 5 }, { "sixer", 6 }, { "severner", 7 } };
        public List<string> aList = new List<string>() { "fredro", "norbo", "rendro" };
        public string[] anArray = new string[] { "fredro", "norbo", "rendro" };
        private string shouldNotBeSeen = "nope";

        [JsonInclude]
        private string doThisOne = "It is done!";

        public void OnJsonDecoded()
        {
            System.Console.WriteLine("Decoded a ClassWithClassReference");
            Console.WriteLine(shouldNotBeSeen);
            Console.WriteLine(doThisOne);
        }

        public override string ToString()
        {
            return string.Format("Name: {0} Age: {1} Temp: {2} Male: {3} Pet: {4}", name, age, temperature, isMale, pet.ToString());
        }
    }

    [Serializable]
    public class TestArray
    {
        public uint[] theArray;
    }

    [Serializable]
    public class TestList
    {
        public List<int> stuff;
    }

    public enum ItemType : ushort
    { weapon, Meleee }

    [Serializable]
    public class ItemData
    {
        #region Events
        [field: NonSerialized]
        public event Action<string> DescriptionChanged;
        [field: NonSerialized]
        public event Action<string> TitleChanged;
        #endregion

        /// <summary>
        /// The name of the object
        /// </summary>
        public string id;

        public string description;
        public string sprite;

        //Type of item it is
        public ItemType type;

        //for a ranged or wand
        public int uses = 0;
        public int weight = 0;

        //Category in which this item belongs
        public ItemType Category => (ItemType)((ushort)type & 0xFF00);

        bool inInventory;

        [JsonInclude]
        bool equipped;
    }
}