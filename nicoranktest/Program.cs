using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NUnit.Framework;
using System.Diagnostics;

namespace nicoranktest
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Assembly asm = Assembly.GetEntryAssembly();
            foreach (object test in GetTests(asm))
            {
                try
                {
                    SetUp(test);
                    foreach (MethodInfo test_method in GetTestMethods(test))
                    {
                        test_method.Invoke(test, new object[0]);
                    }
                }
                finally
                {
                    TearDown(test);
                }
            }
        }

        private static IEnumerable<object> GetTests(Assembly asm)
        {
            foreach (Type testType in asm.GetTypes())
            {
                if (!testType.IsClass)
                {
                    continue;
                }
                if (testType.IsAbstract)
                {
                    continue;
                }
                if (testType.GetCustomAttributes(typeof(TestFixtureAttribute), false).Length >= 1)
                {
                    ConstructorInfo ctor = testType.GetConstructor(new Type[0]);
                    object test = ctor.Invoke(new object[0]);
                    yield return test;
                }
            }
        }

        private static IEnumerable<MethodInfo> GetTestMethods(object test)
        {
            foreach (MethodInfo test_method in test.GetType().GetMethods())
            {
                if (test_method.GetCustomAttributes(typeof(TestAttribute), false).Length >= 1 &&
                    test_method.GetCustomAttributes(typeof(RunUnitTestAttribute), false).Length >= 1)
                {
                    yield return test_method;
                }
            }
        }

        private static void SetUp(object test)
        {
            foreach (MethodInfo method in test.GetType().GetMethods())
            {
                if (method.GetCustomAttributes(typeof(SetUpAttribute), false).Length >= 1)
                {
                    method.Invoke(test, new object[0]);
                    return;
                }
            }
        }

        private static void TearDown(object test)
        {
            foreach (MethodInfo method in test.GetType().GetMethods())
            {
                if (method.GetCustomAttributes(typeof(TearDownAttribute), false).Length >= 1)
                {
                    method.Invoke(test, new object[0]);
                    return;
                }
            }
        }
    }
}
