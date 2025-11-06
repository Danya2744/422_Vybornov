using _422_Vybornov.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _422_Vybornov
{
    /// <summary>
    /// Логика взаимодействия для основного окна MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new AuthPage());
            MainFrame.Navigated += MainFrame_Navigated;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Back.IsEnabled = MainFrame.CanGoBack;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer(); timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString(); };
            timer.Start();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Message", MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
                e.Cancel = true;
            else
                e.Cancel = false;
        }

        private void Themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Themes.SelectedItem is ComboBoxItem selectedItem)
            {
                string styleFile = selectedItem.Tag as string;
                if (!string.IsNullOrEmpty(styleFile) && styleFile == "MyDictionary.xaml")
                {
                    var uri = new Uri("MyDictionary.xaml", UriKind.Relative);
                    ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                }
                else if (styleFile == "Dictionary.xaml")
                {
                    var uri = new Uri("Dictionary.xaml", UriKind.Relative);
                    ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                    Application.Current.Resources.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                }
            }
        }
    }
}
