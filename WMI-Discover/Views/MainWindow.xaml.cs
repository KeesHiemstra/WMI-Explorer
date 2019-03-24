using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using WMI_Discover.ViewModels;

namespace WMI_Discover
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainModelView ModelView;

		public MainWindow()
		{
			InitializeComponent();

			Title += $" ({System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()})";

			ModelView = new MainModelView(this);
			DataContext = ModelView;
		}

		private void SearchBotton_Click(object sender, RoutedEventArgs e)
		{
			ModelView.UpdateFilterWMIClassNames();
		}

		private void ClearBotton_Click(object sender, RoutedEventArgs e)
		{
			ModelView.ClearFilterSeachPanel();
		}

		private async void WMIClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				string wMIClassName;

				if (((ComboBox)e.Source).SelectedValue == null)
				{
					wMIClassName = string.Empty;
					ModelView.DisableExtraTabs();
					return;
				}
				wMIClassName = ((ComboBox)e.Source).SelectedValue.ToString();
				bool collected = false;

				collected = await ModelView.SelectWMIClassName(wMIClassName);

				if (collected)
				{
					WMIPropertiesDataGrid.ItemsSource = ModelView.WMIProperties;
				}
				else
				{
					WMIPropertiesDataGrid.ItemsSource = null;
				}
			}
			catch
			{
				WMIPropertiesDataGrid.ItemsSource = null;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ModelView.SaveWMIClasses();
		}

		private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			ModelView.SearchTextBoxOnKey(e);
		}

		private void PivotTabItem_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!((bool)e.NewValue))
			{
				if (ModelView == null)
				{
					return;
				}
				ModelView.ActOnPivotTabItem((bool)e.NewValue);
			}
		}
	}
}