using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadScheduler.Services
{
    internal class GithubCloneService
    {
        public async Task<bool> ClonePublicRepository(string repoUrl, string localPath)
        {
            bool isSuccess = false;

            try
            {
                // Ensure the master folder exists
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }
                var repoName = await GetRepositoryName(repoUrl);
                var repoPath = Path.Combine(localPath, repoName);

                // Clone the public repository without credentials
                Repository.Clone(repoUrl, repoPath);

                Console.WriteLine($"Repository cloned successfully into {repoPath}!");
                return isSuccess = true;// Success
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cloning repository: " + ex.Message);
                return isSuccess = true; // Failed

            }
        }

        public async Task<string> GetRepositoryName(string repoUrl)
        {
            // Extract the repository name from the URL
            var uri = new Uri(repoUrl);
            var segments = uri.Segments;
            var lastSegment = segments[segments.Length - 1];
            var repoName = Path.GetFileNameWithoutExtension(lastSegment);
            return repoName;
        }
    }
}
