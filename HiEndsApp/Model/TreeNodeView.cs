using HiEndsCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace HiEndsApp.Model
{
    public class TreeNodeView : NotifyPropertyChanged
    {
        #region Data
        private readonly Dictionary<Type, string[]> _iconPaths = new Dictionary<Type, string[]> 
        {
            { typeof(DirectoryInfo), new string[2] {@"..\Images\FolderClose.png", @"..\Images\FolderOpen.png" } },
            { typeof(HiProjectFile), new string[2] {@"..\Images\HiProject.png", @"..\Images\HiProject.png" } },
            { typeof(HiTemplateFile), new string[2] { @"..\Images\HiTemplate.png", @"..\Images\HiTemplate.png" } }
            //{ typeof(TestSuite), new string[2] { @"..\Themes\Images\testsuitecollapse16.png", @"..\Themes\Images\testsuitecollapse16.png" } },
            //{ typeof(TestCase), new string[2] { @"..\Themes\Images\testcase16.png", @"..\Themes\Images\testcase16.png" } },
            //{ typeof(TestStep), new string[2] { @"..\Themes\Images\teststep.png", @"..\Themes\Images\teststep.png" } },
            //{ typeof(KeywordClass), new string[2] { @"..\Themes\Images\KeywordClass.png", @"..\Themes\Images\KeywordClass.png" } },
            //{ typeof(Keyword), new string[2] { @"..\Themes\Images\Keyword.png", @"..\Themes\Images\String.png" } },
            //{ typeof(TestClass), new string[2] { @"..\Themes\Images\testcase16.png", @"..\Themes\Images\testcase16.png" } }            
        };
        #endregion // Data
        #region Properties

        public object ObjectReference { get; set; }

        public string IconCollapsePath { get; set; } = null!;

        public string IconExpandPath { get; set; } = null!;

        private string _label;
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                OnPropertyChanged();
            }
        }

        private string? _value;
        public string? Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public string IconPath
        {
            get
            {
                if (this.IsExpanded)
                {
                    return IconExpandPath;
                }
                return IconCollapsePath;
            }
        }        
        
        static readonly TreeNodeView DummyChild = new TreeNodeView();

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<TreeNodeView> Children { get; set; }

        public TreeNodeView Parent { get; set; }

        bool _isExpanded;        
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IconPath));
                }

                // Expand all the way up to the root.
                if (_isExpanded && Parent != null)
                    Parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild)
                {
                    this.Children.Remove(DummyChild);
                    this.LoadChildren();
                }
            }
        }

        bool _isEditing;

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (value != _isEditing)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }

        bool _isSelected;
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        private bool HasDummyChild => this.Children.Count == 1 && this.Children[0] == DummyChild;

        #endregion
        
        #region Constructors

        // This is used to create the DummyChild instance.
        public TreeNodeView()
        {
        }

        protected TreeNodeView(TreeNodeView parent, bool lazyLoadChildren)
        {
            Parent = parent;

            Children = new ObservableCollection<TreeNodeView>();

            if (lazyLoadChildren)
                Children.Add(DummyChild);
        }

        public TreeNodeView(Object item, string label, TreeNodeView parent, bool lazyLoadChildren)
        {
            SetupTreeNodeView(item, label, parent, lazyLoadChildren);
        }
        

        private void SetupTreeNodeView(Object item, string label, TreeNodeView parent, bool lazyLoadChildren)
        {
            Parent = parent;
            Children = new ObservableCollection<TreeNodeView>();
            ObjectReference = item;
            if (_iconPaths.ContainsKey(item.GetType()))
            {
                IconCollapsePath = _iconPaths[item.GetType()][0];
                IconExpandPath = _iconPaths[item.GetType()][1];
            }
            _label = label;

            if(ObjectReference is string reference)
                _value = reference;
            if (ObjectReference is int i)
                _value = i.ToString();

            if (lazyLoadChildren)
                Children.Add(DummyChild);
        }
        #endregion

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }

        public TreeNodeView FindSelectedChild()
        {
            foreach (TreeNodeView item in Children)
            {
                if (IsSelected)
                {
                    return this;
                }
                TreeNodeView found = item.FindSelectedChild();                
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        public TreeNodeView FindChild(string searchText)
        {
            foreach (TreeNodeView item in Children)
            {
                item.IsExpanded = true;
                if (item.Label.ToLower().Contains(searchText.ToLower()))
                {
                    return item;
                }
                TreeNodeView found = item.FindChild(searchText);
                if(found != null)
                {
                    return found;
                }
            }
            return null;
        }
    }
}
