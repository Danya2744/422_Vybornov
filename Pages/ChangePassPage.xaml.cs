using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для страницы смены пароля
    /// </summary>
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return;
            }
            string hashedPass = GetHash(CurrentPasswordBox.Password);
            var user = Vybornov_DB_PaymentEntities1.GetContext().User
                .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedPass);

            if (user == null)
            {
                MessageBox.Show("Текущий пароль/Логин неверный!");
                return;
            }
            if (NewPasswordBox.Password.Length >= 6)
            {
                bool en = true;
                bool number = false;

                for (int i = 0; i < NewPasswordBox.Password.Length; i++)
                {
                    if (NewPasswordBox.Password[i] >= '0' && NewPasswordBox.Password[i] <= '9')
                        number = true;
                    else if (!((NewPasswordBox.Password[i] >= 'A' && NewPasswordBox.Password[i] <= 'Z') ||
                              (NewPasswordBox.Password[i] >= 'a' && NewPasswordBox.Password[i] <= 'z')))
                        en = false;
                }

                if (!en)
                {
                    MessageBox.Show("Используйте только английскую раскладку!");
                    return;
                }
                else if (!number)
                {
                    MessageBox.Show("Добавьте хотя бы одну цифру!");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }
            user.Password = GetHash(NewPasswordBox.Password);
            Vybornov_DB_PaymentEntities1.GetContext().SaveChanges();

            MessageBox.Show("Пароль успешно изменен!");
            NavigationService?.Navigate(new AuthPage());
        }
    }
}
