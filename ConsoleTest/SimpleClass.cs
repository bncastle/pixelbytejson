﻿using System;
using System.Collections.Generic;

namespace Pixelbyte.JsonUnity
{
    internal class SimpleClass
    {
        public string name = string.Empty;
        public int age = 0;
        [JsonMaxDecimalPlaces(2)]
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

    public enum Ferocity { Docile, Fierce, Afraid}
    public struct V3 { public float x, y, z; }

    internal class ClassWithClassReference : IDeserializationCallbacks
    {
        public string name = string.Empty;
        public int age = 0;
        public DateTime birthday = new DateTime(1990, 09, 09);
        public float temperature = 0;
        public bool isMale = false;
        public Animal pet = null;
        public List<string> stuffs = new List<string>() {"fredro", "norbo", "rendro"};

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