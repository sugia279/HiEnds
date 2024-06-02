using System;
using System.IO;
using HiEndsApp.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HiEndsApp.Model;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;
using System.Windows.Threading;

namespace HiEndsApp.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadSyntaxHighlighting();
            DataContext = new MainWindowViewModel();
            //cbHiEndsExplorer.IsChecked = true;
        }

        private void LoadSyntaxHighlighting()
        {
            // Load .xshd file
            using (XmlTextReader reader = new XmlTextReader("YamlSyntaxHighlight.xshd"))
            {
                IHighlightingDefinition customHighlighting;
                customHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

                // Assign to your AvalonEdit TextEditor
                YamlTextEditor.SyntaxHighlighting = customHighlighting;
            }
        }

        private void TestNodeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Find the TreeViewItem that was clicked
            if ((sender as TreeView)?.SelectedItem is TreeNodeView treeNode)
            {
                var vm = (DataContext as MainWindowViewModel);
                // Find the associated data item
                vm?.OpenFileCommand.Execute(treeNode);
            }
        }

        private void TreeNodeView_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar? toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }
        
        private void YamlTextEditor_OnTextChanged(object? sender, EventArgs e)
        {
            var vm = (DataContext as MainWindowViewModel);
            // Find the associated data item
            vm?.YamlTextChangedCommand.Execute(null);
        }

        private void TreeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TreeView treeView && treeView.SelectedItem is TreeNodeView item)
            {
                var vm = (DataContext as MainWindowViewModel);
                //if (e.Key == Key.F2)
                //{
                //    item.IsSelected = true;
                //    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => item.IsEditing = true));
                //}

                if (e.Key == Key.Delete)
                {
                    vm?.DeleteFileCommand.Execute(item);
                }

                if (e.Key == Key.Enter)
                {
                    if (item.IsEditing)
                    {
                        item.IsEditing = false;
                        vm?.AcceptEditNameCommand.Execute(item);
                    }
                    else
                    {
                        if (item.ObjectReference is DirectoryInfo)
                        {
                            item.IsExpanded = !item.IsExpanded;
                        }
                    }
                }
            }
        }
        
        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is TreeNodeView node)
            {
                node.IsEditing = false;

            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if(Col1.Width.Equals(new GridLength(0)))
                Col1.Width = new GridLength(300);
            else
            {
                Col1.Width = new GridLength(0);
            }

        }
    }
}
