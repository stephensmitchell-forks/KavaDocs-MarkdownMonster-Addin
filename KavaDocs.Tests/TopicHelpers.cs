﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocHound.Model;
using NUnit.Framework;


namespace DocumentationMonster.Core.Tests
{
    [TestFixture]
    public class TopicHelpersTests
    {
        

        [Test]
        public void SlugTests()
        {
            var topicName = "This is a simple topic name";

            var topic = new DocTopic();
            topic.Title = topicName;
            var slug = topic.CreateSlug();

            Console.WriteLine(slug);
            Assert.AreEqual(slug, "This-is-a-simple-topic-name");
            
            slug = topic.CreateSlug("This is Rick's Test topic");
            Console.WriteLine(slug);
            Assert.AreEqual(slug, "This-is-Ricks-Test-topic");


            slug = topic.CreateSlug("This is an off-center topic");
            Console.WriteLine(slug);
            Assert.AreEqual(slug, "This-is-an-off--center-topic");

        }
    }
}
