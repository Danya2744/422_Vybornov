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
    /// Логика взаимодействия для CategoryTabPage.xaml
    /// </summary>
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
        {
            InitializeComponent();
            DataGridCategory.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().Category.ToList();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Vybornov_DB_PaymentEntities1.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                DataGridCategory.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().Category.ToList();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var categoriesForRemoving = DataGridCategory.SelectedItems.Cast<Category>().ToList();

            if (categoriesForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите категории для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var categoriesWithPayments = new List<Category>();
            foreach (var category in categoriesForRemoving)
            {
                var hasPayments = Vybornov_DB_PaymentEntities1.GetContext().Payment.Any(p => p.CategoryID == category.ID);
                if (hasPayments)
                {
                    categoriesWithPayments.Add(category);
                }
            }

            string message;
            if (categoriesWithPayments.Any())
            {
                message = $"Вы точно хотите удалить {categoriesForRemoving.Count} категорий?\n" +
                         $"Внимание: {categoriesWithPayments.Count} категорий имеют связанные платежи, " +
                         $"которые также будут удалены!";
            }
            else
            {
                message = $"Вы точно хотите удалить {categoriesForRemoving.Count} категорий?";
            }

            if (MessageBox.Show(message, "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new Vybornov_DB_PaymentEntities1())
                    {
                        foreach (var category in categoriesForRemoving)
                        {
                            var categoryWithPayments = context.Category
                                .Include("Payment") 
                                .FirstOrDefault(c => c.ID == category.ID);

                            if (categoryWithPayments != null)
                            {
                                if (categoryWithPayments.Payment != null && categoryWithPayments.Payment.Any())
                                {
                                    context.Payment.RemoveRange(categoryWithPayments.Payment);
                                }

                                context.Category.Remove(categoryWithPayments);
                            }
                        }

                        context.SaveChanges();
                    }

                    MessageBox.Show("Данные успешно удалены!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    Vybornov_DB_PaymentEntities1.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                    DataGridCategory.ItemsSource = Vybornov_DB_PaymentEntities1.GetContext().Category.ToList();
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
            NavigationService.Navigate(new AddCategoryPage((sender as Button).DataContext as Category));
        }
    }
}
