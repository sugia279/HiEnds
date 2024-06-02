using HiEndsApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using HiEndsApp.Repository;
using HiEndsApp.Model;
using System.Windows.Threading;
using HiEndsCore.Models;
using System.ComponentModel;

namespace HiEndsApp.View
{
    /// <summary>
    /// Interaction logic for HiEndsMainUserControl.xaml
    /// </summary>
    public partial class HiEndsMainUserControl : UserControl
    {
        private HiEndsMainUCViewModel vm;
        public HiEndsMainUserControl()
        {
            InitializeComponent();
            vm = (DataContext as HiEndsMainUCViewModel);
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }
        
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Adding 1 to make the row count start at 1 instead of 0            
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
            DataGrid dt = (sender as DataGrid);
            if (e.Row.GetIndex() - 1 > -1)
            {
                DataGridRow prevRow = (dt.ItemContainerGenerator.ContainerFromIndex(e.Row.GetIndex() - 1) as DataGridRow);
                if (prevRow != null)
                {
                    prevRow.Header = e.Row.GetIndex().ToString();
                    dt.ScrollIntoView(e.Row);
                }
            }

            DataGridRow nextRow = (dt.ItemContainerGenerator.ContainerFromIndex(e.Row.GetIndex() + 1) as DataGridRow);
            if (nextRow != null)
            {
                nextRow.Header = (e.Row.GetIndex() + 2).ToString();
                dt.ScrollIntoView(e.Row);
            }
        }

        private void Data_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.SelectedRows.Clear();
            for (int i=0; i < SourceData.SelectedItems.Count; i++)
            {
                if (SourceData.SelectedItems[i] is DataRowView)
                {
                    var rowView = (DataRowView)SourceData.SelectedItems[i];
                    vm.SelectedRows.Add(rowView.Row);
                    if (i == SourceData.SelectedItems.Count - 1)
                    {
                        vm.SelectedRow = rowView.Row;
                        if (rowView.Row.RowState != DataRowState.Detached)
                        {
                            if ((string)rowView.Row[ConstString.RunStatus] == nameof(RunStatus.Running))
                                TxtLogContent.ScrollToEnd();
                        }
                        SourceData.ScrollIntoView(SourceData.SelectedItem);
                    }
                    SelectedRowCount.Text = vm.SelectedRows.Count.ToString();
                }
            }
        }


        private void SourceData_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();

            if (header == ConstString.RunResultFile) e.Cancel = true;
            if (header == ConstString.RunComments) e.Cancel = true;

            // Replace all underscores with two underscores, to prevent AccessKey handling
            e.Column.Header = header.Replace("_", "__");

            if (!e.Column.Header.ToString().StartsWith("@") || e.Column.Header.ToString().Equals(ConstString.RowId))
                e.Column.IsReadOnly = false;
            else
                e.Column.IsReadOnly = true;
        }
        
        private void ClearQuery_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(SourceData.ItemsSource);

            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
            }
        }

        private void SourceData_OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            // Find the TreeViewItem that was clicked
            if ((sender as DataGrid)?.SelectedItem is DataRowView)
            {                
                // Find the associated data item
                vm?.CellEditEndingCommand.Execute(e);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MoreSettingRow.Height.Value == 0)
            {
                SettingRow.Height = new GridLength(200); 
                MoreSettingRow.Height = new GridLength(170);
            }
            else
            {
                SettingRow.Height = new GridLength(50);
                MoreSettingRow.Height = new GridLength(0);
            }
        }
    }
}
