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

namespace _422_Vybornov.Pages
{
    public partial class PaymentTabPage : Page
    {
        public PaymentTabPage()
        {
            InitializeComponent();
            DataGridPayment.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().Payment.ToList();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Vybornov_DB_PaymentEntities1.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                DataGridPayment.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().Payment.ToList();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPaymentPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var paymentForRemoving = DataGridPayment.SelectedItems.Cast<Payment>().ToList();

            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {paymentForRemoving.Count()} элементов?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Vybornov_DB_PaymentEntities1.GetContext().Payment.RemoveRange(paymentForRemoving);
                    Vybornov_DB_PaymentEntities1.GetContext().SaveChanges();
                    MessageBox.Show("Данные успешно удалены!");
                    DataGridPayment.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().Payment.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddPaymentPage((sender as Button).DataContext as Payment));
        }
    }
}
