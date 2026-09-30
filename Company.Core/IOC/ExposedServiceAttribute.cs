using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Core.IOC
{
    public enum Lifetime
    {
        Singleton,
        Transient,
    }
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false)]
    public class ExposedServiceAttribute:Attribute
    {
        public ExposedServiceAttribute(Lifetime lifetime, bool autoInitialize, Type[] types)
        {
            Lifetime = lifetime;
            AutoInitialize = autoInitialize;
            Types = types;
        }

        public Lifetime Lifetime { get; set; }
        public bool AutoInitialize { get; set; }
        public Type[] Types { get; set; }
       
    }
}
