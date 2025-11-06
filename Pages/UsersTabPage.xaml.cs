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
    /// Логика взаимодействия для страницы отображения всех пользователей
    /// </summary>
    public partial class UsersTabPage : Page
    {
        public UsersTabPage()
        {
            InitializeComponent();
            DataGridUser.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().User.ToList();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Vybornov_DB_PaymentEntities1.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                DataGridUser.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().User.ToList();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddUserPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var usersForRemoving = DataGridUser.SelectedItems.Cast<User>().ToList();

            if (usersForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите пользователей для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var usersWithPayments = new List<User>();
            foreach (var user in usersForRemoving)
            {
                var hasPayments = Vybornov_DB_PaymentEntities1.GetContext().Payment.Any(p => p.UserID == user.ID);
                if (hasPayments)
                {
                    usersWithPayments.Add(user);
                }
            }

            string message;
            if (usersWithPayments.Any())
            {
                message = $"Вы точно хотите удалить {usersForRemoving.Count} пользователей?\n" +
                         $"Внимание: {usersWithPayments.Count} пользователей имеют связанные платежи, " +
                         $"которые также будут удалены!";
            }
            else
            {
                message = $"Вы точно хотите удалить {usersForRemoving.Count} пользователей?";
            }

            if (MessageBox.Show(message, "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new Vybornov_DB_PaymentEntities1())
                    {
                        foreach (var user in usersForRemoving)
                        {
                            var userWithPayments = context.User
                                .Include("Payment") 
                                .FirstOrDefault(u => u.ID == user.ID);

                            if (userWithPayments != null)
                            {
                                if (userWithPayments.Payment != null && userWithPayments.Payment.Any())
                                {
                                    context.Payment.RemoveRange(userWithPayments.Payment);
                                }

                                context.User.Remove(userWithPayments);
                            }
                        }

                        context.SaveChanges();
                    }

                    MessageBox.Show("Данные успешно удалены!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    Vybornov_DB_PaymentEntities1.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                    DataGridUser.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().User.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}\n\n" +
                                   "Если ошибка связана с внешними ключами, " +
                                   "убедитесь, что в базе данных настроено каскадное удаление.",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddUserPage((sender as Button).DataContext as User));
        }
    }
}
