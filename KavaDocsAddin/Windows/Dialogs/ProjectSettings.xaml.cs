﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using DocHound.Annotations;
using DocHound.Model;
using KavaDocsAddin;
using KavaDocsAddin.Controls;
using MarkdownMonster;
using MarkdownMonster.Windows;

namespace DocHound.Windows.Dialogs
{
    /// <summary>
    /// Interaction logic for NewTopicDialog.xaml
    /// </summary>
    public partial class ProjectSettingsDialog : INotifyPropertyChanged        
    {
        public MarkdownMonster.MainWindow Window
        {
            get { return _window; }
            set
            {
                if (Equals(value, _window)) return;
                _window = value;
                OnPropertyChanged();
            }
        }
        private MarkdownMonster.MainWindow _window;


        public KavaDocsModel AppModel
        {
            get { return _appModel; }
            set
            {                
                if (_appModel == value) return;
                _appModel = value;
                OnPropertyChanged(nameof(AppModel));
            }
        }
        private KavaDocsModel _appModel = kavaUi.AddinModel;
      

        public DocProject Project { get; set; }

        
        #region Initialization

        public ProjectSettingsDialog(MainWindow window)
        {
            InitializeComponent();
            mmApp.SetThemeWindowOverride(this);

            Owner = window;
            Window = window;
            AppModel = kavaUi.AddinModel;

            Project = AppModel.ActiveProject;

            DataContext = this;
        }
        
        #endregion

        

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void Button_UpdateProjectTemplate(object sender, RoutedEventArgs e)
        {
            if (Project == null)
                return;
            
            var creator = new DocProjectCreator();
            if (!creator.CopyProjectAssets(Project))
            {
                AppModel.Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Warning, Colors.Firebrick);
                AppModel.Window.ShowStatus(creator.ErrorMessage, 6000);
            }
            else
            {
                UriToCachedImageConverter.ClearCachedImages();
                AppModel.Window.ShowStatus("Templates have been updated.", 6000);
            }
        }
    }
}
