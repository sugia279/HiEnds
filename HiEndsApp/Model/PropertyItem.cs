using HiEndsCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HiEndsApp.Model
{
    public class PropertyItemsView : NotifyPropertyChanged
    {
        public ObservableCollection<PropertyItem> PropertyItemCollection { get; set; }

        private ICollectionView _propertyItemsView;

        public ICollectionView PropertyCollectionView
        {
            get => _propertyItemsView;
            set
            {
                _propertyItemsView = value;
                OnPropertyChanged();
            }
        }

        private SourceProject _sourceProject;

        public PropertyItemsView(SourceProject sourceProject)
        {
            if (sourceProject == null) return;
            _sourceProject = sourceProject;
            LoadProjectProperties(sourceProject);
            PropertyCollectionView = CollectionViewSource.GetDefaultView(PropertyItemCollection);
            PropertyGroupDescription groupByCategory = new PropertyGroupDescription(nameof(PropertyItem.Category));
            PropertyCollectionView.GroupDescriptions.Add(groupByCategory);
            //PropertyGroupDescription groupBySubCategory = new PropertyGroupDescription(nameof(PropertyItem.SubCategory));
            //PropertyCollectionView.GroupDescriptions.Add(groupBySubCategory);
            // Attach PropertyChanged event handler to each item
            foreach (var item in PropertyItemCollection)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                // Update your underlying data source here
                // For example, if you need to update a database or another collection
                // You can access the changed item from 'sender'
                // and its new value from 'Value' property
                PropertyItem changedItem = sender as PropertyItem;
                
                if (changedItem?.Category == nameof(SourceProject.Extract))
                {

                }
                else if (changedItem?.Category == nameof(SourceProject.Run))
                {
                    switch (changedItem.Name)
                    {
                        case nameof(SourceProject.Run.OutputFile):
                            _sourceProject.Run.OutputFile = changedItem.Value.ToString();
                            break;
                        case nameof(SourceProject.Run.OutputFolder):
                            _sourceProject.Run.OutputFolder = changedItem.Value.ToString();
                            break;
                        case nameof(SourceProject.Run.RunThreadNumber):
                            _sourceProject.Run.RunThreadNumber = int.Parse(changedItem.Value.ToString());
                            break;
                    }
                }
                else if (changedItem?.Category == nameof(SourceProject.Vars))
                {

                }
                // Update your data source with the new value
            }
        }

        private void LoadProjectProperties(SourceProject sourceProject)
        {
            if (sourceProject == null) return;

            PropertyItemCollection = new ObservableCollection<PropertyItem>()
            {
                new() { Name = nameof(sourceProject.Extract.TemplateFile), Value = sourceProject.Extract?.TemplateFile??string.Empty, Category = nameof(sourceProject.Extract), IsReadOnly = true },
                new() { Name = nameof(sourceProject.Extract.Query), Value = sourceProject.Extract?.Query??string.Empty, Category = nameof(sourceProject.Extract), IsReadOnly = true},
                new() { Name = nameof(sourceProject.Run.OutputFolder), Value = sourceProject.Run?.OutputFolder??string.Empty, Category = nameof(sourceProject.Run) },
                new() { Name = nameof(sourceProject.Run.OutputFile), Value = sourceProject.Run?.OutputFile??string.Empty, Category = nameof(sourceProject.Run) },
                new() { Name = nameof(sourceProject.Run.RunThreadNumber), Value = sourceProject.Run?.RunThreadNumber??1, Category = nameof(sourceProject.Run) }
            };
            if (sourceProject.Vars != null)
            {
                foreach (var var in sourceProject.Vars)
                {
                    PropertyItemCollection.Add(new()
                        { Name = var.Key, Value = var.Value, Category = nameof(sourceProject.Vars) });
                }
            }
        }
    }
    
    public class PropertyItem : NotifyPropertyChanged
    {
        private string _name;
        private object _value;
        private bool _isReadOnly;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public object Value
        {
            get => _value;
            set { this._value = value; OnPropertyChanged(); }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(); }
        }

        public string Category { get; set; }
        public string SubCategory { get; set; }

    }
}
