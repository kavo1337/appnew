using app.CLIENT.Views.Monitoring;
using app.DESCKTOP.Views.Auth;
using app.DESCKTOP.Views.Dashboard;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VWSR.Desktop;

namespace app.DESCKTOP.Views.Shell
{
	/// <summary>
	/// Логика взаимодействия для ShellWindow.xaml
	/// </summary>
	public partial class ShellWindow : Window
	{
		private bool _sidebarCollapsed;
		public ShellWindow()
		{
			InitializeComponent();
			OpenPage(new DashboardPage());
			LoadUserInfo();
		}

		private void OpenDashboard_Click(object sender, RoutedEventArgs e)
		{
			OpenPage(new DashboardPage());
		}

		private void OpenMonitoring_Click(object sender, RoutedEventArgs e)
		{
			OpenPage(new MonitoringPage());
		}

		private void ShowPlaceholder_Click(object sender, RoutedEventArgs e)
		{

			var text = (sender as FrameworkElement)?.Tag?.ToString() ?? "Раздел";
			MessageBox.Show($"{text}: в разработке.");
		}

		private void ToggleReportsMenu_Click(object sender, RoutedEventArgs e)
		{
			ToggleSubMenu(ReportsSubMenu, ReportsArrowIcon);
		}

		private void ToggleInventoryMenu_Click(object sender, RoutedEventArgs e)
		{
			ToggleSubMenu(InventorySubMenu, InventoryArrowIcon);
		}

		private void ToggleAdminMenu_Click(object sender, RoutedEventArgs e)
		{
			ToggleSubMenu(AdminSubMenu, AdminArrowIcon);
		}

		private void ToggleSubMenu(StackPanel subMenu, ImageBrush arrowIcon)
		{
			if (subMenu.Visibility == Visibility.Collapsed)
			{
				// Раскрываем меню
				subMenu.Visibility = Visibility.Visible;
				// Поворачиваем стрелку вниз
				arrowIcon.ImageSource = new BitmapImage(
					new Uri("pack://application:,,,/Views/Assets/Icons/angle-down.png"));
			}
			else
			{
				// Скрываем меню
				subMenu.Visibility = Visibility.Collapsed;
				// Возвращаем стрелку вправо
				arrowIcon.ImageSource = new BitmapImage(
					new Uri("pack://application:,,,/Views/Assets/Icons/angle-right.png"));
			}
		}

		private void OpenPage(Page page)
		{
			ContentFrame.Navigate(page);
		}

		private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
		{
			_sidebarCollapsed = !_sidebarCollapsed;
			SidebarColumn.Width = _sidebarCollapsed ? new GridLength(64) : new GridLength(250);


			var visibility = _sidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
			NavHeader.Visibility = visibility;
			MenuTextDashboard.Visibility = visibility;
			MenuTextMonitoring.Visibility = visibility;
			MenuTextReports.Visibility = visibility;
			MenuTextInventory.Visibility = visibility;
			MenuTextAdmin.Visibility = visibility;
		}

		private void ProfileButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.ContextMenu != null)
			{
				button.ContextMenu.PlacementTarget = button;
				button.ContextMenu.IsOpen = true;
			}
		}

		private void Logout_Click(object sender, RoutedEventArgs e)
		{

			Session.AccessToken = default;
			Session.RefreshToken = default;
			Session.User = default;

			var login = new AuthWindow();
			login.Show();
			Close();
		}

		private void LoadUserInfo()
		{
			var user = Session.User;
			if (user == null)
			{
				UserNameText.Text = "Гость";
				UserRoleText.Text = string.Empty;
				UserInitialsText.Text = string.Empty;
				return;
			}

			UserNameText.Text = BuildDisplayName(user.FullName);
			UserRoleText.Text = user.Role;
			UserInitialsText.Text = BuildInitials(user.FullName);

			if (!string.IsNullOrWhiteSpace(user.PhotoUrl))
			{
				try
				{
					UserPhoto.Source = new BitmapImage(new Uri(user.PhotoUrl, UriKind.Absolute));
					UserPhoto.Visibility = Visibility.Visible;
					PhotoPlaceholder.Visibility = Visibility.Collapsed;
				}
				catch
				{
					UserPhoto.Visibility = Visibility.Collapsed;
					PhotoPlaceholder.Visibility = Visibility.Visible;
				}
			}
		}

		private static string BuildDisplayName(string fullName)
		{
			if (string.IsNullOrWhiteSpace(fullName))
			{
				return string.Empty;
			}

			var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
			{
				return fullName;
			}

			var surname = parts[0];
			var initials = parts.Skip(1)
				.Where(p => p.Length > 0)
				.Select(p => char.ToUpperInvariant(p[0]) + ".")
				.ToArray();

			return initials.Length == 0
				? surname
				: $"{surname} {string.Join(string.Empty, initials)}";
		}

		private static string BuildInitials(string fullName)
		{
			if (string.IsNullOrWhiteSpace(fullName))
			{
				return string.Empty;
			}

			var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			var initials = parts
				.Take(2)
				.Where(p => p.Length > 0)
				.Select(p => char.ToUpperInvariant(p[0]))
				.ToArray();

			return string.Join(string.Empty, initials);
		}

		private void OpenAdminVendingService_Click(object sender, RoutedEventArgs e)
		{
			OpenPage(new AdminPage());
		}
	}
}

