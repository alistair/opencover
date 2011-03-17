﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An entity that contains methods
    /// </summary>
    public class Class
    {
        public string FullName { get; set; }
        public IList<File> Files { get; set; }
        public IList<Method> Methods { get; set; }
    }
}