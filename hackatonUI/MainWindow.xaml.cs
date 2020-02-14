using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hackatonUI
{
    enum ScreenType
    {
        StartView = 0,
        NormalFileView,
        HashFileView
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            if(_currentScreen == ScreenType.NormalFileView)
            {
                _showScreen(ScreenType.StartView, false);
            }
            if(_currentScreen == ScreenType.HashFileView)
            {
                if (File.Exists(_currentNormalFile))
                    _showFile(_currentNormalFile, false);
                else
                    _showScreen(ScreenType.StartView, false);
            }
        }

        private void btnRightUp_Click(object sender, RoutedEventArgs e)
        {
            if(_currentScreen == ScreenType.StartView)
            {
                _showScreen(ScreenType.NormalFileView, true);
            }
        }

        private void btnRightDown_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScreen == ScreenType.StartView)
            {
                _showScreen(ScreenType.HashFileView, true);
            }
            else if (_currentScreen == ScreenType.NormalFileView)
            {
                // user pressed to hash the file
                String outputFile = "output.json";
                hackaton.Logic.hashFile(_currentNormalFile, outputFile);
                _showFile(outputFile, true);
            }
            else if(_currentScreen == ScreenType.HashFileView)
            {
                _showScreen(ScreenType.StartView, false);
            }
            
        }

        void _showFile(string file, bool hashed)
        {
            if (string.IsNullOrEmpty(file))
                return;

            if (hashed)
                _currentHashedFile = file; 
            else
                _currentNormalFile = file;

            this.contentPresenter.Content = new FileViewerControl(file);
            _setTitle(file);

            if (hashed)
                _showScreen(ScreenType.HashFileView, false);
            else
                _showScreen(ScreenType.NormalFileView, false);
        }

        void _showScreen(ScreenType screen, bool promptOpenFile)
        {
            
            if (screen == ScreenType.StartView)
            {
                this.contentPresenter.Content = new StartViewerControl();

                _setButtonContent(btnLeft, "");
                _setButtonContent(btnRightUp, "Open File");
                _setButtonContent(btnRightDown, "Open Hashed File");
            }
            else if (screen == ScreenType.NormalFileView)
            {
                string normalFile = _currentNormalFile;
                if(promptOpenFile)
                    normalFile = _selectFile(false);

                if (string.IsNullOrEmpty(normalFile))
                    return;
                _currentNormalFile = normalFile;
                this.contentPresenter.Content = new FileViewerControl(normalFile);
                _setTitle(normalFile);

                _setButtonContent(btnLeft, "Back");
                _setButtonContent(btnRightUp, "");
                _setButtonContent(btnRightDown, "Process");
            }
            else if(screen == ScreenType.HashFileView)
            {
                string hashedFile = _currentHashedFile;
                if(promptOpenFile)
                    hashedFile = _selectFile(true);

                if (string.IsNullOrEmpty(hashedFile))
                    return;

                _currentHashedFile = hashedFile;
                this.contentPresenter.Content = new FileViewerControl(hashedFile);
                _setTitle(hashedFile);

                _setButtonContent(btnLeft, "Back");
                _setButtonContent(btnRightUp, "Upload");
                _setButtonContent(btnRightDown, "Restart");
            }

            _currentScreen = screen;
        }

        void _setTitle(string file)
        {
            this.Title = System.IO.Path.GetFileName(file);
        }

        void _setButtonContent(Button button, string content)
        {
            if (string.IsNullOrEmpty(content))
                button.Visibility = Visibility.Hidden;
            else
            {
                button.Visibility = Visibility.Visible;
                button.Content = content;
            }
        }

        string _selectFile(bool hashed)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            if (hashed)
            {
                dlg.DefaultExt = ".json";
                dlg.Filter = "json Files (*.json)|*.json";
            }
            else
            {
                dlg.DefaultExt = ".xml";
                dlg.Filter = "xml Files (*.xml)|*.xml|txt Files (*.txt)|*.txt";
            }

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
                // Open document 
                return dlg.FileName;
            else
                return "";
        }

        ScreenType _currentScreen;
        string _currentNormalFile = "";
        string _currentHashedFile = "";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _showScreen(ScreenType.StartView, false);
        }

        void _uploadToURI(string input)
        {
            #region generate token

            var timeInSeconds = ConvertToUnixTimestamp(DateTime.Now);// parseInt((new Date()).getTime() / 1000);
            var sigString = "appid=567&appkey=567&nonce=654&timestamp=" + timeInSeconds;
            var signature = hackaton.Logic.ComputeSha256Hash(sigString);

            string url = "";
            var client = new RestClient("url");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Content-Length", "200");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined",
                "{\n    \"appid\": \"2345\",\n" +
                        "\"nonce\": \"6143519628774960\",\n    " +
                        "\"signature\": \"" + signature + "\",\n    " +
                        "\"timestamp\": \"" + timeInSeconds + "\"\n}", ParameterType.RequestBody);


            IRestResponse response = client.Execute(request);
            string answer = response.Content;
            JObject o = JObject.Parse(answer);
            string token = "";
            if (o.ContainsKey("data"))
            {
                var data = o["data"];
                token = data["token"].ToString();
            }
            
            #endregion

            #region generate vechain id
            
            client = new RestClient(url);
            request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Content-Length", "50");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Postman-Token", "6a61155a-5019-4bfb-8f08-9c84343b0238,5c7c9b5f-c8aa-43bd-90f9-cc8bc7f59c0c");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("User-Agent", "PostmanRuntime/7.18.0");
            request.AddHeader("x-api-token", token);
            request.AddHeader("Content-Type", "application/json");

            
            int quantity = 1;
            string requestNr = getNewNumber().ToString();
            request.AddParameter("undefined", "{\n   \"" + 
                                "requestNo\":\"" +  requestNr +  "\",\n   \"" + 
                                "quantity\":" + quantity + "\n}", ParameterType.RequestBody);
            

            o = JObject.Parse(response.Content);
            string status = "GENERATING";
            while(status == "GENERATING")
            {
                System.Threading.Thread.Sleep(500);
                response = client.Execute(request);
                o = JObject.Parse(response.Content);
                if (o.ContainsKey("data"))
                {
                    var data = o["data"];
                    try
                    {
                        status = data["status"].ToString();
                    }
                    catch
                    {
                        status = "";
                    }
                }
                else
                    status = "";
            }
            

            string vechainID = response.Content;
            o = JObject.Parse(response.Content);
            var vidList = o["data"]["vidList"].ToString().Replace("\r\n", "").Replace("[", "").Replace("]", "");

            #endregion

            #region hash the file

            if (string.IsNullOrEmpty(_currentHashedFile))
                _currentHashedFile = System.IO.Path.Combine(Environment.CurrentDirectory, "output.json");

            string hashFile = computeHashFile(_currentHashedFile);
            string newFileName = System.IO.Path.Combine(Environment.CurrentDirectory, hashFile + ".json");
            
            File.Copy(_currentHashedFile,   newFileName, true);

            #endregion

            #region upload hash file to blockchain

            client = new RestClient(url);
            request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Content-Length", "295");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");

            request.AddHeader("User-Agent", "PostmanRuntime/7.18.0");

            request.AddHeader("Content-Type", "application/json");

            request.AddHeader("x-api-token", token);

            Console.WriteLine( "{\"data\": [{" +
                                "\"dataHash\":" + "\"0x" + hashFile + "\"," +
                                "\"vid\":" + vidList + "}]," +
                                "\"requestNo\": \"" + getNewNumber() + "\"," +
                                "\"uid\":\"0xc813a1384b0dc0123aa7929a11717e20365d24a40358acad085e07751ba8de34\"\n}");
            
            request.AddParameter(
                "undefined",
                "{\"data\": [{" +
                                "\"dataHash\":" + "\"0x" + hashFile + "\"," +
                                "\"vid\":" + vidList + "}]," +
                                "\"requestNo\": \"" + getNewNumber() + "\"," +
                                "\"uid\":\"0xc813a1384b0dc0123aa7929a11717e20365d24a40358acad085e07751ba8de34\"\n}", 
                                ParameterType.RequestBody);

            


            response = client.Execute(request);

            Console.WriteLine(response.Content);

            o = JObject.Parse(response.Content);
            status = "PROCESSING";
            while (status == "PROCESSING")
            {
                System.Threading.Thread.Sleep(500);
                response = client.Execute(request);
                o = JObject.Parse(response.Content);
                if (o.ContainsKey("data"))
                {
                    var data = o["data"];
                    try
                    {
                        status = data["status"].ToString();
                    }
                    catch
                    {
                        status = "";
                    }
                }
                else
                    status = "";
            }

            string transactionID = response.Content.ToString();

            Console.WriteLine("transaction id = " + transactionID);

            #endregion
        }

        string computeHashFile(string file)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var line in File.ReadAllLines(file))
                builder.AppendLine(line);

            return hackaton.Logic.ComputeOnlySha256Hash(builder.ToString());
        }

        private void btnDebug_Click(object sender, RoutedEventArgs e)
        {
            Mouse.SetCursor(Cursors.Wait);
            btnDebug.IsEnabled = false;
            _uploadToURI("");
            btnDebug.IsEnabled = true;
            Mouse.SetCursor(Cursors.Arrow);
        }

        

        private int getNewNumber()
        {
            int res = -1;
            string file = "counter.txt";
            if (File.Exists(file))
            {
                int current = int.Parse(File.ReadAllLines(file)[0]);
                res = current + 1;
            }
            else
                res = 1;

            using (StreamWriter writer = new StreamWriter(file))
                writer.WriteLine(res);

            return res;
        }

        double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
    }
}
