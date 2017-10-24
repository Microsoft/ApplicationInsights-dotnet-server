﻿namespace FuncTest.Helpers
{
    using System;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public class LocalDb
    {
        public const string LocalDbConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog={0};Integrated Security=True;Connection Timeout=300";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:Pass System.Uri objects instead of strings", Justification = "Not a normal URI.")]
        public static void CreateLocalDb(string databaseName, string scriptName)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);

            string outputFolder = Path.Combine(Environment.GetEnvironmentVariable("SystemDrive") + "\\", "SqlExpress");
            string mdfFilename = databaseName + ".mdf";
            string databaseFileName = Path.Combine(outputFolder, mdfFilename);

            // Create Data Directory If It Doesn't Already Exist.
            if (!Directory.Exists(outputFolder))
            {
                Trace.TraceInformation($"Sql Test Directory does not exist. Creating directory: {outputFolder}");
                Directory.CreateDirectory(outputFolder);

                if (!Directory.Exists(outputFolder))
                {
                    throw new Exception($"Failed to create Sql Test Directory: '{outputFolder}'");
                }
            }

            else if (!CheckDatabaseExists(databaseName))
            {
                Trace.TraceInformation($"Sql Database does not exist. Creating database: {databaseName}");
                // If the database does not already exist, create it.
                CreateDatabase(databaseName, databaseFileName);
                ExecuteScript(databaseName, scriptName);

                if (!CheckDatabaseExists(databaseName))
                {
                    throw new Exception($"Failed to create Sql Database: '{databaseName}'");
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private static void ExecuteScript(string databaseName, string scriptName)
        {
            string connectionString = string.Format(CultureInfo.InvariantCulture, LocalDbConnectionString, databaseName);

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);

            var file = new FileInfo(scriptName);
            string script = file.OpenText().ReadToEnd();

            string[] commands = script.Split(new[] { "GO\r\n", "GO ", "GO\t" }, StringSplitOptions.RemoveEmptyEntries);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (string c in commands)
                {
                    var command = new SqlCommand(c, connection);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static bool CheckDatabaseExists(string databaseName)
        {
            string connectionString = string.Format(CultureInfo.InvariantCulture, LocalDbConnectionString, "master");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("select db_id(@databaseName)", connection);

                cmd.Parameters.Add(new SqlParameter("@databaseName", databaseName));
                object result = cmd.ExecuteScalar();
                if (result != null && !Convert.IsDBNull(result))
                {
                    return true;
                }
            }

            return false;
        }

        private static void CreateDatabase(string databaseName, string databaseFileName)
        {
            string connectionString = string.Format(CultureInfo.InvariantCulture, LocalDbConnectionString, "master");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string commandStr = $"CREATE DATABASE {databaseName} ON (NAME = N'{databaseName}', FILENAME = '{databaseFileName}')";
                SqlCommand cmd = new SqlCommand(commandStr, connection);

                //cmd.Parameters.Add(new SqlParameter("@databaseName", databaseName));
                //cmd.Parameters.Add(new SqlParameter("@databaseFileName", databaseFileName));
                cmd.ExecuteNonQuery();
            }
        }
    }
}
