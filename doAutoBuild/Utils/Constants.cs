﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doAutoBuild.Utils
{
    static class Constants
    {

        public const string RootPath = "E:\\AutoBuildHome";

        public static string BuildConfig = RootPath + Path.DirectorySeparatorChar + "Build.config";

        public static string BuildBin = RootPath+ Path.DirectorySeparatorChar + "BuildBin";

        public static string CurrentConfig = RootPath + Path.DirectorySeparatorChar + "CurrentConfig";

        public static string CurrentVersion = RootPath + Path.DirectorySeparatorChar + "CurrentVersion";

        public static string SourceFile = RootPath + Path.DirectorySeparatorChar + "SourceFile";

        public static string HistoryVersion = RootPath + Path.DirectorySeparatorChar + "HistoryVersion";

        public static string Temp = RootPath + Path.DirectorySeparatorChar + "Temp";

        public static string TargetServices = CurrentConfig + Path.DirectorySeparatorChar + "TargetServices";

        public static string Projects = CurrentConfig + Path.DirectorySeparatorChar + "Projects";


        ////////////////////////////////////////////////////////////
        public const string ConfigFile = "ConfigFile/Qiniu.config";
    }
}
