using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Configuration;
using ServiceStack.Redis;
using System.Net;

namespace RedisTestApp
{
    public partial class _default : System.Web.UI.Page
    {
        string redisHost;
        int redisPort;
        string redisPassword;

        class Guids
        {
            public DateTime Date { get; set; }
            public Guid Value { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                redisHost = ConfigurationManager.AppSettings["redisHost"];
                redisPort = int.Parse(ConfigurationManager.AppSettings["redisPort"]);
                redisPassword = ConfigurationManager.AppSettings["redisPassword"];

                if (!IsPostBack)
                {
                    RedisClient redisClient = new RedisClient(redisHost, redisPort) { Password = redisPassword };
                    using (var redisGuids = redisClient.GetTypedClient<Guids>())
                    {
                        redisGuids.Store(new Guids { Date = DateTime.Now, Value = Guid.NewGuid() });
                        var allValues = redisGuids.GetAll();
                        GridView1.DataSource = allValues;
                        GridView1.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new WebException(ex.ToString(), WebExceptionStatus.ConnectFailure);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string redisHost = ConfigurationManager.AppSettings["redisHost"];
            int redisPort = int.Parse(ConfigurationManager.AppSettings["redisPort"]);
            string redisPassword = ConfigurationManager.AppSettings["redisPassword"];

            RedisClient redisClient = new RedisClient(redisHost, redisPort) { Password = redisPassword };
            using (var redisGuids = redisClient.GetTypedClient<Guids>())
            {
                redisGuids.Store(new Guids { Date = DateTime.Now, Value = Guid.NewGuid() } );
                var allValues = redisGuids.GetAll();
                GridView1.DataSource = allValues;
                GridView1.DataBind();
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string redisHost = ConfigurationManager.AppSettings["redisHost"];
            int redisPort = int.Parse(ConfigurationManager.AppSettings["redisPort"]);
            string redisPassword = ConfigurationManager.AppSettings["redisPassword"];

            RedisClient redisClient = new RedisClient(redisHost, redisPort) { Password = redisPassword };
            using (var redisGuids = redisClient.GetTypedClient<Guids>())
            {
                redisGuids.DeleteAll();
                var allValues = redisGuids.GetAll();
                GridView1.DataSource = allValues;
                GridView1.DataBind();
            }
        }
    }
}