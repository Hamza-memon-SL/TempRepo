using Auto_Alert_2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Alert_2.Services
{
    public class ReportingService
    {
        string sourceConnectionString = Constants.KELiveConnectionString;
        string targetConnectionString = Constants.KELiveBillingConnectionString;
        string loggingConnectionString = Constants.KELiveLoggingConnectionString;
        int batchSize = 500;

        public async Task<bool> BulkReportInsert(List<ReportDataModel> reportDataModels)
        {
            bool isSuccess = false;

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
                        command.Parameters.Add(new SqlParameter("@WebJobLogsName", SqlDbType.NVarChar) { Value = "Auto-Alert-1-Part-2" });
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
            catch (Exception ex)
            {
                return isSuccess;
            }
        }
    }
}
