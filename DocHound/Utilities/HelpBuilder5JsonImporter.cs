﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using DocHound.Model;
using Westwind.Utilities;

namespace DocHound.Utilities
{
    public class HelpBuilder5JsonImporter
    {

        public bool ImportHbp(string inputFile,string outputFolder = null)
        {
            if (outputFolder == null)
                outputFolder = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(inputFile));

            if (!Directory.Exists(outputFolder))            
                Directory.CreateDirectory(outputFolder);

            if (!Directory.Exists(Path.Combine(outputFolder, "wwwroot")))
                Directory.CreateDirectory(Path.Combine(outputFolder, "wwwroot"));


            var oldTopics =
                JsonSerializationUtils.DeserializeFromFile(inputFile, typeof(List<HbpTopic>)) as List<HbpTopic>;

            var project = new DocProject(Path.Combine(outputFolder,"_toc.json"));
            var newTopics = new ObservableCollection<DocTopic>();
            project.Topics = newTopics;
            foreach (var oldTopic in oldTopics)
            {
                var newTopic = new DocTopic(project)
                {
                    Id = oldTopic.pk,
                    ParentId = oldTopic.parentpk,
                    Title = oldTopic.topic,                                        
                    Type = oldTopic.type?.ToLower(),
                    Keywords = oldTopic.keywords,
                    Remarks = oldTopic.remarks,
                    Example = oldTopic.example,
                    SeeAlso = oldTopic.seealso,
                    SortOrder = oldTopic.sortorder,
                    IsLink = oldTopic.external,
                    Incomplete = oldTopic.followup,
                    HelpId = oldTopic.helpid.ToString(),
                    ClassInfo = new ClassInfo()
                    {
                       Syntax = string.IsNullOrEmpty(oldTopic.syntax) ? null : oldTopic.syntax,
                       Classname = string.IsNullOrEmpty(oldTopic._class) ? null : oldTopic._class,
                       MemberName = string.IsNullOrEmpty(oldTopic.method) ? null : oldTopic.method,
                       Parameters = string.IsNullOrEmpty(oldTopic.parameters) ? null : oldTopic.parameters,
                       Returns= string.IsNullOrEmpty(oldTopic.returns) ? null : oldTopic.returns,
                       Scope = string.IsNullOrEmpty(oldTopic.scope) ? null : oldTopic.scope,
                       Implements = string.IsNullOrEmpty(oldTopic.implements) ? null : oldTopic.implements,
                       Inherits = string.IsNullOrEmpty(oldTopic.inherits) ? null : oldTopic.inherits,
                       InheritanceTree = string.IsNullOrEmpty(oldTopic.inh_tree) ? null : oldTopic.inh_tree,
                       Signature = string.IsNullOrEmpty(oldTopic.signature) ? null : oldTopic.signature,
                       Assembly = oldTopic.assembly,
                       Contract = oldTopic.contract,                       
                       Namespace = oldTopic._namespace,
                       Exceptions = oldTopic.exceptions,
                    }, 

                };
                newTopic.Project = project;                
                newTopic.Body = oldTopic.body;

                int format = oldTopic.viewmode;
                newTopic.BodyFormat = format == 2 ? TopicBodyFormats.Markdown : TopicBodyFormats.HelpBuilder;

                newTopic.SaveTopicFile();
                
                // Properties have to be parsed out
                // BodyFormats
                project.Topics.Add(newTopic);
            }


            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // Copy images
            string sourceFolder = Path.GetDirectoryName(inputFile);
            KavaUtils.CopyDirectory(Path.Combine(sourceFolder, "Images"), Path.Combine(outputFolder, "wwwroot", "images"));

            project.GetTopicTreeFromFlatList(project.Topics);

            return project.SaveProject(Path.Combine(outputFolder, "_toc.json"));
        }
    }

    public class ExportedHbp
    {
        public List<HbpTopic> Topics { get; set; }
    }

    public class HbpTopic
    {
        public string pk { get; set; }
        public string parentpk { get; set; }
        public int viewmode { get; set; }
        public string topic { get; set; }
        public string type { get; set; }
        public bool external { get; set; }
        public string body { get; set; }
        public string endbody { get; set; }
        public string _class { get; set; }
        public string method { get; set; }
        public string scope { get; set; }
        public string syntax { get; set; }
        public string parameters { get; set; }
        public string returns { get; set; }
        public string remarks { get; set; }
        public string example { get; set; }
        public string seealso { get; set; }
        public string keywords { get; set; }
        public string properties { get; set; }
        public string methods { get; set; }
        public string events { get; set; }
        public bool expanded { get; set; }
        public int helpid { get; set; }
        public bool nocontent { get; set; }
        public int sortorder { get; set; }
        public DateTime updated { get; set; }
        public bool followup { get; set; }
        public string signature { get; set; }
        public string assembly { get; set; }
        public string _namespace { get; set; }
        public string inherits { get; set; }
        public string inh_tree { get; set; }
        public string implements { get; set; }
        public string exceptions { get; set; }
        public string contract { get; set; }
        public bool _static { get; set; }
    }

}