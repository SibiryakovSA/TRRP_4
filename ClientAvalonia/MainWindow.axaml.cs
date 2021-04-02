using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ClientAvalonia
{
    public class MainWindow : Window
    {
        private string? imagePath = null;
        public string? ImagePath
        {
            get => imagePath;
            set
            {
                imagePath = value;
                ImagePathChanged();
            }
        }

        private string? textBoxText = null;
        
        public MainWindow()
        {
            InitializeComponent();
            #if DEBUG
                this.AttachDevTools();
            #endif
            var envVar = Environment.GetEnvironmentVariable("SERVER_HOST");
            if (envVar != null)
            {
                this.Find<TextBox>("TextBox").Text = envVar;
                textBoxText = envVar;
            }
        }
        
        public static void ExecuteCommand(string command)
        {
            Process proc = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = @"/bin/bash",
                    Arguments = "-c \" " + command + " \"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };

            proc.Start ();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        void ImagePathChanged()
        {
            var res = FindFaces.Find(ImagePath, textBoxText);
            if (res)
            {
                var pathToResult = Path.GetFullPath("result");
                ExecuteCommand($"open \"{pathToResult}\"");
            }
            else
            {
                MessageBox.Show(this, 
                    "Не удалось вырезать лица с фотографии", 
                    "Ошибка",
                    MessageBox.MessageBoxButtons.Ok);
            }
        }
    

        private async Task GetImagePath(Window parent)
        {
            var fileSelect = new OpenFileDialog()
            {
                AllowMultiple = false, 
                Filters = new List<FileDialogFilter>()
                {
                    new FileDialogFilter()
                    {
                        Extensions = new List<string>(){"jpg", "png", "jpeg"}
                    }
                }
            };
            var result = await fileSelect.ShowAsync(parent);
            if (result != null && result.Length > 0)
            {
                ImagePath = result.Length > 0 ? result[0] : null;
            }
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            textBoxText = this.Find<TextBox>("TextBox").Text;
            if (string.IsNullOrEmpty(textBoxText))
            {
                MessageBox.Show(this, "Сначала введите адрес", "Ошибка", MessageBox.MessageBoxButtons.Ok);
                return;
            }

            GetImagePath(this);
        }
    }
}