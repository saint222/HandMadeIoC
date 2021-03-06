﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HandMadeIoC
{
    public class Container
    {
        readonly Dictionary<Type, Func<object>> registrations = new Dictionary<Type, Func<object>>();

        public void Register<TService, TImpl>() where TImpl : TService // ограничиваем generic
        {
            this.registrations.Add(typeof(TService), () => this.GetInstance(typeof(TImpl)));
        }

        public object GetInstance(Type serviceType)
        {
            Func<object> creator;
            if (this.registrations.TryGetValue(serviceType, out creator))
                return creator();
            else if (!serviceType.IsAbstract)
                return this.CreateInstance(serviceType);
            else 
                throw new InvalidOperationException("No registration for " + serviceType);
        }
        
        private object CreateInstance(Type implementationType)
        {
            var ctor = GetConstructorWithMaxParametrs(implementationType);
            if (implementationType.IsPrimitive) // проверяем на примитивность аргумента
            {
                throw new ArgumentException("The implementation type is primitive...");  
            }
            var parameterTypes = ctor.GetParameters().Select(p => p.ParameterType);
            var dependencies = parameterTypes.Select(t => this.GetInstance(t)).ToArray();            
            return ctor.Invoke(dependencies); // вместо return Activator.CreateInstance(implementationType, dependencies);
        }
        // поиск конструктора с максимальным кол-ом возможных аргументов
        private ConstructorInfo GetConstructorWithMaxParametrs(Type implementationType)
        {
            var ctor = implementationType.GetConstructors();
            var ctorWithMaxParametrs = ctor.OrderByDescending(r => r.GetParameters().Length).FirstOrDefault();
            return ctorWithMaxParametrs;
        }        
    }
    public interface IEngine
    {
        int GetPower();
    }

    public class Engine : IEngine
    {
        public int GetPower()
        {
            return 106;
        }
    }
    public class Car
    {
        private readonly IEngine _engine;
        public string Model { get; set; }
        public string Type { get; set; }

        
        public Car(IEngine engine)
        {
            _engine = engine;
        }

        public void GetDescription()
        {
            Console.WriteLine($"This car is {Model} {Type}. The engine's horsepower of this car is { _engine.GetPower()} hp.");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
            container.Register<IEngine, Engine>();            
            var car = container.GetInstance(typeof(Car)) as Car; // т.к. не generic, возвращает object
            car.Model = "Renault";
            car.Type = "Megane";
            car.GetDescription();
            Console.ReadLine();
        }
    }
}
