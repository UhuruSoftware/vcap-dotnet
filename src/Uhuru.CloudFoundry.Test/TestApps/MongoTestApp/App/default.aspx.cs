using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Data;
using System.Net;

namespace MongoTestApp
{
    public partial class _default : System.Web.UI.Page
    {
        string connString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                connString = WebConfigurationManager.AppSettings["mongoConnectionString"];

                if (!IsPostBack)
                {
                    var db = MongoDatabase.Create(connString);
                    MongoCollection collection = db.GetCollection("guids");


                    var guid = new BsonDocument();
                    guid["Date"] = DateTime.Now;
                    guid["Value"] = Guid.NewGuid().ToString();
                    collection.Insert(guid);

                    MongoCursor<BsonDocument> cursor = collection.FindAllAs<BsonDocument>();

                    DataTable dt = new DataTable();
                    dt.Columns.Add("Date", typeof(DateTime));
                    dt.Columns.Add("Value", typeof(String));

                    foreach (var item in cursor)
                    {
                        dt.Rows.Add(item["Date"], item["Value"]);
                    }

                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                }
            }
            catch(Exception ex)
            {
                throw new WebException(ex.ToString(), WebExceptionStatus.ConnectFailure);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string connString = WebConfigurationManager.AppSettings["mongoConnectionString"];

            var db = MongoDatabase.Create(connString);
            MongoCollection collection = db.GetCollection("guids");
            var guid = new BsonDocument();
            guid["Date"] = DateTime.Now;
            guid["Value"] = Guid.NewGuid().ToString();
            collection.Insert(guid);

            MongoCursor<BsonDocument> cursor = collection.FindAllAs<BsonDocument>();

            DataTable dt = new DataTable();
            dt.Columns.Add("Date", typeof (DateTime));
            dt.Columns.Add("Value", typeof(String));

            foreach (var item in cursor)
            {
                dt.Rows.Add(item["Date"], item["Value"]);
            }


            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string connString = WebConfigurationManager.AppSettings["mongoConnectionString"];

            var db = MongoDatabase.Create(connString);
            MongoCollection collection = db.GetCollection("guids");
            collection.RemoveAll();

            MongoCursor<BsonDocument> cursor = collection.FindAllAs<BsonDocument>();

            DataTable dt = new DataTable();
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Value", typeof(String));

            foreach (var item in cursor)
            {
                dt.Rows.Add(item["Date"], item["Value"]);
            }


            GridView1.DataSource = dt;
            GridView1.DataBind();
        }
    }
}