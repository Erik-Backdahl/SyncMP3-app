using System;
using System.IO;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaTest
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(StartUpAction.TrySendPing());
        }
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Clicky");
        }
    }
}