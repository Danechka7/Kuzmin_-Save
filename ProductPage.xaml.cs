    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;

    namespace Kuzmin_ГлазкиSave
    {
        public partial class ProductPage : Page
        {
            int _currentPage = 1;
            int _maxPage = 0;
            int _pageSize = 10;
            List<Agent> _allAgents;

            public ProductPage()
            {
                InitializeComponent();
                ComboType.SelectedIndex = 0;
                LoadData();
            }
            public void RefreshData()
            {
                _allAgents = KuzminBD_ГлазкиSaveEntities.GetContext().Agent.ToList();
                _maxPage = (int)Math.Ceiling(_allAgents.Count / (double)_pageSize);
                _currentPage = 1;
                UpdatePage();
            }

            public void LoadData()
            {
                _allAgents = KuzminBD_ГлазкиSaveEntities.GetContext().Agent.ToList();
                _maxPage = (int)Math.Ceiling(_allAgents.Count / (double)_pageSize);
                UpdatePage();
            }

            void UpdatePage()
            {
                int skip = (_currentPage - 1) * _pageSize;
                AgentListView.ItemsSource = _allAgents.Skip(skip).Take(_pageSize).ToList();

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

            public void UpdateAgents()
            {
                LoadData();
                var currentAgents = _allAgents.ToList();

                // Поиск
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

                // Фильтр по типу
                if (ComboType.SelectedIndex > 0)
                {
                    var selectedType = (ComboType.SelectedItem as TextBlock)?.Text;
                    if (!string.IsNullOrEmpty(selectedType))
                        currentAgents = currentAgents.Where(a => a.AgentTypeName == selectedType).ToList();
                }

                // Сортировка
                switch (ComboSorting.SelectedIndex)
                {
                    case 1: currentAgents = currentAgents.OrderBy(a => a.Title).ToList(); break;
                    case 2: currentAgents = currentAgents.OrderByDescending(a => a.Title).ToList(); break;
                    case 3: currentAgents = currentAgents.OrderBy(a => a.Discount).ToList(); break;
                    case 4: currentAgents = currentAgents.OrderByDescending(a => a.Discount).ToList(); break;
                    case 5: currentAgents = currentAgents.OrderBy(a => a.Priority).ToList(); break;
                    case 6: currentAgents = currentAgents.OrderByDescending(a => a.Priority).ToList(); break;
                }

                AgentListView.ItemsSource = currentAgents;
            }

            private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateAgents();
            private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateAgents();
            private void ComboSorting_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateAgents();
            private void AddAgentBtn_Click(object sender, RoutedEventArgs e) => KuzminClass.MainFrame.Navigate(new AddEditPage());

            private void EditAgentBtn_Click(object sender, RoutedEventArgs e)
            {
                if (AgentListView.SelectedItem is Agent selectedAgent)
                    KuzminClass.MainFrame.Navigate(new AddEditPage(selectedAgent));
                else
                    MessageBox.Show("Выберите агента для редактирования");
            }
            private void Page_Loaded(object sender, RoutedEventArgs e)
            {
                LoadData();
                UpdateAgents();
            }
        }
    }