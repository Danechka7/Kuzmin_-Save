using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Path = System.IO.Path;

namespace Kuzmin_ГлазкиSave
{
    public partial class AddEditPage : Page
    {
        private Agent _currentAgent;

        public AddEditPage(Agent selectedAgent = null)
        {
            InitializeComponent();
            LoadAgentTypes();

            if (selectedAgent != null)
            {
                _currentAgent = selectedAgent;
                DeleteButton.Visibility = Visibility.Visible;
                // Устанавливаем выбранный тип агента
                if (TypeComboBox.ItemsSource != null)
                    TypeComboBox.SelectedValue = _currentAgent.AgentTypeID;
            }
            else
            {
                _currentAgent = new Agent();
                DeleteButton.Visibility = Visibility.Collapsed;
            }

            DataContext = _currentAgent;
            this.Unloaded += AddEditPage_Unloaded;
        }

        private void LoadAgentTypes()
        {
            try
            {
                var types = KuzminBD_ГлазкиSaveEntities.GetContext().AgentType.ToList();
                TypeComboBox.ItemsSource = types;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentAgent.Title))
                errors.AppendLine("Укажите наименование агента");

            if (TypeComboBox.SelectedItem == null)
                errors.AppendLine("Выберите тип агента");
            else
                _currentAgent.AgentTypeID = (int)TypeComboBox.SelectedValue;

            if (string.IsNullOrWhiteSpace(_currentAgent.Address))
                errors.AppendLine("Укажите адрес");

            if (string.IsNullOrWhiteSpace(_currentAgent.DirectorName))
                errors.AppendLine("Укажите ФИО директора");

            if (_currentAgent.Priority < 0)
                errors.AppendLine("Приоритет должен быть неотрицательным числом");

            if (string.IsNullOrWhiteSpace(_currentAgent.INN))
                errors.AppendLine("Укажите ИНН");

            if (string.IsNullOrWhiteSpace(_currentAgent.KPP))
                errors.AppendLine("Укажите КПП");

            if (string.IsNullOrWhiteSpace(_currentAgent.Phone))
                errors.AppendLine("Укажите телефон");
            else
            {
                string cleanPhone = _currentAgent.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace("+", "").Replace(" ", "");
                if (cleanPhone.Length < 10)
                    errors.AppendLine("Телефон указан неверно");
            }

            if (string.IsNullOrWhiteSpace(_currentAgent.Email))
                errors.AppendLine("Укажите email");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                var context = KuzminBD_ГлазкиSaveEntities.GetContext();

                if (_currentAgent.ID == 0)
                    context.Agent.Add(_currentAgent);

                context.SaveChanges();
                MessageBox.Show("Информация сохранена");

                // Вместо сложной логики - просто перезапускаем страницу списка
                NavigationService.Navigate(new ProductPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddEditPage_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentAgent != null && _currentAgent.ID != 0)
                {
                    var context = KuzminBD_ГлазкиSaveEntities.GetContext();
                    var entry = context.Entry(_currentAgent);

                    if (entry.State != System.Data.Entity.EntityState.Detached &&
                        entry.State != System.Data.Entity.EntityState.Unchanged)
                    {
                        entry.Reload();
                    }
                }
            }
            catch
            {
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var context = KuzminBD_ГлазкиSaveEntities.GetContext();

                // Проверка контекста
                if (context == null)
                {
                    MessageBox.Show("Ошибка подключения к базе данных");
                    return;
                }

                // Проверка существования агента в БД
                var agentInDb = context.Agent.FirstOrDefault(a => a.ID == _currentAgent.ID);
                if (agentInDb == null)
                {
                    MessageBox.Show("Агент не найден в базе данных");
                    return;
                }

                // ПРОВЕРКА НА НАЛИЧИЕ ПРОДАЖ
                bool hasSales = context.ProductSale.Any(ps => ps.AgentID == _currentAgent.ID);

                if (hasSales)
                {
                    MessageBox.Show("Невозможно удалить агента, так как у него есть информация о реализации продукции!",
                                  "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Вы точно хотите выполнить удаление?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Удаляем связанные данные
                    var history = context.AgentPriorityHistory.Where(aph => aph.AgentID == _currentAgent.ID).ToList();
                    if (history.Any())
                        context.AgentPriorityHistory.RemoveRange(history);

                    var shops = context.Shop.Where(s => s.AgentID == _currentAgent.ID).ToList();
                    if (shops.Any())
                        context.Shop.RemoveRange(shops);

                    context.Agent.Remove(agentInDb);

                    int saved = context.SaveChanges();

                    if (saved > 0)
                    {
                        MessageBox.Show("Агент удален");
                        NavigationService.Navigate(new ProductPage());
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить агента");
                    }
                }
            }
            
            
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\nДетали: {ex.InnerException?.Message}");
            }
        }


        private void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
        {
            string agentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agents");
            var dialog = new OpenFileDialog
            {
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp",
                InitialDirectory = agentsFolder
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Создаем папку agents, если её нет
                    Directory.CreateDirectory(agentsFolder);

                    string selectedFile = dialog.FileName;
                    string fileName = Path.GetFileName(selectedFile);
                    string destPath = Path.Combine(agentsFolder, fileName);

                    // Если файл не из папки agents - копируем
                    if (!Path.GetDirectoryName(selectedFile).Equals(agentsFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        // Проверяем дубликаты
                        int i = 1;
                        while (File.Exists(destPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            string ext = Path.GetExtension(fileName);
                            destPath = Path.Combine(agentsFolder, $"{nameWithoutExt}_{i++}{ext}");
                        }
                        File.Copy(selectedFile, destPath);
                    }
                    else
                    {
                        destPath = selectedFile;
                    }

                    _currentAgent.Logo = $"agents\\{Path.GetFileName(destPath)}";
                    LogoImage.Source = new BitmapImage(new Uri(destPath));

                    var binding = LogoImage.GetBindingExpression(Image.SourceProperty);
                    binding?.UpdateTarget();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            KuzminClass.MainFrame.GoBack();
        }
    }
}