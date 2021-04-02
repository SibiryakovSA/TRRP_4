using System;
using System.IO;
using Avalonia;

namespace ClientAvalonia
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 1)
                if (File.Exists(args[0]))
                {
                    var envVar = Environment.GetEnvironmentVariable("SERVER_HOST");
                    if (string.IsNullOrEmpty(envVar))
                    {
                        Console.WriteLine("Переменная окружения SERVER_HOST не задана");
                        return;
                    }

                    var res = FindFaces.Find(args[0], envVar);
                    if (res)
                    {
                        var path = Path.GetFullPath("result");
                        Console.WriteLine("Изображения успешно вырезаны, они находятся по адресу " + path);
                    }
                    else
                        Console.WriteLine("Произошла ошибка при поиске и обрезке лиц");
                }
                else
                    Console.WriteLine("По адресу параметра нет файла");
            else if (args.Length > 1)
            {
                Console.WriteLine("Приложение принимает только один параметр - адрес изображения");
                Console.WriteLine("Адрес сервера задается переменной окружения SERVER_HOST");
            }
            else
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}