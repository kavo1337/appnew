using app.DESCKTOP.Views.Auth;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace app.CLIENT.Views.Monitoring
{
	public partial class MonitoringPage : Page
	{
		private readonly HttpClient _httpClient = new();
		private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
		private string _statusFilter = "", _connectionFilter = "", _additionalFilter = "";
		private List<MonitoringMachineItem> _allData = new();

		public ObservableCollection<MonitoringRow> Rows { get; } = new();

		public MonitoringPage()
		{
			InitializeComponent();
			_httpClient.BaseAddress = Session.GetApiBaseUri();
			DataContext = this;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e) => await LoadData();
		private async void ApplyButton_Click(object sender, RoutedEventArgs e) => await LoadData();

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			WorkingStatusButton.IsChecked = NotWorkingStatusButton.IsChecked = MaintenanceStatusButton.IsChecked = false;
			MdiButton.IsChecked = ExeButton.IsChecked = PhButton.IsChecked = StButton.IsChecked = false;
			ErrorButton.IsChecked = QuestionButton.IsChecked = Circle1Button.IsChecked = Circle2Button.IsChecked =
			Circle3Button.IsChecked = Circle4Button.IsChecked = Circle5Button.IsChecked = Circle6Button.IsChecked = false;

			_statusFilter = _connectionFilter = _additionalFilter = "";
			SortComboBox.SelectedIndex = 0;
		}

		private void StatusButton_Checked(object sender, RoutedEventArgs e)
		{
			var btn = (ToggleButton)sender;

			if (btn.IsChecked == true)
			{
				WorkingStatusButton.IsChecked = btn == WorkingStatusButton;
				NotWorkingStatusButton.IsChecked = btn == NotWorkingStatusButton;
				MaintenanceStatusButton.IsChecked = btn == MaintenanceStatusButton;

				_statusFilter = btn.Name switch
				{
					"WorkingStatusButton" => "Работает",
					"NotWorkingStatusButton" => "Не работает",
					"MaintenanceStatusButton" => "На обслуживании",
					_ => ""
				};
			}
			else
			{
				if (!WorkingStatusButton.IsChecked.Value && !NotWorkingStatusButton.IsChecked.Value && !MaintenanceStatusButton.IsChecked.Value)
					_statusFilter = "";
			}
		}

		private void ConnectionButton_Checked(object sender, RoutedEventArgs e)
		{
			var btn = (ToggleButton)sender;

			if (btn.IsChecked == true)
			{
				MdiButton.IsChecked = btn == MdiButton;
				ExeButton.IsChecked = btn == ExeButton;
				PhButton.IsChecked = btn == PhButton;
				StButton.IsChecked = btn == StButton;

				_connectionFilter = btn.Name switch
				{
					"MdiButton" => "MDI",
					"ExeButton" => "EXE",
					"PhButton" => "PH",
					"StButton" => "ST",
					_ => ""
				};
			}
			else
			{
				if (!MdiButton.IsChecked.Value && !ExeButton.IsChecked.Value && !PhButton.IsChecked.Value && !StButton.IsChecked.Value)
					_connectionFilter = "";
			}
		}

		private void AdditionalButton_Checked(object sender, RoutedEventArgs e)
		{
			var btn = (ToggleButton)sender;

			if (btn.IsChecked == true)
			{
				var buttons = new[] { ErrorButton, QuestionButton, Circle1Button, Circle2Button, Circle3Button, Circle4Button, Circle5Button, Circle6Button };
				foreach (var b in buttons) b.IsChecked = b == btn;

				_additionalFilter = btn.Name switch
				{
					"ErrorButton" => "NoConnection",
					"QuestionButton" => "HardwareProblems",
					"Circle1Button" => "NoSales",
					"Circle2Button" => "NoEncashment",
					"Circle3Button" => "NoService",
					"Circle4Button" => "NoFillup",
					"Circle5Button" => "FewChange",
					"Circle6Button" => "FewGoods",
					_ => ""
				};
			}
			else
			{
				if (!ErrorButton.IsChecked.Value && !QuestionButton.IsChecked.Value && !Circle1Button.IsChecked.Value &&
					!Circle2Button.IsChecked.Value && !Circle3Button.IsChecked.Value && !Circle4Button.IsChecked.Value &&
					!Circle5Button.IsChecked.Value && !Circle6Button.IsChecked.Value)
					_additionalFilter = "";
			}
		}

		private async Task LoadData()
		{
			try
			{
				var url = Session.GetApiUrl($"api/monitoring/machines?status={_statusFilter}&connectionType={_connectionFilter}&additionalStatus={_additionalFilter}".Replace("?&", "?"));
				using var request = new HttpRequestMessage(HttpMethod.Get, url);
				if (!string.IsNullOrWhiteSpace(Session.AccessToken))
					request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Session.AccessToken);

				var response = await _httpClient.SendAsync(request);
				if (!response.IsSuccessStatusCode)
				{
					MessageBox.Show("Ошибка при загрузке данных");
					return;
				}

				var content = await response.Content.ReadAsStringAsync();
				var machines = JsonSerializer.Deserialize<MonitoringMachineItem[]>(content, _jsonOptions) ?? Array.Empty<MonitoringMachineItem>();
				_allData = new List<MonitoringMachineItem>(machines);

				UpdateGrid();
			}
			catch { MessageBox.Show("Ошибка загрузки данных"); }
		}

		private void UpdateGrid()
		{
			if (_allData.Count == 0)
			{
				Rows.Clear();
				return;
			}

			var sortByTime = (SortComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "По времени";

			var sorted = sortByTime
				? _allData.OrderByDescending(i => DateTime.TryParse(i.SystemTime, out var dt) ? dt : DateTime.MinValue)
				: _allData.OrderBy(i => i.Status == "Не работает" ? 0 : i.Status == "На обслуживании" ? 1 : i.Status == "Работает" ? 2 : 3);

			Rows.Clear();
			int row = 1;
			foreach (var m in sorted)
			{
				Rows.Add(new MonitoringRow
				{
					RowNumber = row++,
					TA = m.Name,
					Connection = m.ConnectionState,
					Load = m.LoadItems?.Any() == true ? string.Join(", ", m.LoadItems.Select(x => $"{x.Name} {x.Percent}%")) : "0%",
					Cash = m.CashInMachine.ToString("N0"),
					Events = m.Events,
					Equipment = m.Equipment,
					Info = m.InfoStatus,
					Additional = m.Additional,
					Time = DateTime.TryParse(m.SystemTime, out var t) ? t.ToString("dd.MM.yyyy HH:mm") : m.SystemTime,
					AccountBalance = m.AccountBalance.ToString("N0")
				});
			}

			TotalCountTextBlock.Text = $"Итого автоматов: {_allData.Count}";
			TotalCashTextBlock.Text = $"Денег в автоматах: {_allData.Sum(x => x.CashInMachine):N0} ₽";
		}

		private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateGrid();
		private void StatusButton_Unchecked(object sender, RoutedEventArgs e) => StatusButton_Checked(sender, e);
		private void ConnectionButton_Unchecked(object sender, RoutedEventArgs e) => ConnectionButton_Checked(sender, e);
		private void AdditionalButton_Unchecked(object sender, RoutedEventArgs e) => AdditionalButton_Checked(sender, e);
		private void MonitoringDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
	}

	public class MonitoringRow
	{
		public int RowNumber { get; set; }
		public string TA { get; set; } = "";
		public string Connection { get; set; } = "";
		public string Load { get; set; } = "";
		public string Cash { get; set; } = "";
		public string Events { get; set; } = "";
		public string Equipment { get; set; } = "";
		public string Info { get; set; } = "";
		public string Additional { get; set; } = "";
		public string Time { get; set; } = "";
		public string AccountBalance { get; set; } = "";
	}

	public class MonitoringMachineItem
	{
		public string Name { get; set; } = "";
		public string Status { get; set; } = "";
		public string ConnectionState { get; set; } = "";
		public List<LoadItem> LoadItems { get; set; } = new();
		public decimal CashInMachine { get; set; }
		public string Events { get; set; } = "";
		public string Equipment { get; set; } = "";
		public string InfoStatus { get; set; } = "";
		public string Additional { get; set; } = "";
		public string SystemTime { get; set; } = "";
		public decimal AccountBalance { get; set; }
	}

	public class LoadItem
	{
		public string Name { get; set; } = "";
		public int Percent { get; set; }
	}
}