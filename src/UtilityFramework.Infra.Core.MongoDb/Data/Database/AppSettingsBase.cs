using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Driver;
using System;

namespace UtilityFramework.Infra.Core.MongoDb.Data.Database
{
    public static class AppSettingsBase
    {
        private static IConfigurationRoot Configuration { get; set; }
        private static BaseSettings _settingsDataBase { get; set; }

        public static BaseSettings GetSettings(IHostingEnvironment env)
        {


            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            Configuration = builder.Build();

            var baseSettings = new BaseSettings();

            Configuration.GetSection("DATABASE").Bind(baseSettings);

            if (baseSettings.MaxConnections == 0 || baseSettings.MaxConnections == null)
                baseSettings.MaxConnections = 250;

            if (baseSettings.MinConnections == 0 || baseSettings.MinConnections == null)
                baseSettings.MinConnections = 50;

            _settingsDataBase = baseSettings;

            return baseSettings;
        }

        public static MongoClient GetMongoClient(IHostingEnvironment env)
        {
            _settingsDataBase = GetSettings(env);

            return new MongoClient(ReadMongoClientSettings());

        }

        private static MongoClientSettings ReadMongoClientSettings()
        {
            MongoClientSettings mongoClientSettings = null;

            if (string.IsNullOrEmpty(_settingsDataBase.Name))
            {
                throw new Exception("Informe o nome do banco de dados no campo  DATABASE:NAME no appsettings.*.json");
            }

            if (string.IsNullOrEmpty(_settingsDataBase.ConnectionString) == false)
            {
                mongoClientSettings = MongoClientSettings.FromConnectionString(_settingsDataBase.ConnectionString);

                return mongoClientSettings;
            }

            if (string.IsNullOrEmpty(_settingsDataBase.Local))
            {
                throw new Exception("Informe o nome do banco de dados no campo  DATABASE:LOCAL no appsettings.*.json");
            }

            if (string.IsNullOrEmpty(_settingsDataBase.Remote))
            {
                throw new Exception("Informe o nome do banco de dados no campo  DATABASE:REMOTE no appsettings.*.json");
            }

            mongoClientSettings = new MongoClientSettings
            {
                ConnectionMode = ConnectionMode.Automatic,
                Servers = ListServers(),
                MaxConnectionPoolSize = _settingsDataBase.MaxConnections.GetValueOrDefault(),
                MinConnectionPoolSize = _settingsDataBase.MinConnections.GetValueOrDefault()
            };

            if (string.IsNullOrEmpty(_settingsDataBase.Password) || string.IsNullOrEmpty(_settingsDataBase.User))
                return mongoClientSettings;

            var mongoCredential = MongoCredential.CreateMongoCRCredential(_settingsDataBase.Name,
                _settingsDataBase.User, _settingsDataBase.Password);

            mongoClientSettings.Credential = mongoCredential;

            return mongoClientSettings;
        }

        private static IEnumerable<MongoServerAddress> ListServers()
        {
            var servers = new List<MongoServerAddress>
            {
                BaseSettings.IsDev
                    ? new MongoServerAddress(_settingsDataBase.Remote, 27017)
                    : new MongoServerAddress(_settingsDataBase.Local, 27017)
            };

            return servers;
        }

    }
}