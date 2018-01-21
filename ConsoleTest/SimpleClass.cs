namespace Pixelbyte.JsonUnity
{
    internal class SimpleClass
    {
        public string name = string.Empty;
        public int age = 0;
        [DecimalPlaces(2)]
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
        public override string ToString()
        {
            return string.Format("Animal: {0} Extinction: {1}", name, percentExtinct);
        }
    }
    internal class ClassWithClassReference : IDeserializationCallbacks
    {
        public string name = string.Empty;
        public int age = 0;
        public float temperature = 0;
        public bool isMale = false;
        public Animal pet = null;

        public void OnDeserialized()
        {
            System.Console.WriteLine("Deserialized!");
        }

        public override string ToString()
        {
            return string.Format("Name: {0} Age: {1} Temp: {2} Male: {3} Pet: {4}", name, age, temperature, isMale, pet.ToString());
        }
    }
}