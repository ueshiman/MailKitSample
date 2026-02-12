using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MailKitSample.Services
{
    public  class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private const string ApiKeyKey = "MailCreateAIKey";
        private const string DeploymentNameKey = "DeploymentName";
        private const string ValidDomainKey = "ValidDomain";
        private const string EndPointKey = "Endpoint";
        private const string DefaultDeploymentName = "gpt-4.1-mini";

        public ConfigurationService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();
        }


        public string ValidDomain =>  _configuration[ValidDomainKey] ?? string.Empty;


        public string DeploymentName => _configuration[DeploymentNameKey] ?? DefaultDeploymentName;

        public string MailCreateAIKey => Environment.GetEnvironmentVariable(ApiKeyKey) ?? string.Empty;
        public string EndPoint => _configuration[EndPointKey] ?? string.Empty;
    }
}
