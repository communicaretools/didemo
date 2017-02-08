using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Registering...");
            
            var container = new Container();
            container.RegisterTransient<A>();
            container.RegisterTransient<B>();
            container.RegisterSingleton(new C());
            
            Console.WriteLine("Getting a type...");
            var b = container.Get<B>();
            b.SayHello();
            Console.WriteLine("done.");
        }
    }

    public class A {}

    public class B
    {
        private readonly A a;
        private readonly C c;

        public B(A a, C c)
        {
            this.a = a;
            this.c = c;
        }

        public void SayHello()
        {
            Console.WriteLine("B is up and running; a={0}, c={1}", a, c);
        }
    }

    public class C {}

    public class Container
    {
        private readonly ISet<Type> transients = new HashSet<Type>();
        private readonly IDictionary<Type, object> singletons = new Dictionary<Type, object>();

        public void RegisterTransient<T>()
        {
            transients.Add(typeof(T));
        }

        public void RegisterSingleton<T>(T instance)
        {
            singletons[typeof(T)] = instance;
        }

        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        public object Get(Type type)
        {
            if (singletons.ContainsKey(type))
            {
                return singletons[type];
            }
            if (!transients.Contains(type))
            {
                throw new InvalidOperationException("Type " + type.FullName + " is not registered");
            }
            return Construct(type);
        }

        public object Construct(Type type)
        {
            var ctor = type.GetConstructors().Single();
            var args = new List<object>();
            foreach (var parameter in ctor.GetParameters())
            {
                args.Add(Get(parameter.ParameterType));
            }
            return ctor.Invoke(args.ToArray());
        }
    }
}
