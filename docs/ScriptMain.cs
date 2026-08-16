#region Help:  Introduction to the script task
/* The Script Task allows you to perform virtually any operation that can be accomplished in
 * a .Net application within the context of an Integration Services control flow. 
 * 
 * Expand the other regions which have "Help" prefixes for examples of specific ways to use
 * Integration Services features within this script task. */
#endregion


using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;

#region Namespaces
using System;
using System.Data;
using Microsoft.SqlServer.Dts.Runtime;
using System.Windows.Forms;
using System.Data.SqlClient;
#endregion

namespace ST_d292ba917ade4849b84093fd59b60679
{
    /// <summary>
    /// ScriptMain is the entry point class of the script.  Do not change the name, attributes,
    /// or parent of this class.
    /// </summary>
	[Microsoft.SqlServer.Dts.Tasks.ScriptTask.SSISScriptTaskEntryPointAttribute]
	public partial class ScriptMain : Microsoft.SqlServer.Dts.Tasks.ScriptTask.VSTARTScriptObjectModelBase
	{
        #region Help:  Using Integration Services variables and parameters in a script
        /* To use a variable in this script, first ensure that the variable has been added to 
         * either the list contained in the ReadOnlyVariables property or the list contained in 
         * the ReadWriteVariables property of this script task, according to whether or not your
         * code needs to write to the variable.  To add the variable, save this script, close this instance of
         * Visual Studio, and update the ReadOnlyVariables and 
         * ReadWriteVariables properties in the Script Transformation Editor window.
         * To use a parameter in this script, follow the same steps. Parameters are always read-only.
         * 
         * Example of reading from a variable:
         *  DateTime startTime = (DateTime) Dts.Variables["System::StartTime"].Value;
         * 
         * Example of writing to a variable:
         *  Dts.Variables["User::myStringVariable"].Value = "new value";
         * 
         * Example of reading from a package parameter:
         *  int batchId = (int) Dts.Variables["$Package::batchId"].Value;
         *  
         * Example of reading from a project parameter:
         *  int batchId = (int) Dts.Variables["$Project::batchId"].Value;
         * 
         * Example of reading from a sensitive project parameter:
         *  int batchId = (int) Dts.Variables["$Project::batchId"].GetSensitiveValue();
         * */

        #endregion

        #region Help:  Firing Integration Services events from a script
        /* This script task can fire events for logging purposes.
         * 
         * Example of firing an error event:
         *  Dts.Events.FireError(18, "Process Values", "Bad value", "", 0);
         * 
         * Example of firing an information event:
         *  Dts.Events.FireInformation(3, "Process Values", "Processing has started", "", 0, ref fireAgain)
         * 
         * Example of firing a warning event:
         *  Dts.Events.FireWarning(14, "Process Values", "No values received for input", "", 0);
         * */
        #endregion

        #region Help:  Using Integration Services connection managers in a script
        /* Some types of connection managers can be used in this script task.  See the topic 
         * "Working with Connection Managers Programatically" for details.
         * 
         * Example of using an ADO.Net connection manager:
         *  object rawConnection = Dts.Connections["Sales DB"].AcquireConnection(Dts.Transaction);
         *  SqlConnection myADONETConnection = (SqlConnection)rawConnection;
         *  //Use the connection in some code here, then release the connection
         *  Dts.Connections["Sales DB"].ReleaseConnection(rawConnection);
         *
         * Example of using a File connection manager
         *  object rawConnection = Dts.Connections["Prices.zip"].AcquireConnection(Dts.Transaction);
         *  string filePath = (string)rawConnection;
         *  //Use the connection in some code here, then release the connection
         *  Dts.Connections["Prices.zip"].ReleaseConnection(rawConnection);
         * */
        #endregion


		/// <summary>
        /// This method is called when this script task executes in the control flow.
        /// Before returning from this method, set the value of Dts.TaskResult to indicate success or failure.
        /// To open Help, press F1.
        /// </summary>
		public void Main()
		{
            // TODO: Add your code here
            try 
            {
                //call api here
                CallApi();
                Dts.TaskResult = (int)ScriptResults.Success;
            }
            catch (Exception ex)
            {
                Dts.Events.FireError(0, "Script Task", ex.Message + Environment.NewLine + ex.StackTrace, string.Empty, 0);
                Dts.TaskResult = (int)ScriptResults.Failure;
                return;
            }
            
		}

        private void CallApi()
        {
            var apiEndPoint = Dts.Variables["ApiEndPoint"].Value.ToString();

            var salesOrders = new List<SalesOrder>();

            var nonExistingRegions = new HashSet<string>();
            var existingRegions = new HashSet<string>();
            var existingOrderIds = new HashSet<int?>();
            var dupOrderIds = new HashSet<int?>();

            var connString = Dts.Connections["AdoDotNetConnMngr"].ConnectionString;

            
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                //get existing regions from the database
                using (var cmd = new SqlCommand("SELECT DISTINCT TRIM(Description) AS Description FROM tblRegion", conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while(rdr.Read())
                        {
                            existingRegions.Add(rdr.GetString(0).Trim());
                        }
                    }
                }

                //get orderids
                using (var cmd = new SqlCommand("SELECT DISTINCT OrderID FROM tblSalesOrder", conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while(rdr.Read())
                        {
                            existingOrderIds.Add(rdr.GetInt32(0));
                        }
                    }
                }
            }

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(apiEndPoint).Result;

                //makesure response is success
                response.EnsureSuccessStatusCode();

                var data = response.Content.ReadAsStringAsync().Result;

                //deserialize json data
                JsonElement jsonData = JsonSerializer.Deserialize<JsonElement>(data);

                foreach (var item in jsonData.EnumerateArray())
                {
                    var salesOrder = new SalesOrder
                    {
                        OrderID = int.TryParse(item.TryGetProperty("orderID", out JsonElement jsonOrderId) ? jsonOrderId.GetString().Trim() : string.Empty, out int orderid) ? orderid : (int?)null,
                        OrderDate = DateTime.TryParse(item.TryGetProperty("orderDate", out JsonElement jsonOrderDate) ? jsonOrderDate.GetString().Trim() : string.Empty,out DateTime orderdate) ? orderdate : (DateTime?)null,
                        CustomerName = item.TryGetProperty("customerName", out JsonElement jsonCustomerName) ? jsonCustomerName.GetString().Trim() : string.Empty,
                        Product = item.TryGetProperty("product", out JsonElement jsonProduct) ? jsonProduct.GetString().Trim() : string.Empty,
                        Quantity = int.TryParse(item.TryGetProperty("quantity", out JsonElement jsonQuantity) ? jsonQuantity.GetString().Trim() : string.Empty, out int quantity) ? quantity : (int?)null,
                        UnitPrice = decimal.TryParse(item.TryGetProperty("unitPrice", out JsonElement jsonUnitPrice) ? jsonUnitPrice.GetString().Trim() : string.Empty, out decimal unitprice) ? unitprice : (decimal?)null,
                        Region = item.TryGetProperty("region", out JsonElement jsonRegion) ? jsonRegion.GetString().Trim() : string.Empty
                    };

                    //validate each record

                    var exceptedOut = false;

                    //check if order id is null or 0
                    if (!salesOrder.OrderID.HasValue || salesOrder.OrderID <= 0)
                    {
                        exceptedOut = true;
                    }

                    //check if date is null
                    if (!salesOrder.OrderDate.HasValue)
                    {
                        exceptedOut = true;
                    }

                    //check if product is empty
                    if (string.IsNullOrEmpty(salesOrder.Product))
                    {
                        exceptedOut = true;
                    }

                    //check quantity
                    if (!salesOrder.Quantity.HasValue || salesOrder.Quantity <= 0)
                    {
                        exceptedOut = true;
                    }

                    //check unit price value
                    if (!salesOrder.UnitPrice.HasValue)
                    {
                        exceptedOut = true;
                    }

                    //check region value
                    if (string.IsNullOrEmpty(salesOrder.Region) || !existingRegions.Contains(salesOrder.Region))
                    {
                        exceptedOut = true;

                        if (!string.IsNullOrEmpty(salesOrder.Region))
                        {
                            nonExistingRegions.Add(salesOrder.Region);
                        }
                    }

                    //check if orderid already exists
                    if (salesOrder.OrderID.HasValue && existingOrderIds.Contains(salesOrder.OrderID))
                    {
                        exceptedOut = true;
                    }

                    //check if orderid is duplicate in jsondata
                    if (salesOrder.OrderID.HasValue)
                    {
                        if (!dupOrderIds.Add(salesOrder.OrderID))
                        {
                            exceptedOut = true;
                        }
                    }

                    salesOrder.IsExceptedOut = exceptedOut;

                    //add to list
                    salesOrders.Add(salesOrder);

                }

                //[{"orderID":"1001","orderDate":"2026-05-01","customerName":"Company A","product":"Printer Paper","quantity":"5","unitPrice":"4.50","region

            }

            //insert to database (salesorder and non-existing regions)
            InsertToDatabase(salesOrders, nonExistingRegions);
        }

        private void InsertToDatabase(List<SalesOrder> orders, HashSet<string> nonexistingregion)
        {
            var connString = Dts.Connections["AdoDotNetConnMngr"].ConnectionString;

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                //insert non-existing regions
                if (nonexistingregion.Count > 0)
                {
                    DataTable dt = new DataTable("Regions");

                    dt.Columns.Add("Description", typeof(string));

                    foreach (var item in nonexistingregion)
                    {
                        dt.Rows.Add(item);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        using (var bulkcopy = new SqlBulkCopy(conn))
                        {
                            bulkcopy.DestinationTableName = "tblRegion";
                            bulkcopy.ColumnMappings.Add("Description", "Description");

                            bulkcopy.WriteToServer(dt);
                        }
                    }
                }

                //insert to either ssalesorder table or exception table
                if(orders.Count > 0)
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        var salesOrders = new List<SalesOrder>();

                        //1 -> exceptedout
                        //2 -> salesorder
                        salesOrders = i == 1 ? orders.FindAll(o => o.IsExceptedOut) : orders.FindAll(o => !o.IsExceptedOut);

                        //create datatable
                        DataTable dt = new DataTable("SalesOrder");
                        dt.Columns.Add("OrderID", typeof(int));
                        dt.Columns.Add("OrderDate", typeof(DateTime));
                        dt.Columns.Add("CustomerName", typeof(string));
                        dt.Columns.Add("Product", typeof(string));
                        dt.Columns.Add("Quantity", typeof(int));
                        dt.Columns.Add("UnitPrice", typeof(decimal));
                        dt.Columns.Add("Region", typeof(string));

                        foreach(var o in salesOrders)
                        {
                            dt.Rows.Add
                                (
                                    o.OrderID
                                    , o.OrderDate
                                    , o.CustomerName
                                    , o.Product
                                    , o.Quantity
                                    , o.UnitPrice
                                    , o.Region
                                );
                        }

                        //bulkinsert if has records
                        if (dt.Rows.Count > 0)
                        {
                            using (var bulkcopy = new SqlBulkCopy(conn))
                            {
                                bulkcopy.DestinationTableName = i == 1 ? "_tblExceptions" : "tblSalesOrder";
                                bulkcopy.ColumnMappings.Add("OrderID", "OrderID");
                                bulkcopy.ColumnMappings.Add("OrderDate", "OrderDate");
                                bulkcopy.ColumnMappings.Add("CustomerName", "CustomerName");
                                bulkcopy.ColumnMappings.Add("Product", "Product");
                                bulkcopy.ColumnMappings.Add("Quantity", "Quantity");
                                bulkcopy.ColumnMappings.Add("UnitPrice", "UnitPrice");
                                bulkcopy.ColumnMappings.Add("Region", "Region");

                                bulkcopy.WriteToServer(dt);

                            }
                        }                        

                    }

                }
            }
        }


        #region ScriptResults declaration
        /// <summary>
        /// This enum provides a convenient shorthand within the scope of this class for setting the
        /// result of the script.
        /// 
        /// This code was generated automatically.
        /// </summary>
        enum ScriptResults
        {
            Success = Microsoft.SqlServer.Dts.Runtime.DTSExecResult.Success,
            Failure = Microsoft.SqlServer.Dts.Runtime.DTSExecResult.Failure
        };
        #endregion

	}

    internal class SalesOrder
    {
        public int? OrderID { get; set; }  
        public DateTime? OrderDate { get; set; }  
        public string CustomerName { get; set; } = "";
        public string Product { get; set; } = "";
        public int? Quantity { get; set; }  
        public decimal? UnitPrice { get; set; }
        public string Region { get; set; } = "";
        public bool IsExceptedOut { get; set; } = false;
    }
}