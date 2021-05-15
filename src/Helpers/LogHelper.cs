// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System;
using System.Diagnostics;
using System.IO;

using NLog;
using NLog.Config;

namespace SOSIEL.Helpers
{
    public class LogHelper
    {
        private static readonly string _configFileName = "NLog.config";
        private static readonly object _mutex = new object();
        private static bool _loggingInitialized;

        public static Logger GetLogger()
        {
            return GetLogger(GetCallingClass().FullName);
        }

        public static Logger GetLogger(string name)
        {
            lock (_mutex)
            {
                if (!_loggingInitialized)
                    InitializeLogging();
                return LogManager.GetLogger(name);
            }
        }

        private static void InitializeLogging()
        {
            if (File.Exists(_configFileName))
                LogManager.Configuration = new XmlLoggingConfiguration(_configFileName);
            _loggingInitialized = true;
        }

        private static Type GetCallingClass()
        {
            return (new StackTrace()).GetFrame(1).GetMethod().ReflectedType;
        }
    }
}
