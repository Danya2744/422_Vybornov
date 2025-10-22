using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _422_Vybornov.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;
        private User currentUser;

        public AuthPage()
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

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин или пароль");
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            using (var db = new Vybornov_DB_PaymentEntities1())
            {
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    failedAttempts++;

                    // Здесь нужно добавить логику для капчи
                    // if (failedAttempts >= 3)
                    // {
                    //     if (captcha.Visibility != Visibility.Visible)
                    //     {
                    //         CaptchaSwitch();
                    //     }
                    //     CaptchaChange();
                    // }
                    return;
                }
                else
                {
                    MessageBox.Show("Пользователь успешно найден!");
                    currentUser = user;

                    switch (user.Role)
                    {
                        //case "user":
                        //    navigationservice?.navigate(new userpage());
                        //    break;
                        //case "admin":
                        //    navigationservice?.navigate(new adminpage());
                        //    break;
                        //default:
                        //    messagebox.show("неизвестная роль пользователя");
                        //    break;
                    }
                }
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
        }

        private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxLogin.Focus();
        }

        private void txtHintPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }
    }
}