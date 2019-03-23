using ITAMLib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

      ModelView = new MainModelView(this);
      DataContext = ModelView;
    }

    private void SearchBotton_Click(object sender, RoutedEventArgs e)
    {
      ModelView.UpdateFilterWMIClassNames();
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
