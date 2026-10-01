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
        public ExposedServiceAttribute(Lifetime lifetime=Lifetime.Transient, params Type[] types)
        {
            Lifetime = lifetime;
            Types = types;
        }

        public Lifetime Lifetime { get; set; }
        public bool IsAutoInitialize { get; set; }
        public Type[] Types { get; set; }
       
    }
}
