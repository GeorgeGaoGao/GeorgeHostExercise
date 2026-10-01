using Company.Core.IOC;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Company.Core.Extensions
{
    /// <summary>
    /// 依赖注入扩展类，可以在加载模块时，实例化 标注为ExposedServiceAttribute特性的类。
    /// </summary>
    public static class DependencyExtensionForExposedServiceAttribute
    {
        /// <summary>
        /// 找出assembly中符合要求的类。
        /// 打出某个Assembly中需要标注ExposedServiceAttribute特性的类。
        /// 判定条件：不为空，是类，不是抽象类，有指定的特性。
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static List<Type> GetTypesWithExposedServiceAttributeFromAssembly(Assembly assembly)
        {
            var result=assembly.GetTypes().Where(t=>t!=null&&t.IsClass&&
            !t.IsAbstract&&t.CustomAttributes.Any(a=>a.AttributeType==typeof(ExposedServiceAttribute))).ToList();
            return result;
        }


        /// <summary>
        /// 实例化assembly中找到的符合要求的类，是个扩展方法。
        /// </summary>
        /// <param name="containerRegistry"></param>
        /// <param name="assembly"></param>
        public static void RegisterTypesOfAssembly(this IContainerRegistry containerRegistry, Assembly assembly)
        {
            var list=GetTypesWithExposedServiceAttributeFromAssembly(assembly);
            foreach (var type in list)
            {
                RegisterTypesOfAssembly(containerRegistry, type);
            }
        }
        /// <summary>
        /// 扩展方法的同名重载方法，具体实现对类的实例化。
        /// 
        /// </summary>
        /// <param name="containerRegistry"></param>
        /// <param name="type"></param>
        private static void RegisterTypesOfAssembly(IContainerRegistry containerRegistry, Type type)
        {
            IEnumerable<ExposedServiceAttribute> list = GetExposedServiceAttributesFromType(type);
            foreach (var attribute in list)
            {
                if (attribute.Lifetime==Lifetime.Singleton)
                {
                    containerRegistry.RegisterSingleton(type);//特性中指明是单例，所以把该类注册为单例
                }

                foreach (var IType in attribute.Types)
                {
                    if (attribute.Lifetime==Lifetime.Singleton)
                    {
                        containerRegistry.RegisterSingleton(IType, type);//特性中指明是单例，把特性的types中的类也注册为单例。
                        //types中都是接口，type的单例，作为该接口的单例实现。
                        //IContainerRegistry.RegisterSingleton(Type from, Type to) 的核心功能是：
                        //将一个服务接口（from）映射到一个具体的实现类（to），并确保在整个应用程序的生命周期内，该服务只被创建一次（单例），
                        //所有对该服务的请求都会返回同一个实例。
                    }
                    else if (attribute.Lifetime == Lifetime.Transient)
                    {
                        containerRegistry.Register(IType,type);//否则把特性的types中的类注册为多例。
                    }
                }
            }
        }
        /// <summary>
        /// 找出一个类中的指定特性的集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<ExposedServiceAttribute> GetExposedServiceAttributesFromType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttributes<ExposedServiceAttribute>();
        }

        //初始化程序集中所有标注为ExposedServiceAttribute特性的类，要求单例且自动加载AutoInitialize=true

        public static void InitializeTypesOfAssembly(this IContainerProvider containerProvider, Assembly assembly)
        {
            var list=GetTypesWithExposedServiceAttributeFromAssembly(assembly);
            foreach (var type in list)
            {
                InitializeTypesOfAssembly(containerProvider, type);
            }
        }

        private static void InitializeTypesOfAssembly(IContainerProvider containerProvider, Type type)
        {
            var list = GetExposedServiceAttributesFromType(type);
            foreach (var attribute in list) 
            {
                if (attribute.Lifetime==Lifetime.Singleton&&attribute.IsAutoInitialize)
                {
                    containerProvider.Resolve(type);
                }
            }
        }
    }
}
