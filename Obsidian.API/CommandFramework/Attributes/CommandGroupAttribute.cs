﻿using System;

namespace Obsidian.CommandFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandGroupAttribute : Attribute
    {
        public string GroupName;
        public string[] Aliases;

        public CommandGroupAttribute(string groupname, params string[] aliases)
        {
            this.GroupName = groupname;
            this.Aliases = aliases;
        }
    }
}
