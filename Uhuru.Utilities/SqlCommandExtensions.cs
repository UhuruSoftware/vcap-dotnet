using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace Uhuru.Utilities
{
    public static class SqlCommandExtensions
    {
        public static DataSet ExecuteDataset(this SqlCommand sqlCommand)
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            dataAdapter.SelectCommand = sqlCommand;

            DataSet dataset = new DataSet();
            dataAdapter.Fill(dataset);
            return dataset;
        }

       
    }
}
