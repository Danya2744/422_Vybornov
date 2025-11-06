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
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;

namespace _422_Vybornov.Pages
{
    /// <summary>
    /// Логика взаимодействия для страницы добавления новых пользователей 
    /// </summary>
    public partial class AddUserPage : Page
    {
        private User _currentUser = new User();
        private string _selectedImagePath = "";

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();

            if (selectedUser != null)
            {
                _currentUser = selectedUser;
                _selectedImagePath = selectedUser.Photo ?? "";
                LoadSelectedImage();
            }

            DataContext = _currentUser;

            SetupDragDrop();
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void SetupDragDrop()
        {
            ImageBorder.AllowDrop = true;
            ImageBorder.Drop += Border_Drop;
            ImageBorder.DragEnter += Border_DragEnter;
            ImageBorder.DragOver += Border_DragOver;
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    LoadImageFromPath(files[0]);
                }
            }
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                LoadImageFromPath(openFileDialog.FileName);
            }
        }

        private void LoadImageFromPath(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    UserImage.Source = bitmap;
                    UserImage.Visibility = Visibility.Visible;
                    ImagePlaceholder.Visibility = Visibility.Collapsed;

                    _selectedImagePath = filePath;
                    _currentUser.Photo = filePath;
                    SelectedImagePath.Text = filePath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка");
            }
        }

        private void LoadSelectedImage()
        {
            if (!string.IsNullOrEmpty(_selectedImagePath) && File.Exists(_selectedImagePath))
            {
                LoadImageFromPath(_selectedImagePath);
            }
            else
            {
                UserImage.Visibility = Visibility.Collapsed;
                ImagePlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.Login))
                errors.AppendLine("Укажите логин!");

            if (string.IsNullOrWhiteSpace(TBPass.Text))
                errors.AppendLine("Укажите пароль!");

            if ((_currentUser.Role == null) || (cmbRole.Text == ""))
                errors.AppendLine("Выберите роль!");
            else
                _currentUser.Role = cmbRole.Text;

            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
                errors.AppendLine("Укажите ФИО");

            if (!string.IsNullOrEmpty(TBPass.Text))
            {
                if (TBPass.Text.Length >= 6)
                {
                    bool en = true;
                    bool number = false;

                    for (int i = 0; i < TBPass.Text.Length; i++)
                    {
                        if (TBPass.Text[i] >= '0' && TBPass.Text[i] <= '9')
                            number = true;
                        else if (!((TBPass.Text[i] >= 'A' && TBPass.Text[i] <= 'Z') ||
                                  (TBPass.Text[i] >= 'a' && TBPass.Text[i] <= 'z')))
                            en = false;
                    }

                    if (!en)
                    {
                        errors.AppendLine("Используйте только английскую раскладку в пароле!");
                    }
                    else if (!number)
                    {
                        errors.AppendLine("Добавьте хотя бы одну цифру в пароль!");
                    }
                }
                else
                {
                    errors.AppendLine("Пароль слишком короткий, должно быть минимум 6 символов!");
                }
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_currentUser.ID == 0)
            {
                using (var db = new Vybornov_DB_PaymentEntities1())
                {
                    var existingUser = db.User
                        .AsNoTracking()
                        .FirstOrDefault(u => u.Login == _currentUser.Login);

                    if (existingUser != null)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!");
                        return;
                    }
                }
            }

            if (!string.IsNullOrEmpty(TBPass.Text))
            {
                _currentUser.Password = GetHash(TBPass.Text);
            }
            _currentUser.Photo = _selectedImagePath;

            if (_currentUser.ID == 0)
                Vybornov_DB_PaymentEntities1.GetContext().User.Add(_currentUser);

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

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Text = "";
            TBPass.Text = "";
            cmbRole.SelectedIndex = -1;
            TBFio.Text = "";

            UserImage.Source = null;
            UserImage.Visibility = Visibility.Collapsed;
            ImagePlaceholder.Visibility = Visibility.Visible;
            _selectedImagePath = "";
            _currentUser.Photo = "";
            SelectedImagePath.Text = "";
        }
    }
}