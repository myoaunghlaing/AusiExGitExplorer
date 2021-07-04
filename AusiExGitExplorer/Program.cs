using log4net;
using log4net.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace AusiExGitExplorer
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                
        static async Task Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            try
            {
                using (var client = new HttpClient())
                {
                    string user = ConfigurationManager.AppSettings.Get("user");
                    string token = ConfigurationManager.AppSettings.Get("token");
                    string baseUrl = ConfigurationManager.AppSettings.Get("baseUrl");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "http://developer.github.com/v3/#user-agent-required");

                    var respRepos = await client.GetAsync($"{baseUrl}/user/repos");                    

                    List<string> repoList = new List<string>();
                    if (respRepos.IsSuccessStatusCode)
                    {
                        JArray contents = (JArray)JsonConvert.DeserializeObject(respRepos.Content.ReadAsStringAsync().Result);                       

                        log.Info("\n\nRepositories user has access");
                        log.Info("=================================================");

                        foreach (JObject content in contents.Children<JObject>())
                        {
                            string repo = (string)content["name"];
                            repoList.Add(repo);
                            log.Info("id: " + (string)content["id"]);
                            log.Info("name: " + repo);
                            log.Info("full_name: " + (string)content["full_name"]);
                            log.Info("html_url: " + (string)content["html_url"]);
                            log.Info("description: " + (string)content["description"]);
                            log.Info("=================================================");
                        }
                    }
                    else
                    {
                        log.Error("Error in getting repositories info");
                        log.Error($"Response status: {respRepos.StatusCode}");
                    }

                    log.Info("\n\nLatest 10 commits of the repositories");
                    log.Info("=================================================");

                    foreach (string repo in repoList)
                    {
                        string url = $"{baseUrl}/repos/{user}/{repo}/commits?per_page=10";

                        var respCommits = await client.GetAsync(url);                        
                        
                        if (respCommits.IsSuccessStatusCode)
                        {
                            JArray commits = (JArray)JsonConvert.DeserializeObject(respCommits.Content.ReadAsStringAsync().Result);
                            log.Info("\n\nrepository: " + repo);
                            log.Info("=================================================");
                            foreach (JObject com in commits.Children<JObject>())
                            {
                                log.Info("sha: " + (string)com["sha"]);
                                log.Info("author name: " + (string)com["commit"]["author"]["name"]);
                                log.Info("email: " + (string)com["commit"]["author"]["email"]);
                                log.Info("date: " + (string)com["commit"]["author"]["date"]);
                                log.Info("message: " + (string)com["commit"]["message"]);
                                log.Info("url: " + (string)com["url"]);
                                log.Info("=================================================");
                            }
                        }
                        else if(respCommits.StatusCode == HttpStatusCode.Conflict)
                        {
                            log.Info($"\n\nNo commit found for the repository {repo}");
                            log.Info("=================================================");
                        }
                        else
                        {
                            log.Error("Error in getting commits info");
                            log.Error($"Response status: {respRepos.StatusCode}");
                        }
                    }           
                    Console.ReadKey();
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }
       
    }
}
