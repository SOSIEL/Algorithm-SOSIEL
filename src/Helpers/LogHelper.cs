// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

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
