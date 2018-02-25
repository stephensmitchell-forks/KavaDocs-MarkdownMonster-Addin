﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KavaDocsAddin
{
    public class KavaDocsMenuHandler
    {
        public AddinModel Model { get; set; }

        public MenuItem KavaDocsMenuItem { get; set; }

        public KavaDocsMenuHandler()
        {
            Model = kavaUi.AddinModel;
        }
        public MenuItem CreateKavaDocsMenu()
        {
            var mi = new MenuItem
            {
                Header = "_Kava Docs",                
            };
            mi.Items.Add(new MenuItem
            {
                Header = "_Open Project",
                Command = Model.Commands.OpenProjectCommand
            });
            mi.Items.Add(new MenuItem
            {
                Header = "_Save Project",
                Command = Model.Commands.SaveProjectCommand
            });
            mi.Items.Add(new MenuItem
            {
                Header = "_Close Project",
                Command = Model.Commands.CloseProjectCommand
            });

            // insert Item after MainMenuEdit item on Main menu
            Model.Addin.AddMenuItem(mi, "MainMenuTools",mode: 0);

            KavaDocsMenuItem = mi;

            return mi;
        }

    }
}