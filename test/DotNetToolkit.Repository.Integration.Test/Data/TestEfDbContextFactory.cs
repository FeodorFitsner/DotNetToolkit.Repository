﻿namespace DotNetToolkit.Repository.Integration.Test.Data
{
    using System.Data.Common;

    public static class TestEfDbContextFactory
    {
        public static TestEfDbContext Create()
        {
            var currentFile = TestPathHelper.GetTempFileName();
            var connectionString = $"Data Source={currentFile};Persist Security Info=False";
            var conn = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0").CreateConnection();

            conn.ConnectionString = connectionString;
            conn.Open();

            return new TestEfDbContext(conn);
        }
    }
}
