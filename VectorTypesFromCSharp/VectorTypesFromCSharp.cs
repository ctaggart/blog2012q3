using System;
namespace VectorTypesFromCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var v = new VectorTypes.Vector2D();
            v.X = 100;
            v.Y = 200;
            Console.WriteLine("X: {0}, Y: {1}", v.X, v.Y);
            Console.ReadKey(true);
        }
    }
}
