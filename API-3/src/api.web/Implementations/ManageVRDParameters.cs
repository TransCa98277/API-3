using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.web.Implementations
{
    public class ManageVRDParameters : IManageVRDParameters
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        private const string API3_SETTINGS = "API3Settings";
        private string VRDEndPoint { get; }
        private string DataFolder { get; }

        public IDictionary<string, string> VRDParameters { get; set; }

        /// <summary>
        /// Help to read parameters from appsettings.json
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public ManageVRDParameters(ILogger<ManageVRDParameters> logger, IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            string sectionName = this.GetType().Name;

            // VRDEndPoint
            if (_config.GetSection($"{API3_SETTINGS}:{sectionName}:{nameof(VRDEndPoint)}").Value == null)
                _logger.LogCritical($"Unable to find config section: {API3_SETTINGS}:{sectionName}:{nameof(VRDEndPoint)}");
            else
                VRDEndPoint = _config.GetSection($"{API3_SETTINGS}:{sectionName}:{nameof(VRDEndPoint)}")?.Value;

            // DataFolder
            if (_config.GetSection($"{API3_SETTINGS}:{sectionName}:{nameof(DataFolder)}").Value == null)
                _logger.LogCritical($"Unable to find config section: {API3_SETTINGS}:{sectionName}:{nameof(DataFolder)}");
            else
                DataFolder = _config.GetSection($"{API3_SETTINGS}:{sectionName}:{nameof(DataFolder)}")?.Value;

            // Initialize dictionary
            VRDParameters = new Dictionary<string, string>()
            {
                {nameof(VRDEndPoint), VRDEndPoint },
                {nameof(DataFolder), DataFolder },
            };
        }
    }
}
