using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using DocHound.Annotations;
using MarkdownMonster.Utilities;


namespace DocHound.Model
{

    public class TopicState : INotifyPropertyChanged
    {
        private DocTopic Topic;
        

        public TopicState(DocTopic topic)
        {
            Topic = topic;
            BodyState = new BodyState();
        }

        public BodyState BodyState { get; set; }
       
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected) return;
               _isSelected = value;
                OnPropertyChanged();
            }
        }
        private bool _isSelected;


        public bool IsHidden
        {
            get => _isHidden;
            set
            {
                if (value == _isHidden) return;
                _isHidden = value;
                OnPropertyChanged();
            }
        }
        private bool _isHidden;


        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                if (value == _isEditing) return;
                _isEditing = value;
                OnPropertyChanged();
            }
        }
        private bool _isEditing;

        
        public bool IsDirty
        {
            get { return _IsDirty; }
            set
            {
                if (value == _IsDirty) return;
                _IsDirty = value;
                OnPropertyChanged(nameof(IsDirty));
            }
        }
        private bool _IsDirty = false;


        public bool IsPreview { get; set; }


        public string ImageFilename
        {
            get
            {
                string outfolder = Topic.Project.OutputDirectory;

                if (string.IsNullOrEmpty(Topic.Project.OutputDirectory))
                    return null;

                var type = Topic.DisplayType;
                if (type == null)
                {
                    if (Topic.Topics != null && Topic.Topics.Count > 0)
                        type = "header";
                    else
                        type = "topic";

                }

                return Path.Combine(Topic.Project.OutputDirectory, "icons", type.ToLower() + ".png");
            }
        }

        public string OpenImageFilename
        {
            get
            {
                string outfolder = Topic.Project.OutputDirectory;

                if (string.IsNullOrEmpty(Topic.Project.OutputDirectory))
                    return null;

                var type = Topic.DisplayType;
                if (type == null)
                {
                    if (Topic.Topics != null && Topic.Topics.Count > 0)
                        type = "header";
                    else
                        type = "topic";
                }
                if( type == "header")
                    return Path.Combine(Topic.Project.OutputDirectory, "icons", type.ToLower() + "_open.png");

                return Path.Combine(Topic.Project.OutputDirectory, "icons", type.ToLower() + ".png");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BodyState
    {
        public string OriginalText { get; set; }

        public bool IsDirty { get; set; }
    }
}