using DownloadScheduler.Contracts.Context;
using DownloadScheduler.Contracts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadScheduler.Services
{
    public class FilesService
    {
        private readonly DbContextGenAiPOC _context;

        public FilesService(DbContextGenAiPOC context)
        {
            _context = context;
        }
        public async Task<List<FileDetails>> GetAllFileDetails(int projectId, string repoUrl, string localPath)
        {

            try
            {
                GithubCloneService gitService = new GithubCloneService();

                var repoName = await gitService.GetRepositoryName(repoUrl);
                var repoPath = Path.Combine(localPath, repoName);

                Console.WriteLine("Project Files Repo Name " + repoName);

                //var fileDetailsList = new List<FileDetails>();
                var files = Directory.GetFiles(localPath, "*.*", SearchOption.AllDirectories);
                List<string> allowedExtensions = new List<string> { ".txt", ".cs" };

                var fileDetailsList = files
                    .Select(file => new FileInfo(file))
                    .Where(fileInfo =>
                        !fileInfo.Name.StartsWith(".") &&
                        !fileInfo.Name.Equals("config", StringComparison.OrdinalIgnoreCase) &&
                        (allowedExtensions.Count == 0 || allowedExtensions.Contains(fileInfo.Extension.ToLower())))
                    .Select(fileInfo => new FileDetails
                    {
                        Name = fileInfo.Name,
                        FullPath = fileInfo.FullName,
                        Extension = fileInfo.Extension,
                        Status = "Listed",
                        Size = FormatFileSize(fileInfo.Length),
                        CreateDate = fileInfo.CreationTime,
                        ProjectId = projectId,
                    })
                    .ToList();

                Console.WriteLine("Project Files Listed Successfully");
                return fileDetailsList;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Project Files Listed Filed due to this error "+ ex.Message);
                throw;
            }
         

            //foreach (var file in files)
            //{
            //    // Define allowed extensions (e.g., ".txt" and ".cs")
            //    List<string> allowedExtensions = new List<string> { ".txt", ".cs" };
            //    var fileInfo = new FileInfo(file);

            //    //// Skip hidden files
            //    //if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            //    //{
            //    //    continue;
            //    //}

            //    if (fileInfo.Name.StartsWith(".") || fileInfo.Name.Equals("config", StringComparison.OrdinalIgnoreCase))
            //    {
            //        continue;
            //    }
            //    // Filter by allowed extensions
            //    if (allowedExtensions.Count > 0 && !allowedExtensions.Contains(fileInfo.Extension.ToLower()))
            //    {
            //        continue;
            //    }

            //    var details = new FileDetails
            //    {
            //        Name = fileInfo.Name,
            //        FullPath = fileInfo.FullName,
            //        Extension = fileInfo.Extension,
            //        Status = "Listed",
            //        Size = fileInfo.Length,
            //        CreateDate = fileInfo.CreationTime,
            //    };

            //    fileDetailsList.Add(details);


            //}

        }

        private static string FormatFileSize(long bytes)
        {
            const int kb = 1024;
            const int mb = kb * 1024;
            const int gb = mb * 1024;

            if (bytes >= gb)
                return $"{bytes / (double)gb:0.##} GB";
            if (bytes >= mb)
                return $"{bytes / (double)mb:0.##} MB";
            if (bytes >= kb)
                return $"{bytes / (double)kb:0.##} KB";
            return $"{bytes} bytes";
        }

        public async Task<bool> SaveFileDetailsToDatabase(List<FileDetails> fileDetailsList)
        {
            bool isSuccess = false;
            try
            {
                Console.WriteLine("Project Files Saving Function Started");

                // Insert data into the database using AddRange
                _context.FileDetails.AddRange(fileDetailsList);
                int isFileDetailsSaved = await _context.SaveChangesAsync();

                if (isFileDetailsSaved > 0)
                    return isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Project Files Saving Function Failed");
                return isSuccess;
            }
            return isSuccess;
        }
    }
}
