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
    /// <summary>
    /// Логика взаимодействия для страницы добавления новых платежей
    /// </summary>
    public partial class AddPaymentPage : Page
    {
        private Payment _currentPayment = new Payment();

        public AddPaymentPage(Payment selectedPayment)
        {
            InitializeComponent();

            CBCategory.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().Category.ToList();
            CBCategory.DisplayMemberPath = "Name";

            CBUser.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().User.ToList();
            CBUser.DisplayMemberPath = "FIO";

            if (selectedPayment != null)
                _currentPayment = selectedPayment;
            else
                _currentPayment.Date = DateTime.Today; 

            DataContext = _currentPayment;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (DPDate.SelectedDate == null)
                errors.AppendLine("Укажите дату!");
            else
                _currentPayment.Date = DPDate.SelectedDate.Value;

            if (string.IsNullOrWhiteSpace(_currentPayment.Num.ToString()) || _currentPayment.Num <= 0)
                errors.AppendLine("Укажите корректное количество!");

            if (string.IsNullOrWhiteSpace(_currentPayment.Price.ToString()) || _currentPayment.Price <= 0)
                errors.AppendLine("Укажите корректную цену");

            if (string.IsNullOrWhiteSpace(_currentPayment.UserID.ToString()))
                errors.AppendLine("Укажите клиента!");

            if (string.IsNullOrWhiteSpace(_currentPayment.CategoryID.ToString()))
                errors.AppendLine("Укажите категорию!");

            if (string.IsNullOrWhiteSpace(_currentPayment.Name))
                errors.AppendLine("Укажите название платежа!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_currentPayment.ID == 0)
                Vybornov_DB_PaymentEntities1.GetContext().Payment.Add(_currentPayment);

            try
            {
                Vybornov_DB_PaymentEntities1.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void TBCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBPaymentName.Text = "";
            TBAmount.Text = "";
            TBCount.Text = "";
            DPDate.SelectedDate = DateTime.Today; 
            CBCategory.SelectedIndex = -1;
            CBUser.SelectedIndex = -1;
        }
    }
}