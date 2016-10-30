using System;
using System.Reflection;

namespace DiscoreTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Assembly asm = Assembly.GetEntryAssembly();
            foreach (TypeInfo ti in asm.DefinedTypes)
            {
                Attribute testClassAttr = ti.GetCustomAttribute(typeof(TestClassAttribute));
                if (testClassAttr != null)
                {
                    foreach (MethodInfo mi in ti.DeclaredMethods)
                    {
                        Attribute testMethodAttr = mi.GetCustomAttribute(typeof(TestMethodAttribute));
                        if (testMethodAttr != null)
                        {
                            if (mi.IsStatic && mi.GetParameters().Length == 0)
                            {
                                try
                                {
                                    mi.Invoke(null, null);
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine($"[{ti.Name}.{mi.Name}] Pass");
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"[{ti.Name}.{mi.Name}] Fail: {ex.InnerException.Message}");
                                }
                            }
                            else
                                throw new Exception($"Invalid TestMethod: {mi}");
                        }
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nDone.");
            Console.ReadKey();
        }
    }
}
