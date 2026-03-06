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

    namespace Kuzmin_ГлазкиSave
    {
        /// <summary>
        /// Логика взаимодействия для ProductPage.xaml
        /// </summary>
        /// 
        public partial class ProductPage : Page
        {
            int _currentPage = 1;
            int _maxPage = 0;
            int _pageSize = 10;
        List<Agent> _allAgents;

        public ProductPage()
            {
                InitializeComponent();
                var currentAgents = KuzminBD_ГлазкиSaveEntities.GetContext().Agent.ToList();
                AgentListView.ItemsSource = currentAgents;
                ComboType.SelectedIndex = 0;
                LoadData();
            }
            
        void LoadData()
        {
            _allAgents = KuzminBD_ГлазкиSaveEntities.GetContext().Agent.ToList();
            _maxPage = (int)Math.Ceiling(_allAgents.Count /(double)_pageSize);
            UpdatePage();
        }
        void UpdatePage()
        {
            // Показываем текущую страницу
            int skip = (_currentPage - 1) * _pageSize;
            AgentListView.ItemsSource = _allAgents.Skip(skip).Take(_pageSize).ToList();

            // Создаем кнопки страниц
            PageButtonsPanel.Children.Clear();
            for (int i = 1; i <= _maxPage; i++)
            {
                Button btn = new Button() { Content = i, Width = 30, Margin = new Thickness(3) };
                if (i == _currentPage)
                    btn.Background = Brushes.LightBlue;
                int pageNum = i;
                btn.Click += (s, e) => { _currentPage = pageNum; UpdatePage(); };

                PageButtonsPanel.Children.Add(btn);
            }

            // Вкл/выкл кнопки навигации
            PrevButton.IsEnabled = _currentPage > 1;
            NextButton.IsEnabled = _currentPage < _maxPage;
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage--;
            UpdatePage();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++;
            UpdatePage();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
            {
                KuzminClass.MainFrame.Navigate(new AddEditPage());
            }
            private void UpdateAgents()
            {
                LoadData();
                var context = KuzminBD_ГлазкиSaveEntities.GetContext();
                var currentAgents = context.Agent.ToList();

                // Поиск по тексту (в названии или телефоне)

                if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                {
                    var searchText = TBoxSearch.Text.ToLower();

                    var digitsOnlySearch = new string(searchText.Where(char.IsDigit).ToArray());

                    currentAgents = currentAgents.Where(a =>
                    {

                        if (a.Title != null && a.Title.ToLower().Contains(searchText))
                            return true;


                        if (a.Email != null && a.Email.ToLower().Contains(searchText))
                            return true;


                        if (a.Phone != null)
                        {

                            if (a.Phone.ToLower().Contains(searchText))
                                return true;


                            if (!string.IsNullOrEmpty(digitsOnlySearch))
                            {
                                var cleanPhone = new string(a.Phone.Where(char.IsDigit).ToArray());

                                if (cleanPhone.Contains(digitsOnlySearch))
                                    return true;
                            }
                        }

                        return false;
                    }).ToList();
                }
                // Фильтрация по типу агента (ComboType)
                if (ComboType.SelectedIndex > 0)
                {
                    var selectedType = (ComboType.SelectedItem as TextBlock)?.Text;
                    if (!string.IsNullOrEmpty(selectedType))
                    {
                        currentAgents = currentAgents.Where(a => a.AgentTypeName == selectedType).ToList();
                    }
                }

                // Сортировка (ComboSorting)
                switch (ComboSorting.SelectedIndex)
                {
                    case 1: // от А до Я
                        currentAgents = currentAgents.OrderBy(a => a.Title).ToList();
                        break;
                    case 2: // от Я до А
                        currentAgents = currentAgents.OrderByDescending(a => a.Title).ToList();
                        break;
                    case 3: // Скидка по возрастанию
                        currentAgents = currentAgents.OrderBy(a => a.Discount).ToList();
                        break;
                    case 4: // Скидка по убыванию
                        currentAgents = currentAgents.OrderByDescending(a => a.Discount).ToList();
                        break;
                    case 5: // Приоритет по возрастанию
                        currentAgents = currentAgents.OrderBy(a => a.Priority).ToList();
                        break;
                    case 6: // Приоритет по убыванию
                        currentAgents = currentAgents.OrderByDescending(a => a.Priority).ToList();
                        break;
                }

                // Устанавливаем источник данных
                AgentListView.ItemsSource = currentAgents;
            }

            private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
            {
                UpdateAgents();
            }

            private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                UpdateAgents();
            }

            private void ComboSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                UpdateAgents();
            }
        }
    }
