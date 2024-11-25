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
    public class UserInboxService
    {
        string sourceConnectionString = Constants.KELiveConnectionString;
        string targetConnectionString = Constants.KELiveBillingConnectionString;

        int batchSize = 500;

        public async Task<bool> BulkUserInboxInsert(List<UserInboxModel> userInboxModel, string operationId)
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


            bool isSuccess = false;


            #region AppInsightdependency

            var dependency = new DependencyTelemetry(
             "Auto-Alert",
            "Auto-Alert-Web-Job-1-Part-1",
          "Bulk User Inbox Insert",
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
                using (SqlConnection connection = new SqlConnection(sourceConnectionString))
                {
                    connection.Open();

                    // Your code for bulk insertion goes here.

                    // Perform the bulk insert.
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        //bulkCopy.DestinationTableName = "Report_BulkPushNotification_Job"; // Replace with your actual table name in the database.

                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(userInboxModel);
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
                Console.WriteLine("StackTrace :" + ex.StackTrace);

                #region AppInsightdependency

                var dependency1 = new DependencyTelemetry(
                 "Auto-Alert",
                "Auto-Alert-Web-Job-1-Part-1",
                "Bulk User Inbox Insert",
               $"Bulk User Inbox Insert Exception, at {currentDateTime} {ex.Message.ToString()}",
                 currentDateTime,
                 timeSpan,
                "500",
                 false
                 );
                dependency1.Context.Operation.Id = operationId;

                appInsightsInitializer.TrackDependency(dependency1);

                #endregion

                return isSuccess;

            }

        }

        public async Task InsertDataIntoTargetDatabase(SqlConnection connection, DataTable dataTable, int i)
        {

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "UserInbox"; // Set your target table name
                bulkCopy.WriteToServer(dataTable);
            }
        }
    }
}
