﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DocHound.Annotations;
using DocHound.Razor;
using MarkdownMonster;
using Newtonsoft.Json;
using Westwind.Utilities;

namespace DocHound.Model
{
        
    public class DocProject : INotifyPropertyChanged
    {
        

        /// <summary>
        ///  The descriptive name of the project
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }
        private string _title;


        
        /// <summary>
        /// An optional owner/company name that owns this project
        /// </summary>
        public string Owner
        {
            get { return _owner; }
            set
            {
                if (value == _owner) return;
                _owner = value;
                OnPropertyChanged();
            }
        }
        private string _owner;


        /// <summary>
        /// The project's base URL from which files are loaded
        /// </summary>
        public string BaseUrl
        {
            get { return _baseUrl; }
            set
            {
                if (value == _baseUrl) return;
                _baseUrl = value;
                OnPropertyChanged();
            }
        }    
        private string _baseUrl = "http://markdownmonster.west-wind.com/docs/";

        [JsonIgnore]
        public string Filename
        {
            get { return _filename; }
            set
            {
                if (value == _filename) return;
                _filename = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProjectDirectory));
                OnPropertyChanged(nameof(OutputDirectory));
            }
        }
        private string _filename;


        [JsonIgnore]
        /// <summary>
        /// The base folder where the project is located.
        /// Used as the base folder to find related Markdown content files
        /// </summary>
        public string ProjectDirectory {
            get
            {
                if (!string.IsNullOrEmpty(Filename))
                    return Path.GetDirectoryName(Filename);

                return null;
            }
        }

        /// <summary>
        /// Folder where the actual HTML is generated.
        /// </summary>
        [JsonIgnore]
        public string OutputDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(Filename))
                    return null;

                return Path.Combine(ProjectDirectory, "wwwroot");
            }
        }

        /// <summary>
        /// The base folder where the project is located.
        /// Used as the base folder to find related Markdown content files
        /// </summary>
        public string Location { get; set; }

       

        /// <summary>
        /// Determines how HTML is rendered when rendering topics
        /// </summary>
        public HtmlRenderModes ActiveRenderMode { get; set; } = HtmlRenderModes.Html;

        /// <summary>
        /// Language of the help file
        /// </summary>
        public string Language
        {
            get { return _language; }
            set
            {
                if (value == _language) return;
                _language = value;
                OnPropertyChanged();
            }
        }
        private string _language = "en-US";

        
        /// <summary>
        /// Help Builder Version used to create this file
        /// </summary>
        public string Version
        {
            get { return _version; }
            set
            {
                if (value == _version) return;
                _version = value;
                OnPropertyChanged();
            }
        }
        private string _version;


        /// <summary>
        /// Topic list
        /// </summary>
        public ObservableCollection<DocTopic> Topics
        {
            get { return _topics; }
            set
            {
                if (Equals(value, _topics)) return;
                _topics = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<DocTopic> _topics = new ObservableCollection<DocTopic>();




        /// <summary>
        /// A list of custom topics that are available in this help file
        /// </summary>
        public Dictionary<string, string> CustomFields { get; set; }

        public Dictionary<string,string> TopicTypes { get; set; }


        [JsonIgnore]
        public DocTopic Topic { get; set; }

        
        [JsonIgnore]
        /// <summary>
        /// Template Renderer used to render topic templates
        /// </summary>
        internal RazorTemplates TemplateRenderer
        {
            get
            {
                if (_templateRender == null)
                {
                    _templateRender = new RazorTemplates();
                    _templateRender.StartRazorHost(ProjectDirectory);
                }
                return _templateRender;
            }
            set
            {
                if (value == null)                
                    _templateRender?.StopRazorHost();               
                _templateRender = value;
            }
        }
        private RazorTemplates _templateRender;


        public DocProject()
        {
            TopicTypes = new Dictionary<string, string>
            {
                { "index", "Top level topic for the documentation." },
                { "header", "Header topic for sub-topics" },
                { "topic", "Generic topic" },
                { "whatsnew", "What's new" },
                { "weblink", "External link" },
                { "classheader", "Class Header" },
                { "interface", "Interface" },
                { "namespace", "Namespace" },
                { "classmethod", "Class method" },
                {"classproperty", "Class properyty" },
                {"classfield", "Class field" },
                {"classevent", "Class event" },
                { "classconstructor","Class constructor" },
                {"enum", "Enumeration" },
                {"delegate", "Delegate" },
                { "webservice", "Web Service" },
                {"database", "Database" },
                { "datacolumn", "Data columns" },
                { "datafunction", "Data function" },
                { "datastoredproc", "Data stored procedure" },
                {"datatable", "dataview" }
            };
        }

        public DocProject(string filename = null)
        {
            if (filename == null)
                filename = "DocumentationProject.json";
            Filename = filename;            
        }

        #region Loading and Saving of Topics

        /// <summary>
        /// Loads a topic by id
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public DocTopic LoadTopic(string topicId)
        {
            Topic = Topics.FirstOrDefault(t => t.Id == topicId);
            if (Topic == null)
            {
                SetError("Topic not found.");
                return null;
            }
            Topic.Project = this;

            if (!Topic.LoadTopicFile()) // load disk content
            {
                SetError("Topic body content not found.");
                return null;
            }

            return Topic;
        }


        /// <summary>
        /// Loads a topic by its topic title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public DocTopic LoadByTitle(string title)
        {
            Topic = Topics.FirstOrDefault(t => !string.IsNullOrEmpty(t.Title) && t.Title.ToLower() == title.ToLower());
            if (Topic == null)
            {
                SetError("Topic not found.");
                Topic = null;
                return null;
            }
            Topic.Project = this;

            if (!Topic.LoadTopicFile())
            {
                SetError("Topic body content not found.");
                return null;
            }

            return Topic;
        }


        /// <summary>
        /// Loads a topic by its topic slug
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public DocTopic LoadBySlug(string slug)
        {
            if (slug.StartsWith("_"))
                slug = slug.Substring(1);

            Topic = Topics.FirstOrDefault(t => t.Slug.ToLower() == slug.ToLower());
            if (Topic == null)
            {
                SetError("Topic not found.");
                Topic = null;
                return null;
            }
            Topic.Project = this;

            if (!Topic.LoadTopicFile())
            {
                SetError("Topic body content not found.");
                return null;
            }

            return Topic;
        }


        /// <summary>
        /// Removes an item from collection
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public void DeleteTopic(DocTopic topic)
        {
            if (topic == null)
                return;

            var children = Topics.Where(t => t.ParentId == topic.Id).ToList();
            for (int i = children.Count-1; i > -1; i--)
            {
                var childTopic = children[i];
                DeleteTopic(childTopic);
            }
            
            Topics.Remove(topic);
            topic = null;            
        }

        /// <summary>
        /// Creates a new topic and assigns it to the Topic property.
        /// </summary>
        /// <returns></returns>
        public DocTopic NewTopic()
        {
            Topic = new DocTopic(this);
            return Topic;
        }


        /// <summary>
        /// Saves a topic safely into the topic list.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public bool SaveTopic(DocTopic topic = null)
        {
            if (topic == null)
                topic = Topic;
            if (topic == null)
                return false;

            var loadTopic = Topics.FirstOrDefault(t=> t.Id == topic.Id );

            //if (loadTopic == topic)
            //{                
            //    var result = topic.SaveTopicFile();                
            //}

            if (loadTopic == null)
            {
                using(var updateLock = new TopicFileUpdateLock())
                {
                    Topics.Add(topic);
                    return topic.SaveTopicFile();                    
                }
            }
            if (loadTopic.Id == topic.Id)
            {
                for (int i = 0; i < Topics.Count; i++)
                {
                    var tpc = Topics[i];
                    if (tpc == null)
                    {
                        using (var updateLock = new TopicFileUpdateLock())
                        {
                            Topics.RemoveAt(i);
                        }
                        continue;
                    }
                    if (Topics[i].Id == topic.Id)
                    {
                        using (var updateLock = new TopicFileUpdateLock())
                        {
                            Topics[i] = topic;
                            return topic.SaveTopicFile();
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves a topic but doesn't load it into Active Topic 
        /// based on a topic id, slug or html link
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns>topic or null</returns>
        public DocTopic GetTopic(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            DocTopic topic = null;
            if (id.StartsWith("_"))
            {
                string strippedSlugId = id;
                if (!string.IsNullOrEmpty(strippedSlugId) && strippedSlugId.StartsWith("_"))
                    strippedSlugId = id.Substring(1);

                topic = Topics                    
                    .FirstOrDefault(t => t.Id == id ||
                                         t.Slug == id ||
                                         t.Slug == strippedSlugId);
                return topic;
            }
            if (id.StartsWith("msdn:"))
            {
                // for now just return as is
                throw new NotSupportedException();
            }
            else if (id.StartsWith("sig:"))
            {
                throw new NotSupportedException();
            }

            // Must be topic title
            topic = Topics                
                .FirstOrDefault(t => t.Title != null && t.Title.ToLower() == id.ToLower() ||
                                     t.Slug != null && t.Slug.ToLower() == id.ToLower());


            return topic;
        }

        /// <summary>
        /// Updates a topic from a markdown document
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="topic"></param>
        public void UpdateTopicFromMarkdown(MarkdownDocument doc, DocTopic topic)
        {
            var fileTopic = new DocTopic();
            fileTopic.Project = this;
            fileTopic.LoadTopicFile(doc.Filename);

            topic.Body = fileTopic.Body;
            if (!string.IsNullOrEmpty(topic.Title))
                topic.Title = fileTopic.Title;
            
            topic.Type = fileTopic.Type;
            if (!string.IsNullOrEmpty(fileTopic.Slug))
                topic.Slug = fileTopic.Slug;

            if (!string.IsNullOrEmpty(fileTopic.Link))
                topic.Link = fileTopic.Link;

            topic.SaveTopicFile();
        }


        /// <summary>
        /// Sets IsHidden and IsExpanded flags on topics in the tree 
        /// depending on a search match.
        /// </summary>
        /// <param name="topics"></param>
        /// <param name="searchPhrase"></param>
        /// <param name="nonRecursive"></param>
        public void FilterTopicsInTree(ObservableCollection<DocTopic> topics, string searchPhrase, bool nonRecursive)
        {
            if (topics == null || topics.Count < 1)
                return;
            
            foreach (var topic in topics)
            {
                if (string.IsNullOrEmpty(searchPhrase))
                    topic.IsHidden = false;
                else if (topic.Title.IndexOf(searchPhrase, StringComparison.CurrentCultureIgnoreCase) < 0)
                    topic.IsHidden = true;
                else
                    topic.IsHidden = false;

                // Make parent topics visible and expanded
                if (topic.IsHidden == false)
                {
                    var parent = topic.Parent;
                    while (parent != null)
                    {
                        parent.IsHidden = false;
                        parent.IsExpanded = true;
                        parent = parent.Parent;
                    }
                }

                if (!nonRecursive)
                    FilterTopicsInTree(topic.Topics, searchPhrase, false);
            }
        }
        #endregion


        #region Output Generation

        public void GenerateHtmlOutput()
        {
            
        }

        public void GenerateTableOfContents()
        {
            
        }

        #endregion

        #region Topic Names and Links

        /// <summary>
        /// Returns a topic A HTML link depending on which mode you're running in
        /// </summary>
        /// <param name="displayText"></param>
        /// <param name="id"></param>
        /// <param name="anchor"></param>
        /// <param name="attributes"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string GetTopicLink(string displayText, 
            string id,
            string anchor = null, 
            string attributes = null,             
            HtmlRenderModes mode = HtmlRenderModes.None)
        {            
            string topicFile = GetTopicFilename(id);
            if (string.IsNullOrEmpty(topicFile))
                return null;

            string anchorString = (string.IsNullOrEmpty(anchor) ? "" : "#" + anchor);
            string linkText = HtmlUtils.HtmlEncode(displayText);
            string link = null;

            if (mode == HtmlRenderModes.None)
                mode = ActiveRenderMode;

            // Plain HTML
            if (mode == HtmlRenderModes.Html )
                link = $"<a href='{StringUtils.UrlEncode(topicFile)}' {anchorString} {attributes}>{linkText}</a>";
            // Preview Mode
            else if (mode == HtmlRenderModes.Preview)
                link = $"<a href='dm://Topic/{StringUtils.UrlEncode(id)}' {anchorString} {attributes}>{linkText}</a>";
            if (mode == HtmlRenderModes.Print)
                link = $"<a href='#{StringUtils.UrlEncode(topicFile)}' {anchorString} {attributes}>{linkText}</a>";

            return link;
        }

        /// <summary>
        /// Returns the file name of a specific topic
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTopicFilename(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            
            string topicId = null;
            if (id.StartsWith("_"))
            {
                string strippedSlugId = id;
                if (!string.IsNullOrEmpty(strippedSlugId) && strippedSlugId.StartsWith("_"))
                    strippedSlugId = id.Substring(1);

                topicId = Topics
                    .Where(t => t.Id == id ||
                                t.Slug == id ||
                                t.Slug == strippedSlugId)
                    .Select(t => t.Slug)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(topicId))
                    return topicId;
            }
            else if (id.StartsWith("msdn:"))
            {
                // for now just return as is
                return id;
            }
            else if (id.StartsWith("sig:"))
            {
                return id;
            }

            // Must be topic title
            topicId = Topics
                .Where(t => t.Title != null && t.Title.ToLower() == id.ToLower())
                .Select(t => t.Slug)
                .FirstOrDefault();

            if (topicId == null)
                topicId = id;

            return "_" + topicId + ".html";
        }

        #endregion


        #region Load and Save to File

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static DocProject LoadProject(string filename)
        {
            var project = DocProjectManager.Current.LoadProject(filename);
            if (project == null)
                return null;

            foreach (var topic in project.Topics)
            {
                topic.Project = project;
            }
            
            return project;
        }

        /// <summary>
        /// Creates a new project based on given parameters
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static DocProject CreateProject(DocProjectCreator creator)
        {
            return DocProjectManager.CreateProject(creator);
        }

        public bool SaveProject(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
                filename = Filename;
            
            if (!DocProjectManager.Current.SaveProject(this, filename))
            {
                SetError(DocProjectManager.Current.ErrorMessage);
                return false;
            }

            return true;
        }
        #endregion

        #region Topic List from Flat List


        /// <summary>
        /// Gets a flat list of topics. This is faster and more efficient
        /// to work with.
        /// </summary>
        /// <returns></returns>
        public List<DocTopic> GetTopics()
        {            
            return Topics
                .OrderBy(t=> t.ParentId)
                .ThenByDescending(t => t.SortOrder)
                .ThenBy(t => t.Type)
                .ThenBy(t => t.Title)
                .ToList();
        }



         /// <summary>
        /// Converts a flat topic list as a tree of nested topics
        /// </summary>
        /// <remarks>
        /// Assumes this is the root collection - ie. parent Id and Parent get set to empty
        /// </remarks>
        /// <param name="topicList"></param>
        /// <returns></returns>
        public void GetTopicTreeFromFlatList(ObservableCollection<DocTopic> topics)
        {
            if (topics == null)
                return;

            var list = new ObservableCollection<DocTopic>();

            // need to copy so we can clear the root collection
            var allTopics = new ObservableCollection<DocTopic>(topics);
            
            var topicsList = topics.Where(t => string.IsNullOrEmpty(t.ParentId))
                .OrderByDescending(t => t.SortOrder)
                .ThenByDescending(t => t.Type)
                .ThenBy(t => t.Title)
                .ToList();

            topics.Clear();
            foreach (var top in topicsList)
            {
                GetChildTopicsForTopicFromFlatList(allTopics, top);
                
                top.ParentId = null;
                top.Parent = null;
                top.Project = this;
                topics.Add(top);                
            }
                        
        }


        /// <summary>
        /// Recursively fills all topics with subtopics
        /// </summary>
        /// <param name="topics"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        public void GetChildTopicsForTopicFromFlatList(ObservableCollection<DocTopic> topics, DocTopic topic)
        {

            if (topics == null)
                topics = new ObservableCollection<DocTopic>();

            var children = topics.Where(t => t.ParentId == topic.Id)
                .OrderByDescending(t => t.SortOrder)
                .ThenBy(t => t.Type)
                .ThenBy(t => t.Title).ToList();

            if (topic.Topics != null)
                topic.Topics.Clear();
            else
                topic.Topics = new ObservableCollection<DocTopic>();
                    
            foreach (var childTopic in children)
            {                
                childTopic.Parent = topic;
                childTopic.ParentId = topic.Id;
                childTopic.Project = this;
                topic.Topics.Add(childTopic);
            }            
        }

        #endregion

        #region Fix up Tree to ensure ids and parents are set

        /// <summary>
        /// Fixes up a topic tree so that parent, id, parent ID and all other
        /// related dependencies are fixed up properly.
        /// </summary>
        /// <param name="topicList"></param>
        /// <returns></returns>
        public void GetTopicTree(ObservableCollection<DocTopic> topics = null)
        {
            if (topics == null)
                topics = Topics;

            var list = new ObservableCollection<DocTopic>();

            var topicList = topics
                .OrderByDescending(t => t.SortOrder)
                .ThenBy(t => t.Type)
                .ThenBy(t => t.Title).ToList();

            topics.Clear();
            foreach (var top in topicList)
            {
                GetChildTopicsForTopic(top);
                top.Project = this;
                topics.Add(top);                
            }                       
        }


        /// <summary>
        /// Recursively fills all topics with subtopics 
        /// reorders them and assigns the appropriate 
        /// hierarchical ids and dependencies
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="topicList"></param>
        /// <returns></returns>
        public void GetChildTopicsForTopic(DocTopic topic)
        {
            if (topic.Topics == null)
            {
                topic.Topics = new ObservableCollection<DocTopic>();
                return;
            }

            var children = topic.Topics
                .OrderByDescending(t => t.SortOrder)
                .ThenBy(t => t.Type)
                .ThenBy(t => t.Title).ToList();

            foreach (var childTopic in children)
            {
                GetChildTopicsForTopic(childTopic);
                childTopic.Parent = topic;
                childTopic.ParentId = topic.Id;
                childTopic.Project = this;
            }
            
        }

        /// <summary>
        /// Finds a topic in the tree
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="rootTopics"></param>
        /// <returns></returns>
        public DocTopic FindTopicInTree( DocTopic topic, ObservableCollection<DocTopic> rootTopics = null)
        {
            if (rootTopics == null)
                rootTopics =Topics;
            
            if (rootTopics == null)
                    return null;

            foreach (var top in rootTopics)
            {
                if (top == topic)
                    return topic;

                var foundTopic = FindTopicInTree(top, top.Topics);
                if (foundTopic != null)
                    return topic;
            }

            return null;
        }

        public void WriteTopicTree(ObservableCollection<DocTopic> topics, int level, StringBuilder sb)
        {
                if (topics == null || topics.Count < 1)
                    return;

                sb.AppendLine("    " + level);
                foreach (var topic in topics)
                {
                    sb.AppendLine(new string(' ', level * 2) +
                                      $"{topic.Title} - {topic.Id} {topic.ParentId} {topic.Project != null}");

                    WriteTopicTree(topic.Topics, level + 1,sb);
                }            
        }

        public DocTopic FindTopicInFlatTree(DocTopic topic, ObservableCollection<DocTopic> rootTopics = null)
        {
            if (rootTopics == null)
                rootTopics = Topics;

            return rootTopics?.FirstOrDefault(t => t.Id == topic.Id);
        }


        #endregion




        #region Error Handling
        [JsonIgnore]
        public string ErrorMessage { get; set; }

        protected void SetError()
        {
            SetError("CLEAR");
        }

        protected void SetError(string message)
        {
            if (message == null || message == "CLEAR")
            {
                ErrorMessage = string.Empty;
                return;
            }
            ErrorMessage += message;
        }

        protected void SetError(Exception ex, bool checkInner = false)
        {
            if (ex == null)
                ErrorMessage = string.Empty;

            Exception e = ex;
            if (checkInner)
                e = e.GetBaseException();

            ErrorMessage = e.Message;
        }
        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        
    }

    public enum HtmlRenderModes
    {
        Html,
        Preview, 
        Print,
        None
    }
}