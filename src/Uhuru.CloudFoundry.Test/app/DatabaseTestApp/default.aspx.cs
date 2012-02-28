using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;

namespace DatabaseTestApp
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            TestDBConnection();
        }

        private void TestDBConnection()
        {
            string connString = WebConfigurationManager.AppSettings["databaseTestAppDb"];
            string tableName = "test" + Guid.NewGuid().ToString().Replace("-", "");
            SqlConnection conn = new SqlConnection(connString);
            conn.Open();
            try
            {
                SqlCommand command = conn.CreateCommand();
                command.CommandText = "Create table " + tableName + " (id smallint, description varchar(50))";
                command.ExecuteNonQuery();
                for (int i = 0; i < 20; i++)
                {
                    command.CommandText = "insert into " + tableName + " (id, description) values (" + i + ", \'" + Guid.NewGuid().ToString() + "\')";
                    command.ExecuteNonQuery();
                }

                command.CommandText = "select * from " + tableName;
                SqlDataReader reader = command.ExecuteReader();
                GridView1.DataSource = reader;
                GridView1.DataBind();
            }
            catch (Exception ex)
            {
                Response.Write("oops, something went terribly wrong:" + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }
    }
}