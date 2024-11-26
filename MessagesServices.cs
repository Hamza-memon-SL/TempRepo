using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Auto_Alert_2.Model;
using System.Runtime.Remoting.Messaging;

namespace Auto_Alert_2
{
    public class MessagesServices
    {
        public async Task< List<MessageDataModel>> GenerateMessages()
        {

            TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            //Get the current local time in Pakistan
            DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);

            DateTime currentDate = pakistanTime.Date; // Assuming this is the current date Pakistani
            DateTime currentDateTime = pakistanTime; // Assuming this is the current date time Pakistani
            // need to connected with Ke server database 
            // create a similar sp for this for there env
            // check the dynamic message
            var result = new List<Message>();
            var messageDataModelResult = new List<MessageDataModel>();

            
            string connectionString = Constants.KELiveConnectionString;
            string targetConnectionString = Constants.KELiveBillingConnectionString;

            using (SqlConnection connection = new SqlConnection(targetConnectionString))
            {
                connection.Open();
 
                using (SqlCommand command = new SqlCommand(Constants.GetAllAlertDataPaymentReminders, connection))
                {
                    Console.WriteLine($"Get data from GetAllAlertDataPaymentReminders view , at {currentDateTime}");


                    command.CommandTimeout = 300;
                    //command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var messageDataModel = new MessageDataModel();
                      
                           //TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                           // //Get the current local time in Pakistan
                           //DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);


                            DateTime currentDatePakistani = pakistanTime.Date; // Assuming this is the current date Pakistani

                            //DateTime dueDate = new DateTime(2023, 9, 30); // Example due date
                            var dueDate = Convert.ToDateTime(reader["Due_Date"].ToString());
                            //Calculate the difference
                           TimeSpan difference = dueDate - currentDatePakistani;

                            //Access the difference in days, hours, minutes, etc.
                            int daysDifference = difference.Days;
                            var Message = "";

                            if (reader["Alert_ID"].ToString() == "61")
                            {
                                if (daysDifference == 0)
                                {
                                    Message = "Dear Customer, your " + reader["Billing_Month"].ToString() + " bill is Rs. " + reader["Billed_Amount"].ToString() + " for a/c # " + reader["Account_No"].ToString() + " is due today. Please pay before " + reader["Due_Date"].ToString() + " to avoid late payment surcharge of Rs. " + reader["Is_Late_Payment_Surcharge"].ToString() + ". Tap “Pay Now” button on KE Live App to pay your bill now. Kindly disregard this message if you have already paid your amount.";
                                }
                                else if (daysDifference == 1)
                                {
                                    Message = "Payment reminder, your due date is tomorrow for " + reader["Billing_Month"].ToString() + " bill against a/c # " + reader["Account_No"].ToString() + " with amount Rs. " + reader["Billed_Amount"].ToString() + ". Please pay before " + reader["Due_Date"].ToString() + " to avoid late payment surcharge. Tap “Pay Now” button on KE Live App to pay your bill now. Kindly disregard this message if you have already paid your amount.";
                                }
                                else if (daysDifference == 3)
                                {
                                    Message = "Dear Customer, your " + reader["Billing_Month"].ToString() + " bill is Rs. " + reader["Billed_Amount"].ToString() + " for a/c # " + reader["Account_No"].ToString() + " is due in " + daysDifference + " days. Please pay before " + reader["Due_Date"].ToString() + ". To pay your bill, tap on “Pay Now” button on KE Live App. Kindly disregard this message if you have already paid your amount.";
                                }
                                else if (daysDifference == 5)
                                {
                                    Message = "Dear Customer, your " + reader["Billing_Month"].ToString() + " bill is Rs. " + reader["Billed_Amount"].ToString() + " for a/c # " + reader["Account_No"].ToString() + " is due soon. Please pay before " + reader["Due_Date"].ToString() + ". To pay your bill, tap on “Pay Now” button on KE Live App. Kindly disregard this message if you have already paid your amount.";
                                }
                                else if (daysDifference == 7)
                                {
                                    Message = "Dear Customer, your " + reader["Billing_Month"].ToString() + " bill is Rs. " + reader["Billed_Amount"].ToString() + " for a/c # " + reader["Account_No"].ToString() + ". Please pay before " + reader["Due_Date"].ToString() + ". To pay your bill, tap on “Pay Now” button on KE Live App. Kindly disregard this message if you have already paid your amount.";
                                }
                            }
                            else if (reader["Alert_ID"].ToString() == "79")
                            {
                                Message = "Aap k A/C " + reader["Account_No"].ToString() + " Ka " + reader["Billing_Month"].ToString() + " bill Rs. " + reader["Billed_Amount"].ToString() + " hai. Bjli munqata hune se bachnay k liye wajibat " + reader["Due_Date"].ToString() + " tk ada krain.";
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
            }

            return await Task.FromResult(messageDataModelResult);
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


        public async Task<List<MessageDataModel>> GenerateMessagesWithRetry()
        {
            TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            //Get the current local time in Pakistan
            DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);

            DateTime currentDate = pakistanTime.Date; // Assuming this is the current date Pakistani
            DateTime currentDateTime = pakistanTime; // Assuming this is the current date time Pakistani

            TimeSpan timeSpan = currentDateTime.Subtract(currentDateTime);
            //AppInsightsInitializer appInsightsInitializer = new AppInsightsInitializer();
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

                    //var dependency = new DependencyTelemetry(
                    // "Auto-Alert",
                    //"Auto-Alert-Web-Job-1-Part-1",
                    //"Generate message with retry start",
                    //$"Generate message with retry start , at {currentDateTime}",
                    // currentDateTime,
                    // timeSpan,
                    // "200",
                    // true
                    // );
                    //dependency.Context.Operation.Id = operationId;

                    //appInsightsInitializer.TrackDependency(dependency);

                    #endregion
                    Console.WriteLine($"Current Retry {currentRetry}, at {currentDateTime}");

                    // Attempt to execute the original GenerateMessages method
                    return await GenerateMessages();
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

                   // var dependency = new DependencyTelemetry(
                   //  "Auto-Alert",
                   // "Auto-Alert-Web-Job-1-Part-1",
                   //"Generate message with retry Exception",
                   //  $"Generate message with retry Exception, at {currentDateTime} {ex.Message.ToString()}",
                   //  currentDateTime,
                   //  timeSpan,
                   // "500",
                   //  false
                   //  );
                   // dependency.Context.Operation.Id = operationId;
                   // appInsightsInitializer.TrackDependency(dependency);

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
