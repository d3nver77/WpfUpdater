using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace UpdateCreator.Models
{
    public class Uploader
    {
        public string Protocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Path { get; set; }

        public Uploader()
        {
        }

        public Uploader(string inputString)
        {
            this.Parse(inputString);
        }

        private void Parse(string inputString)
        {
            //ftp://user:password@host:port/path
            //ftp://mobi1942:pass1942@XXX.usa.com:1234/path%20url/file
            //ftp://mobi1942:pass1942@XXX.usa.com:1234/path%20url/file
            //ftp://mobi1942@XXX.usa.com:1234/path%20url/file
            //ftp://XXX.usa.com/path%20url/file
            //https://XXX.usa.local:1234/path%20url/file
            // http://regexlib.com/RETester.aspx
            var pattern =
                @"^(?<protocol>(ht|f)tps?)\://((?<username>[a-zA-Z0-9\.\-_@]+)(\:(?<password>.+))*@)?(?<host>([a-zA-Z0-9\-_]+\.)*[a-zA-Z0-9\-_]+\.[a-zA-Z]{2,})(\:(?<port>[0-9]+))?(?<path>(/.*)*)$";
            var regex = new Regex(pattern, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
            var match = Regex.Match(inputString, pattern, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

            GroupCollection groups = match.Groups;
            this.Protocol = groups["protocol"].Value;
            this.Username = groups["username"].Value;
            this.Password = groups["password"].Value;
            this.Host = groups["host"].Value;
            this.Port = groups["port"].Value;
            this.Path = groups["path"].Value;
        }

        //todo change method to upload ftp(s), htpp(s) and maybe sftp (ssh)
        public void UploadFiles(params string[] files)
        {
            var method = this.Protocol == "ftp" ? "STOR" : "POST";
            using (WebClient client = new WebClient())
            {
                //client.Credentials = new NetworkCredential(this.Username, this.Password);
                
                foreach (var file in files)
                {
                    var host = this.Host + (string.IsNullOrEmpty(this.Port) ? string.Empty : ":" + this.Port);
                    var path = string.Format("{0}://{1}{2}", this.Protocol, host, this.Path);
                    //path = string.Format("http://www.alleasy.com/Upload/Upload");
                    client.UploadFile(path, file);
                }
                
            }
        }

        /*
        имя пользователя (login): g1006858@mvrht.com
        пароль: 22271e9a               
        сервер: node0.net2ftp.ru (россия)
        IPv4: 93.189.45.35
        порт: 21
        ftp://g1006858@mvrht.com:22271e9a@node0.net2ftp.ru:21/path1/path 2/aaa
        ftp://g1006858@mvrht.com:22271e9a@node0.net2ftp.ru:21
        имя пользователя (login): g1006858@mvrht.com
        ссылка на смену пароля: https://net2ftp.ru/?key=042b0f3dc6ef1da5755f972bcce7c1cc
        сервер: node0.net2ftp.ru (россия)
        IPv4: 93.189.45.35
        порт: 21 
        */
    }
}