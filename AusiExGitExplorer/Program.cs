using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AusiExGitExplorer
{
    class Program
    {
        private static HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {

            //using (var client = new HttpClient())
            //{
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", "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");                               
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "http://developer.github.com/v3/#user-agent-required");
                var contentsJson = await client.GetAsync("https://api.github.com/user/repos");
                Console.WriteLine(contentsJson.StatusCode);
                JArray contents = (JArray)JsonConvert.DeserializeObject(contentsJson.Content.ReadAsStringAsync().Result);
                List<string> repoList = new List<string>();

                Console.WriteLine("Repositories accessible");
                Console.WriteLine("=================================================");
               
                foreach (JObject content in contents.Children<JObject>())
                {
                    string repo = (string)content["name"];
                    repoList.Add(repo);
                    Console.WriteLine("id: " + (string)content["id"]);
                    Console.WriteLine("name: " + repo);
                    Console.WriteLine("full_name: " + (string)content["full_name"]);
                    Console.WriteLine("html_url: " + (string)content["html_url"]);
                    Console.WriteLine("description: " + (string)content["description"]);
                    Console.WriteLine("=================================================");
                                      
                }

                string user = "myoaunghlaing";
                foreach (string repo in repoList)
                {
                string url = $"https://api.github.com/repos/{user}/{repo}/commits?per_page=10";


                string contentsCommits = client.GetStringAsync("https://api.github.com/repos/myoaunghlaing/AuX_Myo/commits?per_page=10").Result;
                    JArray commits = (JArray)JsonConvert.DeserializeObject(contentsCommits);
                    foreach (JObject com in commits.Children<JObject>())
                    {
                        Console.WriteLine("sha: " + (string)com["sha"]);
                    }

                }
            
            Console.ReadLine();
        }

       
    }
}
