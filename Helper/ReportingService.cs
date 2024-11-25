using Bulk_Push.ApplicationInsights;
using Bulk_Push.Model;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulk_Push.Services
{
    public class ReportingService
    {
        string sourceConnectionString = Constants.KELiveConnectionString;
        string targetConnectionString = Constants.KELiveBillingConnectionString;
        string loggingConnectionString = Constants.KELiveLoggingConnectionString;

        int batchSize = 500;

        public async Task<bool> BulkReportInsert(List<ReportDataModel> reportDataModels, string operationId)
        {
            TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            //Get the current local time in Pakistan
            DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);

            DateTime currentDate = pakistanTime.Date; // Assuming this is the current date Pakistani
            DateTime currentDateTime = pakistanTime; // Assuming this is the current date time Pakistani
                                                     // need to connected with Ke server database 
                                                     // create a similar sp for this for there env
                                                     // check the dynamic message

            TimeSpan timeSpan = currentDateTime.Subtract(currentDateTime);
            AppInsightsInitializer appInsightsInitializer = new AppInsightsInitializer();

            DependencyTelemetry dependencyTelemetry = new DependencyTelemetry();

            bool isSuccess = false;


            #region AppInsightdependency

            var dependency = new DependencyTelemetry(
             "Auto-Alert",
            "Auto-Alert-Web-Job-1-Part-1",
          "Bulk Reporting Insert",
           string.Empty,
             currentDateTime,
             timeSpan,
             "200",
             true
             );
            dependency.Context.Operation.Id = operationId;

            appInsightsInitializer.TrackDependency(dependency);

            #endregion


            try
            {
                using (SqlConnection connection = new SqlConnection(targetConnectionString))
                {
                    connection.Open();

                    // Your code for bulk insertion goes here.

                    // Perform the bulk insert.
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        //bulkCopy.DestinationTableName = "Report_BulkPushNotification_Job"; // Replace with your actual table name in the database.

                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(reportDataModels);
                        DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(json);

                        for (int i = 0; i < dataTable.Rows.Count; i += batchSize)
                        {

                            DataTable batchDataTable = dataTable.AsEnumerable()
                                .Skip(i)
                                .Take(batchSize)
                                .CopyToDataTable();

                            // add data into KeLiveUsers Table
                            await InsertDataIntoTargetDatabase(connection, batchDataTable, i);
                            isSuccess = true;
                        }


                        // Perform the bulk insert.
                        //bulkCopy.WriteToServer(dataTable);

                        //isSuccess = true;

                    }
                    connection.Close();
                }
                return isSuccess;
            }
            catch (Exception ex)
            {

                #region AppInsightdependency

                var dependency1 = new DependencyTelemetry(
                 "Auto-Alert",
                "Auto-Alert-Web-Job-1-Part-1",
                "Bulk User Inbox Insert",
              $"Bulk Reporting Insert Exception, at {currentDateTime} {ex.Message.ToString()}",
                 currentDateTime,
                 timeSpan,
                "500",
                 false
                 );
                dependency1.Context.Operation.Id = operationId;

                appInsightsInitializer.TrackDependency(dependency1);

                #endregion

                Console.WriteLine("StackTrace :" + ex.StackTrace);
                return isSuccess;

            }

        }

        public async Task InsertDataIntoTargetDatabase(SqlConnection connection, DataTable dataTable, int i)
        {

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "Report_BulkPushNotification_Job"; // Set your target table name
                bulkCopy.WriteToServer(dataTable);
            }
        }

        public async Task<bool> InsertWebJobLog()
        {
            bool isSuccess = false;
            TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            //Get the current local time in Pakistan
            DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);

            try
            {
                using (SqlConnection connectionLogging = new SqlConnection(loggingConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_WebJobLogsInsert", connectionLogging))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters to the stored procedure
                        command.Parameters.Add(new SqlParameter("@WebJobLogsName", SqlDbType.NVarChar) { Value = "Auto-Alert-1-Part-1" });
                        command.Parameters.Add(new SqlParameter("@IsSuccess", SqlDbType.NVarChar) { Value = 1 });
                        command.Parameters.Add(new SqlParameter("@CreateDate", SqlDbType.NVarChar) { Value = pakistanTime });

                        try
                        {
                            connectionLogging.Open();
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            // Handle exceptions appropriately (log, rethrow, etc.)
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        connectionLogging.Close();
                    }
                }
                return isSuccess = true;
            }
            catch(Exception ex)
            {
                return isSuccess;
            }
        }
    }
}
