using app.DESCKTOP.Views.Shell;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace app.DESCKTOP.Views.Auth
{
	/// <summary>
	/// Логика взаимодействия для AuthWindow.xaml
	/// </summary>
	public partial class AuthWindow : Window
	{
		private readonly HttpClient _client = new();
		private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
		public AuthWindow()
		{
			InitializeComponent();
			_client.BaseAddress = new Uri("https://localhost:7062/");
			Session.ApiBaseUrl = _client.BaseAddress.ToString();
		}

		private async void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			var login = LoginBox.Text.Trim();
			var password = PasswordBox.Password.Trim();

			if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
			{
				Statusbar.Text = "Заполните все поля...";
				return;
			}
			var payload = new LoginRequest(login, password);
			var json = JsonSerializer.Serialize(payload);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			ButtonLogin.IsEnabled = false;
			Statusbar.Text = "Ожидайте ...";
			try
			{
				var response = await _client.PostAsync("api/auth/login", content);
				if (!response.IsSuccessStatusCode)
				{
					Statusbar.Text = "Ошибка входа...";
					return;
				}

				var body = await response.Content.ReadAsStringAsync();
				var data = JsonSerializer.Deserialize<LoginResponse>(body, _jsonOptions);
				if (data is null)
				{
					Statusbar.Text = "Ошибка входа...";
					return;
				}

				Session.AccessToken = data.AccessToken;
				Session.RefreshToken = data.RefreshToken;
				Session.User = data.User;

				Statusbar.Text = "Вы успешно вошли...";


				var shell = new ShellWindow();
				shell.Show();
				Close();

			}
			catch (Exception ex)
			{
				Statusbar.Text = "Ошибка: " + ex.Message;
			}
			finally
			{
				ButtonLogin.IsEnabled = true;
			}
		}
	}
}
