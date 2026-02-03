using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MailKitSample.Services
{
    public  class PromptService : IPromptService
    {
        private readonly IConfigurationBuilder _configurationBuilder;
        private readonly IConfigurationRoot _configurationRoot;
        public PromptService()
        {
            _configurationBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("Pronpt.json", optional: false, reloadOnChange: true);
            _configurationRoot = _configurationBuilder.Build();
        }

        public string? GetPrompt(string key)
        {
            string? prompt = _configurationRoot[key];
            return prompt;
        }

        public string? RetailBusiness => GetPrompt("retailBusiness");
        public string? UserSupport => GetPrompt("userSupport");
        public string? EndUser => GetPrompt("endUser"); 
        public string? ComplianceViolation => GetPrompt("complianceViolation");
    }
}
