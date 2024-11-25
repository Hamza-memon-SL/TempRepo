
using DownloadScheduler.Contracts.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DownloadScheduler.Services
{
    public class ProjectsService
    {
        private readonly DbContextGenAiPOC _context;

        public ProjectsService(DbContextGenAiPOC context)
        {
            _context = context;
        }
        public async Task<bool> UpdateProjectStatus(int projectId)
        {
            bool isSuccess = false;

            try
            {
                var isProjectExist = await _context.Projects.Where(x => x.Id.Equals(projectId)).FirstOrDefaultAsync();
                if (isProjectExist.Status == "New")
                {
                    isProjectExist.Status = "Processing";
                    _context.SaveChanges(); // Save the changes to the database
                    Console.WriteLine("Project status updated to 'Processing'.");
                    return isSuccess = true;
                }
                else
                {
                    return isSuccess;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating project status: " + ex.Message);
                return isSuccess;
            }

        }
    }
}
