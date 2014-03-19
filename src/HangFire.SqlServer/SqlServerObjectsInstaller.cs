﻿using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Dapper;

namespace HangFire.SqlServer
{
    internal static class SqlServerObjectsInstaller
    {
        private const int RequiredSchemaVersion = 1;

        private static readonly ILog Log = LogManager.GetLogger(typeof(SqlServerStorage));

        public static void Install(SqlConnection connection)
        {
            Log.Debug("Start installing HangFire SQL objects...");

            if (!IsSqlEditionSupported(connection))
            {
                throw new PlatformNotSupportedException("The SQL Server edition of the target server is unsupported, e.g. SQL Azure.");
            }

            var script = GetStringResource(
                typeof(SqlServerObjectsInstaller).Assembly, 
                "HangFire.SqlServer.Install.sql");

            script = script.Replace("SET @TARGET_SCHEMA_VERSION = 2;", "SET @TARGET_SCHEMA_VERSION = " + RequiredSchemaVersion + ";");

            connection.Execute(script);

            Log.Debug("HangFire SQL objects installed.");
        }

        private static bool IsSqlEditionSupported(SqlConnection connection)
        {
            var edition = connection.Query<int>("SELECT SERVERPROPERTY ( 'EngineEdition' )").Single();
            return edition >= SqlEngineEdition.Standard && edition <= SqlEngineEdition.Express;
        }

        private static string GetStringResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static class SqlEngineEdition
        {
            // See article http://technet.microsoft.com/en-us/library/ms174396.aspx for details on EngineEdition
            public const int Personal = 1;
            public const int Standard = 2;
            public const int Enterprise = 3;
            public const int Express = 4;
            public const int SqlAzure = 5;
        }
    }
}
