﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using KavaDocsAddin.Controls;
using DocHound;
using DocHound.Configuration;
using DocHound.Model;
using DocHound.Windows.Dialogs;
using DocHound.Configuration;
using KavaDocsAddin.Core.Configuration;
using MarkdownMonster;
using Microsoft.Win32;
using Westwind.Utilities;

namespace KavaDocsAddin
{
    public class AppCommands
    {
        private AddinModel Model;
        

        public AppCommands(AddinModel model)
        {
            Model = model;
            CreateCommands();

        }

        

        private void CreateCommands()
        {
            Command_OpenProject();
            Command_SaveProject();
            Command_NewProject();
            Command_CloseProject();

            Command_NewTopic();
            Command_DeleteTopic();


            // Generic Edit Toolbar Insertion features
            Command_ToolbarInsertMarkdown();

            // Tools
            Command_Settings();

            // Views
            Command_PreviewBrowser();
            
        }

        #region File Commands

        public CommandBase OpenProjectCommand { get; set; }


        private void Command_OpenProject()
        {
            OpenProjectCommand = new CommandBase((parameter, command) =>
            {
                var fd = new OpenFileDialog
                {
                    DefaultExt = ".md",
                    Filter = "Markdown files (*.kava,*.json)|*.kava;*.json|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = true,
                    RestoreDirectory = true,
                    Multiselect = false,
                    Title = "Open Kava Docs Project"
                };

                if (!string.IsNullOrEmpty(KavaApp.Configuration.LastProjectFile))
                    fd.InitialDirectory = KavaApp.Configuration.LastProjectFile;

                bool? res = null;
                try
                {
                    res = fd.ShowDialog();
                }
                catch (Exception ex)
                {
                    mmApp.Log("Unable to open file.", ex);
                    MessageBox.Show(
                        $@"Unable to open file:\r\n\r\n" + ex.Message,
                        "An error occurred trying to open a file",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (res == null || !res.Value)
                    return;

                Model.OpenProject(fd.FileName);

            }, (parameter, command) =>
            {
                return true;
            });
        }

        public CommandBase SaveProjectCommand { get; set; }

        public void Command_SaveProject()
        {
            SaveProjectCommand = new CommandBase((parameter, command) =>
            {
                Model.ActiveMarkdownEditor?.GetMarkdown();
                Model.ActiveProject?.SaveProject();

                Model.Window.ShowStatus("Project saved.", KavaApp.Configuration.StatusMessageTimeout);
            }, (p, c) => true);
        }

        public CommandBase NewProjectCommand { get; set; }

        public void Command_NewProject()
        {
            NewProjectCommand = new CommandBase((parameter, command) =>
            {
                var dialog = new NewProjectDialog(Model.Window);
                dialog.ShowDialog();                
            }, (p, c) => true);
        }


        public CommandBase CloseProjectCommand { get; set; }

        public void Command_CloseProject()
        {
            CloseProjectCommand = new CommandBase((parameter, command) =>
            {
                string projectName = Model.ActiveProject.Title;
                Model.ActiveProject.SaveProject();
                Model.ActiveProject = null;

                Model.TopicsTree.LoadProject(null);

                Model.Window.ShowStatus("Project " + projectName + " has been closed.",
                    KavaApp.Configuration.StatusMessageTimeout);
            }, (p, c) => true);
        }

        #endregion

        #region Markdown Editor Commands

        public CommandBase ToolbarInsertMarkdownCommand { get; set; }

        public void Command_ToolbarInsertMarkdown()
        {
            ToolbarInsertMarkdownCommand = new CommandBase((parameter, command) =>
            {
                string action = parameter as string;
                Model.ActiveMarkdownEditor?.ProcessEditorUpdateCommand(action);
            }, (p, c) => true);
        }
        #endregion

        #region Tool Commands
        public CommandBase SettingsCommand { get; set; }

        public void Command_Settings()
        {
            SettingsCommand = new CommandBase((parameter, command) =>
            {
                ShellUtils.GoUrl(Path.Combine(mmApp.Configuration.CommonFolder, "KavaDocsAddin.json"));
            }, (p, c) => true);
        }
        #endregion




        #region Topic Commands

        public CommandBase NewTopicCommand { get; set; }

        public void Command_NewTopic()
        {
            NewTopicCommand = new CommandBase((parameter, command) =>
            {
                var newTopic = new NewTopicDialog(Model.Window);
                newTopic.ShowDialog();
            }, (p, c) => true);
        }


        public CommandBase DeleteTopicCommand { get; set; }

        public void Command_DeleteTopic()
        {
            DeleteTopicCommand = new CommandBase((parameter, command) =>
            {
                var topic = Model.ActiveTopic;
                if (topic == null)
                    return;
                if (MessageBox.Show($"You are about to delete topic\r\n\r\n{topic.Title}\r\n\r\nAre you sure?",
                        "Delete Topic",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                    return;                


                Model.ActiveProject.DeleteTopic(topic);

                // select previous topic or parent
                if (!string.IsNullOrEmpty(topic.ParentId))
                {
                    var parent = Model.ActiveProject.Topics.FirstOrDefault(t => t.Id == topic.ParentId);
                    if (parent != null)
                    {
                        int topicIndex = parent.Topics.IndexOf(topic);
                        if (topicIndex == 0)
                            parent.TopicState.IsSelected = true;
                        else
                            parent.Topics[topicIndex - 1].TopicState.IsSelected = true;
                    }
                }


                // reload the tree - slowish but easiest
                Model.TopicsTree.LoadProject(Model.ActiveProject);                
            }, (p, c) => true);
        }


        #endregion

        #region View Commands


        public CommandBase PreviewBrowserCommand { get; set; }

        public void Command_PreviewBrowser()
        {
            PreviewBrowserCommand = new CommandBase((parameter, command) =>
            {
                Model.Window.ShowPreviewBrowser(!mmApp.Configuration.IsPreviewVisible);
                Model.PreviewTopic(false);
                
                
            }, (p, c) => true);
        }

        


        #endregion


    }
}