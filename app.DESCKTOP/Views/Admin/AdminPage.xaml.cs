using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using app.DESCKTOP.Views.Auth;
using Microsoft.Win32;

namespace VWSR.Desktop;

public partial class AdminPage : Page
{
    private readonly HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private int _page = 1;
    private int _pageSize = 20;
    private int _total;

    public ObservableCollection<VendingMachineRow> Machines { get; } = new();

    public AdminPage()
    {
        _httpClient.BaseAddress = Session.GetApiBaseUri();
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) => await LoadMachines();
    }

    private async Task LoadMachines()
    {
        try
        {
            var search = SearchBox.Text?.Trim();
            var url = Session.GetApiUrl($"api/vending-machines?search={Uri.EscapeDataString(search ?? string.Empty)}&page={_page}&pageSize={_pageSize}");

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyAuth(request);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Не удалось получить список ТА.");
                return;
            }

            var body = await response.Content.ReadAsStringAsync();
            var pageResult = JsonSerializer.Deserialize<PagedResult<VendingMachineListItem>>(body, _jsonOptions);
            if (pageResult == null)
            {
                return;
            }

            _total = pageResult.Total;
            _page = pageResult.Page;
            _pageSize = pageResult.PageSize;

            Machines.Clear();
            foreach (var item in pageResult.Items)
            {
                Machines.Add(new VendingMachineRow
                {
                    Id = item.Id,
                    Name = item.Name,
                    Model = item.Model,
                    Company = item.Company ?? string.Empty,
                    ModemId = item.ModemId,
                    Address = item.Address,
                    Place = item.Place,
                    WorkingSince = item.WorkingSince
                });
            }

            UpdatePagingInfo();
            ApplyGrouping();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}");
        }
    }

    private void ApplyGrouping()
    {
        var view = CollectionViewSource.GetDefaultView(Machines);
        view.GroupDescriptions.Clear();

        if (GroupByCompany.IsChecked == true)
        {
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(VendingMachineRow.Company)));
        }
    }

    private void UpdatePagingInfo()
    {
        var from = Machines.Count == 0 ? 0 : (_page - 1) * _pageSize + 1;
        var to = (_page - 1) * _pageSize + Machines.Count;

        PageInfo.Text = $"Стр. {_page}";
        TotalInfo.Text = $"Показано {from}-{to} из {_total}";
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(Session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.AccessToken);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _page = 1;
        _ = LoadMachines();
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        _page = 1;
        _ = LoadMachines();
    }

    private void PageSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PageSizeBox.SelectedItem is ComboBoxItem item && int.TryParse(item.Content?.ToString(), out var size))
        {
            _pageSize = size;
            _page = 1;
            _ = LoadMachines();
        }
    }

    private void Prev_Click(object sender, RoutedEventArgs e)
    {
        if (_page > 1)
        {
            _page--;
            _ = LoadMachines();
        }
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        var maxPage = (int)Math.Ceiling(_total / (double)_pageSize);
        if (_page < maxPage)
        {
            _page++;
            _ = LoadMachines();
        }
    }

    private void TableView_Click(object sender, RoutedEventArgs e)
    {
        MachinesGrid.Visibility = Visibility.Visible;
        TilesPanel.Visibility = Visibility.Collapsed;
    }

    private void TileView_Click(object sender, RoutedEventArgs e)
    {
        MachinesGrid.Visibility = Visibility.Collapsed;
        TilesPanel.Visibility = Visibility.Visible;
    }

    private void GroupByCompany_Checked(object sender, RoutedEventArgs e)
    {
        ApplyGrouping();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var form = new VendingMachineFormWindow(null);
        if (form.ShowDialog() == true)
        {
            await LoadMachines();
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (GetRowFromSender(sender) is not VendingMachineRow row)
        {
            return;
        }

        var detail = await LoadDetail(row.Id);
        if (detail == null)
        {
            MessageBox.Show("Не удалось открыть данные ТА.");
            return;
        }

        var form = new VendingMachineFormWindow(detail);
        if (form.ShowDialog() == true)
        {
            await LoadMachines();
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (GetRowFromSender(sender) is not VendingMachineRow row)
        {
            return;
        }

        if (MessageBox.Show("Удалить ТА?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Delete, Session.GetApiUrl($"api/vending-machines/{row.Id}"));
        ApplyAuth(request);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show("Не удалось удалить ТА.");
            return;
        }

        await LoadMachines();
    }

    private async void Unlink_Click(object sender, RoutedEventArgs e)
    {
        if (GetRowFromSender(sender) is not VendingMachineRow row)
        {
            return;
        }

        if (MessageBox.Show("Отвязать модем?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, Session.GetApiUrl($"api/vending-machines/{row.Id}/unlink-modem"));
        ApplyAuth(request);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show("Не удалось отвязать модем.");
            return;
        }

        MessageBox.Show("Модем отвязан.");
        await LoadMachines();
    }

    private VendingMachineRow? GetRowFromSender(object sender)
    {
        return (sender as FrameworkElement)?.DataContext as VendingMachineRow;
    }

    private async Task<VendingMachineDetail?> LoadDetail(int id)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Session.GetApiUrl($"api/vending-machines/{id}"));
        ApplyAuth(request);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<VendingMachineDetail>(body, _jsonOptions);
    }

    private static string? GetSavePath(string filter, string fileName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = fileName
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
