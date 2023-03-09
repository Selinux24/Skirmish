using System;
using System.Collections.Generic;
using System.Reflection;

namespace Engine.Physics.GJKEPA
{
    public class ShapeAttribute : Attribute
    {
        public string Name { get; private set; }
        public ShapeAttribute(string name) => Name = name;

        public static IEnumerable<Type> GetAllTypes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ShapeAttribute), true).Length > 0)
                    yield return type;
            }
        }
    }
}
