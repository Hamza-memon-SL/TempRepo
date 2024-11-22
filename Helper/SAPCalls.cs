using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using secureacceptance.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;

namespace secureacceptance.Helper
{
    public class SAPCalls
    {
        private string _biliingDetailsUrl = ConfigurationManager.AppSettings.Get("BiliingDetails");
        private string constring = ConfigurationManager.ConnectionStrings["KEServiceContext"].ConnectionString;
        private string constring2 = ConfigurationManager.ConnectionStrings["KELoggingCS"].ConnectionString;
        SqlConnection conn, conn2;
        SqlCommand comm, comm2;


        private string DownloadDataCustom(string url)
        {
            string result = string.Empty;
            try
            {
                using (WebClient wc = GetClient())
                {
                    result = wc.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                ExceptionalHandling.LogError(ex, "DownloadDataCustom");
            }

            return result;
        }

        private WebClient GetClient()
        {
            WebClient webClient = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            return webClient;
        }

        public JsonResponseConverter GetBillingDetails(string ContractNo)
        {
            JsonResponseConverter requestDetail = new JsonResponseConverter();
            string result = string.Empty;
            try
            {
                result = DownloadDataCustom(string.Format(_biliingDetailsUrl, ContractNo));

                if (result != null)
                {
                    JObject parsed = JObject.Parse(result);
                    requestDetail = JsonConvert.DeserializeObject<JsonResponseConverter>(parsed.ToString());
                   
                }
            }

            catch (Exception ex)
            {
                 ExceptionalHandling.LogError(ex, "GetBillingDetails");
            }

            return requestDetail;
        }

        public User GetUserDetail(string token, string email)
        {
            conn = new SqlConnection(constring);
            conn.Open();
            User user = new User();
            try
            {
                comm = new SqlCommand();
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.StoredProcedure;
                comm.CommandText = "aspnet_UserDetail_FromToken";
                comm.Parameters.AddWithValue("@Token", token);
                comm.Parameters.AddWithValue("@Email", email);
                
                using (SqlDataReader reader = comm.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user.Email = Convert.ToString(reader["LoweredEmail"]);
                        user.FirstName = Convert.ToString(reader["FirstName"]);
                        user.LastName = Convert.ToString(reader["LastName"]);
                        user.MobileNo = Convert.ToString(reader["UserMobile"]);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionalHandling.LogError(ex, "GetUserDetail");
            }
            finally
            {
                conn.Close();
            }
            return user;
        }

        public bool InsertPaymentLog(ResponseValue responseValue)
        {
            bool returnParameter = false;
            conn2 = new SqlConnection(constring2);
            conn2.Open();

            try
            {
                comm2 = new SqlCommand();
                comm2.Connection = conn2;
                comm2.CommandType = System.Data.CommandType.StoredProcedure;
                comm2.CommandText = "aspnet_Insert_PaymentLog";
                comm2.Parameters.AddWithValue("@EmailAddress", Convert.ToString(responseValue.email));
                comm2.Parameters.AddWithValue("@AccountNumber", Convert.ToString(responseValue.account_no));
                comm2.Parameters.AddWithValue("@ContractNumber", Convert.ToString(responseValue.contract_no));
                comm2.Parameters.AddWithValue("@TransactionId", responseValue.transaction_id_value);
                comm2.Parameters.AddWithValue("@PaymentMethod", responseValue.paymentMethod);
                comm2.Parameters.AddWithValue("@ActualAmount", responseValue.actualAmount);
                comm2.Parameters.AddWithValue("@Charges", responseValue.bankCharge);
                comm2.Parameters.AddWithValue("@PaidAmount", responseValue.paidAmount);
                comm2.Parameters.AddWithValue("@BillingMonth", responseValue.billingMonth);
                comm2.Parameters.AddWithValue("@BillingYear", responseValue.billingYear);
                comm2.Parameters.AddWithValue("@Decision", responseValue.decision_value);
                comm2.Parameters.AddWithValue("@ReasonCode", responseValue.reason_code_value);
                comm2.Parameters.AddWithValue("@Message", responseValue.message_value);
                comm2.Parameters.AddWithValue("@ReffernceNo", responseValue.req_reference_number_value);
                comm2.Parameters.AddWithValue("@TransactionDate", responseValue.signed_date_time_value);

                comm2.ExecuteNonQuery();
                returnParameter = true;
            }
            catch (Exception ex)
            {
                ExceptionalHandling.LogError(ex, "InsertPaymentLog");
            }
            finally
            {
                conn2.Close();
            }
            
            return returnParameter;
        }
    }
}