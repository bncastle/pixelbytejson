namespace Pixelbyte.JsonUnity
{
    internal class SimpleClass
    {
        public string name;
        public int age;
        public float temperature;
        public bool isMale;

        public override string ToString()
        {
            return string.Format("Name: {0} Age: {1} Temp: {2} Male: {3}", name, age, temperature, isMale);
        }
    }

    internal class Animal
    {
        public string name;
        public float percentExtinct;
        public override string ToString()
        {
            return string.Format("Animal: {0} Extinction: {1}", name, percentExtinct);
        }
    }
    internal class LessSimpleClass
    {
        public string name;
        public int age;
        public float temperature;
        public bool isMale;
        public Animal pet;

        public override string ToString()
        {
            return string.Format("Name: {0} Age: {1} Temp: {2} Male: {3} Pet: {4}", name, age, temperature, isMale, pet.ToString());
        }
    }
}