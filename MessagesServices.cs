using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Bulk_Push.Model;
using System.Runtime.Remoting.Messaging;
using Bulk_Push.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bulk_Push
{
    public class MessagesServices
    {
        public async Task<List<MessageDataModel>> GenerateMessages(string operationId)
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

            try
            {

                var result = new List<Message>();
                var messageDataModelResult = new List<MessageDataModel>();


                string connectionString = Constants.KELiveConnectionString;
                string targetConnectionString = Constants.KELiveBillingConnectionString;

                using (SqlConnection connection = new SqlConnection(targetConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(Constants.GetAllAlertData, connection))
                    {
                        Console.WriteLine($"Get data from GetAllAlertData view , at {currentDateTime}");


                        command.CommandTimeout = 300;
                        //command.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {


                            #region AppInsightdependency

                            var dependency = new DependencyTelemetry(
                             "Auto-Alert",
                            "Auto-Alert-Web-Job-1-Part-1",
                            "Get data from GetAllAlertData view",
                            $"Get data from GetAllAlertData view , at {currentDateTime}",
                             currentDateTime,
                             timeSpan,
                             "200",
                             true
                             );
                            dependency.Context.Operation.Id = operationId;

                            appInsightsInitializer.TrackDependency(dependency);

                            #endregion

                            while (reader.Read())
                            {
                                var messageDataModel = new MessageDataModel();
                                var Message = "";

                                if (reader["Alert_ID"].ToString() == "42")
                                {

                                    Message = "Your " + reader["Billing_Month"].ToString() + " bill is Rs. " + reader["Billed_Amount"].ToString() + " for " + reader["Billed_Units"].ToString() + " units against a/c # " + reader["Account_No"].ToString() + ". Due date: " + reader["Due_Date"].ToString() + ". To download your bill, go to “Get KE Bill” section of KE Live App.";
                                }
                                else if (reader["Alert_ID"].ToString() == "43")
                                {
                                    Message = "Dear Customer, your " + reader["Billing_Month"].ToString() + " bill is Rs. " + reader["Billed_Amount"].ToString() + " for " + reader["Billed_Units"].ToString() + " units against a/c # " + reader["Account_No"].ToString() + ". Due date: " + reader["Due_Date"].ToString() + ". The max demand recorded in a day was " + reader["bill_demand"].ToString() + " KW against your connected load of " + reader["Connected_Load_kw"].ToString() + " KW. Extra GST of Rs. " + reader["ex_gst"].ToString() + " has also been charged. To download your bill, go to “Get KE Bill” section of KE Live App.";
                                }
                                else if (reader["Alert_ID"].ToString() == "16")
                                {
                                    Message = "Dear Customer, payment of Rs. " + reader["Billed_Amount"].ToString() + " against a/c no. " + reader["Account_No"].ToString() + " has been received. Subscribe for E-Bill with a tap on “E-Bill Subscription” section on KE Live App.";
                                }
                                if (Message != "")
                                {
                                    messageDataModel = new MessageDataModel()
                                    {
                                        Messages = new Message()
                                        {
                                            Notification = new Notification()
                                            {
                                                Title = "K-Electric",
                                                //Body = "We have updated our app, Please tap to update your app."
                                                Body = Message
                                            },
                                            Token = reader["DeviceToken"].ToString(),
                                            //Token = "cWjlFkcVL9U:APA91bF0Q13qqI-7VLRpPfWD9etf8Zl7bBbk_UszVc5jpLAbqjElj3_zSfsltxJB_2MZhUZNMqw6JsjFb7QxKZDLnS2HZiHUKO4DUGM8syGC8c_ojmeHFNQBbBwOrciVAY6SyPv3Gwhr"
                                            //Token = "ehWS8fk1TAioCfZkz7drYf:APA91bEOTrsIApT1lDHuSkYiHR7THjPY751_mQOIXzN2gQT0fFEZv7CPOWFBQP1UjnVjTmAVgUTI0CrIzZ89BsMjLKnRlhydWg0wyh7V6XwwciAhDCMFHvkAGMWRqJLErstFzwuQafdg",

                                        },
                                        MobileNo = reader["MobileNumber"].ToString(),
                                        UserId = reader["UserId"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        FirstName = reader["FirstName"].ToString(),
                                        LastName = reader["LastName"].ToString(),
                                        AccountNo = reader["Account_No"].ToString(),
                                        AlertID = reader["Alert_ID"].ToString()
                                    };
                                    messageDataModelResult.Add(messageDataModel);

                                }
                            }
                        }
                    }

                    connection.Close();
                }

                return await Task.FromResult(messageDataModelResult);
            }

            catch (Exception ex)
            {

                #region AppInsightdependency

                var dependency = new DependencyTelemetry(
                 "Auto-Alert",
                "Auto-Alert-Web-Job-1-Part-1",
               "Get data from GetAllAlertData view Exception",
                 $"Get data from GetAllAlertData view Exception, at {currentDateTime} {ex.Message.ToString()}",
                 currentDateTime,
                 timeSpan,
                "500",
                 false
                 );
                dependency.Context.Operation.Id = operationId;

                appInsightsInitializer.TrackDependency(dependency);

                #endregion

                throw;
            }
        }

        public List<List<MessageDataModel>> SplitIntoBatches(List<MessageDataModel> sourceList, int batchSize)
        {
            var batches = new List<List<MessageDataModel>>();
            for (int i = 0; i < sourceList.Count; i += batchSize)
            {
                var batch = sourceList.Skip(i).Take(batchSize).ToList();
                batches.Add(batch);
            }
            return batches;
        }



        public async Task<List<MessageDataModel>> GenerateMessagesWithRetry(string operationId)
        {
            TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            //Get the current local time in Pakistan
            DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);

            DateTime currentDate = pakistanTime.Date; // Assuming this is the current date Pakistani
            DateTime currentDateTime = pakistanTime; // Assuming this is the current date time Pakistani

            TimeSpan timeSpan = currentDateTime.Subtract(currentDateTime);
            AppInsightsInitializer appInsightsInitializer = new AppInsightsInitializer();                                
            // need to connected with Ke server database 
            Console.WriteLine($"Generate message with retry start, at {currentDateTime}");

            const int maxRetries = 3;
            int currentRetry = 0;
            Exception lastException = null;

            while (currentRetry < maxRetries)
            {
               
                try
                {
                    #region AppInsightdependency

                    var dependency = new DependencyTelemetry(
                     "Auto-Alert",
                    "Auto-Alert-Web-Job-1-Part-1",
                    "Generate message with retry start",
                    $"Generate message with retry start , at {currentDateTime}",
                     currentDateTime,
                     timeSpan,
                     "200",
                     true
                     );
                    dependency.Context.Operation.Id = operationId;

                    appInsightsInitializer.TrackDependency(dependency);

                    #endregion
                    Console.WriteLine($"Current Retry {currentRetry}, at {currentDateTime}");

                    // Attempt to execute the original GenerateMessages method
                    return await GenerateMessages(operationId);
                }
                catch (Exception ex)
                {
                    // Log the exception details
                    Console.WriteLine($"Error at retry method : {ex.Message}, at { currentDateTime}");
                    lastException = ex;

                    // Increment the retry counter
                    currentRetry++;

                    // If it's not the last retry, wait for a short duration before retrying
                    if (currentRetry < maxRetries)
                    {
                        // You can adjust the sleep duration based on your requirements
                        await Task.Delay(30000); // 1 second delay, for example
                    }
                    #region AppInsightdependency

                    var dependency = new DependencyTelemetry(
                     "Auto-Alert",
                    "Auto-Alert-Web-Job-1-Part-1",
                   "Generate message with retry Exception",
                     $"Generate message with retry Exception, at {currentDateTime} {ex.Message.ToString()}",
                     currentDateTime,
                     timeSpan,
                    "500",
                     false
                     );
                    dependency.Context.Operation.Id = operationId;
                    appInsightsInitializer.TrackDependency(dependency);

                    #endregion
                }
            }
            // If all retries fail, throw an exception or handle it accordingly
            throw lastException;
        }

        #region Sample Code
        //var result = new List<Message>();

        //for (int i = 0; i < 2; i++)
        //{
        //    var message = new Message()
        //    {
        //        Notification = new Notification()
        //        {
        //            Title = "K-Electric",
        //            Body = "Greetings! Please install updated app, and stay connected with KE",
        //        },
        //        Token = "test",
        //        //e5lAeRPpSmqy6YIkGO2cJu:APA91bELN4CaBzlq5zYcID7cqj4cYtx1kRZRelynisyUiuE2qYWwrJizM3YPlrqnf9cJVM9fCtIFJZKlHECurx7z2Y0_EaDYFohMPUME-LgAbQ_TbrdzY1rL9sug8m2oAgUoZ6w8KKxK
        //    };
        //    result.Add(message);
        //}
        #endregion
    }
}
