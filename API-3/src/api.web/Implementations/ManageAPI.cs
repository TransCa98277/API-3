using api.web.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace api.web.Implementations
{
    public class ManageAPI : IManageAPI
    {
        private readonly ILogger<ManageAPI> _logger;
        private readonly IManageVRDParameters _manageVRDParameters;

        public List<List<Node>> TCApiData { get; set; }

        public ManageAPI(ILogger<ManageAPI> logger, IManageVRDParameters manageVRDParameters)
        {
            _logger = logger;
            _manageVRDParameters = manageVRDParameters;
        }

        /// <summary>
        /// Save File in the repository defined in appsettings.json
        /// </summary>
        /// <param name="vrData"></param>
        /// <param name="fileName"></param>
        /// <returns>string</returns>
        private string SaveFile(IEnumerable<VehicleRecallModel> vrData, string fileName)
        {
            try
            {
                string path = Path.Combine(Environment.CurrentDirectory, _manageVRDParameters.VRDParameters["DataFolder"]);

                // Create data folder, if it doesn't exist
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePath = Path.Combine(path, fileName);

                using (StreamWriter file = File.CreateText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, vrData);
                }

                return filePath;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Save json file received 
        /// </summary>
        /// <param name="vrData"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task SaveInput(IEnumerable<VehicleRecallModel> vrData, string fileName)
        {
            MethodBase method = MethodBase.GetCurrentMethod();

            try
            {
                string filePath = SaveFile(vrData, fileName);

                _logger.LogInformation($"File {filePath} successfully saved -> {method.Name}");
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Call external TC API to get more information about all recallNumber
        /// </summary>
        /// <param name="vrData"></param>
        /// <returns>bool</returns>
        public async Task<bool> CallVRDApiAsync(IEnumerable<VehicleRecallModel> vrData)
        {
            MethodBase method = MethodBase.GetCurrentMethod();
            bool success = true;

            try
            {
                string urlVRDApi = _manageVRDParameters.VRDParameters["VRDEndPoint"];
                string urlVRDApiCall = string.Empty;

                TCApiData = new List<List<Node>>();

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Call VRD API for each Recall Number from API-1, or API-2
                    foreach (VehicleRecallModel vr in vrData)
                    {
                        urlVRDApiCall = string.Format(urlVRDApi, vr.RecallNumber);

                        using (var response = await httpClient.GetAsync(urlVRDApiCall))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                // Extrat just data we need for each Recall number
                                string result = await response.Content.ReadAsStringAsync();
                                var apiData = JsonConvert.DeserializeObject<TCVehicleRecallDataModel>(result);
                                var node = apiData.ResultSet
                                                    .Select(a => a.Where(b => b.Name == "RECALL_NUMBER_NUM" || b.Name == "SYSTEM_TYPE_ETXT" || b.Name == "SYSTEM_TYPE_FTXT"))
                                                    .FirstOrDefault();

                                if (node != null && node.Any())
                                {
                                    TCApiData.Add(node.ToList());
                                    _logger.LogInformation($"API {urlVRDApiCall} called successfully -> {method.Name}");
                                }
                                else
                                    _logger.LogWarning($"API {urlVRDApiCall} call returned no results -> {method.Name}");
                            }
                            else
                            {
                                // Stop if 1 error
                                success = false;
                                _logger.LogError($"Failure {response.StatusCode} calling API {urlVRDApiCall} -> {response}");
                                break;
                            }
                        }
                    }
                }

                return success;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Add SYSTEM_TYPE_ETXT and SYSTEM_TYPE_FTXT for each RECALL_NUMBER_NUM into vrData
        /// </summary>
        /// <param name="vrData"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task AddContentToJsonSource(IEnumerable<VehicleRecallModel> vrData, string fileName)
        {
            MethodBase method = MethodBase.GetCurrentMethod();

            try
            {
                // Add SYSTEM_TYPE_ETXT and SYSTEM_TYPE_FTXT for each RECALL_NUMBER_NUM
                for (int i = 0; i < vrData.Count(); i++)
                {
                    foreach (var nodeList in TCApiData)
                    {
                        var TCApiDataNode = nodeList.FirstOrDefault(a => a.Value.Literal == vrData.ElementAt(i).RecallNumber);

                        if (TCApiDataNode != null)
                        {
                            var systemTypeETXT = nodeList.FirstOrDefault(a => a.Name == "SYSTEM_TYPE_ETXT");
                            vrData.ElementAt(i).SYSTEM_TYPE_ETXT = (systemTypeETXT != null) ? systemTypeETXT.Value.Literal : string.Empty;

                            var systemTypeFTXT = nodeList.FirstOrDefault(a => a.Name == "SYSTEM_TYPE_FTXT");
                            vrData.ElementAt(i).SYSTEM_TYPE_FTXT = (systemTypeFTXT != null) ? systemTypeFTXT.Value.Literal : string.Empty;

                            TCApiData.Remove(nodeList);
                            break;
                        }
                    }
                }

                string filePath = SaveFile(vrData, fileName);
                _logger.LogInformation($"File {filePath} successfully saved -> {method.Name}");
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get All Data from api3.json created
        /// </summary>
        /// <returns>IEnumerable<VehicleRecallModel></returns>
        public async Task<IEnumerable<VehicleRecallModel>> GetAllData()
        {
            MethodBase method = MethodBase.GetCurrentMethod();

            try
            {
                string path = Path.Combine(Environment.CurrentDirectory, _manageVRDParameters.VRDParameters["DataFolder"]);
                string filePath = Path.Combine(path, "api3.json");

                // Read json created by API3
                var data = File.ReadAllText(filePath);
                var jsonObject = JsonConvert.DeserializeObject<IEnumerable<VehicleRecallModel>>(data);

                _logger.LogInformation($"File {filePath} successfully read -> {method.Name}");
                return await Task.FromResult(jsonObject);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Full search on SYSTEM_TYPE_ETXT and SYSTEM_TYPE_FTXT
        /// </summary>
        /// <param name="systemType"></param>
        /// <returns>IEnumerable<VehicleRecallModel></returns>
        public async Task<IEnumerable<VehicleRecallModel>> GetBySystemType(string systemType)
        {
            MethodBase method = MethodBase.GetCurrentMethod();

            try
            {
                // Get full api3.json file and search for specific systemtype
                var jsonObject = await GetAllData();
                var node = jsonObject.Where(a => a.SYSTEM_TYPE_ETXT?.ToLower().Contains(systemType) == true || a.SYSTEM_TYPE_FTXT?.ToLower().Contains(systemType) == true)
                                        .DefaultIfEmpty();

                _logger.LogInformation($"Get by system type {systemType} executed -> {method.Name}");
                return node;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Search by recall Number
        /// </summary>
        /// <param name="recallNumber"></param>
        /// <returns>VehicleRecallModel</returns>
        public async Task<VehicleRecallModel> GetByRecallNumber(string recallNumber)
        {
            MethodBase method = MethodBase.GetCurrentMethod();

            try
            {
                // Get full api3.json file and search for specific recallNumber node
                var jsonObject = await GetAllData();
                var node = jsonObject.FirstOrDefault(a => a.RecallNumber == recallNumber);

                _logger.LogInformation($"Get by recall number {recallNumber} executed -> {method.Name}");
                return node;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
