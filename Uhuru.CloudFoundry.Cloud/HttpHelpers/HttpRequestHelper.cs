using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;

namespace CloudFoundry.Net.HttpHelpers
{
    class HttpRequestHelper
    {
        private string userToken = String.Empty;
        private string proxyUser = String.Empty;

        public HttpRequestHelper(string userToken, string proxyUser)
        {
            this.userToken = userToken;
            this.proxyUser = proxyUser;
        }

        private HttpWebRequest createRequest(string url, string contentType)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);

            req.AllowAutoRedirect = false;

            if (userToken != String.Empty)
            {
                req.Headers.Add("AUTHORIZATION:" + userToken);
            }

            if (proxyUser != String.Empty)
            {
                req.Headers.Add("PROXY-USER:" + proxyUser);
            }

            if (contentType != String.Empty)
            {
                req.ContentType = contentType;
                req.Accept = contentType;
            }
            return req;
        }

        public HttpResponse Get(string path, string content_type)
        {
            HttpResponse response = new HttpResponse();

            HttpWebRequest req = createRequest(path, content_type);
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                response.Status = resp.StatusCode;
                response.Headers = resp.Headers;
                response.Body = sr.ReadToEnd().Trim();
                sr.Close();
                resp.Close();
            }
            catch (WebException e)
            {
                using (WebResponse resp = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)resp;
                    response.Status = httpResponse.StatusCode;
                    httpResponse.Close();
                }

            }
            return response;
        }

        public HttpResponse HttUploadZip(string path, string file, NameValueCollection formData)
        {
            string boundary = "625809";//DateTime.Now.Ticks.ToString("x");
            HttpWebRequest httpWebRequest = createRequest(path, "multipart/form-data; boundary=" + boundary);
            httpWebRequest.Accept = "*/*; q=0.5, application/xml";
            httpWebRequest.Headers.Add("Accept-Encoding:gzip, deflate");
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = true;


            Stream memStream = new System.IO.MemoryStream();
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            byte[] boundarybytesend = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");


            string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

            memStream.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"application\"; filename=\"{0}\"\r\nContent-Type: application/zip\r\n\r\n";
            string header = string.Format(headerTemplate, Path.GetFileName(file));
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);

            memStream.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[1024];

            int bytesRead = 0;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                memStream.Write(buffer, 0, bytesRead);
            }

            fileStream.Close();

            foreach (string key in formData.Keys)
            {
                string formitem = string.Format(formdataTemplate, key, formData[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                memStream.Write(formitembytes, 0, formitembytes.Length);
            }

            memStream.Write(boundarybytesend, 0, boundarybytesend.Length);
            httpWebRequest.ContentLength = memStream.Length;
            Stream requestStream = httpWebRequest.GetRequestStream();
            memStream.Position = 0;
            byte[] tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();
            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();

            HttpResponse response = new HttpResponse();
            try
            {
                HttpWebResponse resp = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                response.Status = resp.StatusCode;
                response.Headers = resp.Headers;
                response.Body = sr.ReadToEnd().Trim();
                sr.Close();
                resp.Close();
            }
            catch (WebException e)
            {
                using (WebResponse resp = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)resp;
                    response.Status = httpResponse.StatusCode;
                    httpResponse.Close();
                }
            }

            return response;
        }

        public HttpResponse Post(string path, string body, string content_type)
        {
            HttpResponse response = new HttpResponse();

            try
            {
                HttpWebRequest req = createRequest(path, content_type);
                req.Method = "POST";
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(body);
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length); //Push it out there
                os.Close();

            
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                response.Status = resp.StatusCode;
                response.Headers = resp.Headers;
                response.Body = sr.ReadToEnd().Trim();
                sr.Close();
                resp.Close();
            }
            catch (WebException e)
            {
                using (WebResponse resp = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)resp;
                    response.Status = httpResponse == null? HttpStatusCode.NotFound : httpResponse.StatusCode;
                    if (httpResponse != null) httpResponse.Close();
                }
            }

            return response;
        }

        public HttpResponse Put(string path, string body, string content_type)
        {
            HttpResponse response = new HttpResponse();
            HttpWebRequest req = createRequest(path, content_type);
            req.Method = "PUT";
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(body);
            req.ContentLength = bytes.Length;
            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();

            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                response.Status = resp.StatusCode;
                response.Headers = resp.Headers;
                response.Body = sr.ReadToEnd().Trim();
                sr.Close();
                resp.Close();
            }
            catch (WebException e)
            {
                using (WebResponse resp = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)resp;
                    response.Status = httpResponse.StatusCode;
                    httpResponse.Close();
                }

            }
            return response;
        }

        public HttpResponse Delete(string path)
        {
            HttpResponse response = new HttpResponse();
            HttpWebRequest req = createRequest(path, "");
            req.Method = "DELETE";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                response.Status = resp.StatusCode;
                response.Headers = resp.Headers;
                response.Body = sr.ReadToEnd().Trim();
                sr.Close();
                resp.Close();
            }
            catch (WebException e)
            {
                using (WebResponse resp = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)resp;
                    response.Status = httpResponse.StatusCode;
                    httpResponse.Close();
                }
            }
            return response;
        }
    }
}
