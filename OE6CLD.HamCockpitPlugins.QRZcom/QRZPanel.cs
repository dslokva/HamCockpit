using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions; 
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using VE3NEA.HamCockpit.PluginAPI;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ComponentModel.Composition;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OE6CLD.HamCockpitPlugins.QRZcom
{
    public partial class QRZPanel: UserControl
    {
        Settings settings; 

        //plugin instance
        public QRZPlugin Plugin;

        // initialize setting strings
        public string MyGrid = "";
        public string QRZusername = "";
        public string QRZpassword = "";

        // initialize QRZ strings 
        public string QRZdburl = "http://xmldata.qrz.com/xml/current/";
        public string QRZxmlSession = "";
        public string QRZxmlError = "";
        public string QRZxmlMessage = "";

        private WebClient wc = new WebClient();
        private DataSet QRZData = new DataSet("QData");
        public bool QRZconnected = false;


        // initialize qslinfo.de REST interface
        public string QSLInfoDeURL = "https://www.qslinfo.de/api/v1/call/";
        public string QSLInfoDeSearch = "";
        public string QSLInfoJSON = "";
        private string QSLInfoURLParameter = "";
        HttpClient client = new HttpClient();


        public QRZPanel()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Captures the Enter key when in the Call Sign textbox.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                QRZLookup.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }



        /// <summary>
        /// Look up a call sign.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonQRZLookup_Click(object sender, EventArgs e)
        {

            if ((QRZconnected) && (QRZsearchcall.Text != ""))
            {
                QRZgetCallsign(QRZsearchcall.Text);
                UpdateQRZPanel();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QRZsearchcall_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonQRZLookup_Click(this, new EventArgs());
            }
        }


        // call REST API from qsl.manager.de to check for a QSL manager
        // this database is much more up-to-date than QRZ
        // use https://www.qslinfo.de/api/v1/call/{callsign} - this returns the information in JSON format

        private string QSLGetMgr (string call)
        {
            // List<char> myPattern = new List<char>() { '{' };
            List<string> found = new List<string>();
            string myPattern = "{(.*?)}";
            QSLInfoJSON = "";
            QSLInfoURLParameter = call;

            QSLInfoDeSearch = QSLInfoDeURL + QSLInfoURLParameter;

            //
            // list for data response
            //
            // Blocking call!
            // wait here until a response is received or a timeout occurs
            HttpResponseMessage response = client.GetAsync(QSLInfoDeSearch).Result;

            if (response.IsSuccessStatusCode)
            {
                // parse the response body

                var jsonString = response.Content.ReadAsStringAsync();

                jsonString.Wait();

                string myJsonResult = jsonString.Result;

                if (myJsonResult.Length > 5)
                {
                    // a vaild JSON objects looks like:
                    // [{"info":"DJ0DX","meta":"UPDATED BY DL1SBF 2019-03-15 03:37","call":"EI7JZ"}]
                    // with multiple matches result looks like this:
                    // [{"info":"W1STT (2010-2013) ","meta":"UPDATED BY DL1SBF 2013-07-03 15:06","call":"K2K"},{"info":"AB1OC (2014\/2015\/2016\/2017)","meta":"UPDATED BY DL1SBF 2017-07-01 19:03","call":"K2K"}]
                    //

                    // use RegEx to extract all strings between a pair of curly brackets {}
                    Regex myRgxLookup = new Regex(myPattern, RegexOptions.Singleline);
                    MatchCollection myLookup = myRgxLookup.Matches(myJsonResult);

                    // for every match found extract the QSL information needed
                    foreach (Match myMatch in myLookup)
                    {
                        // rebuild a valid JSON string
                        string myStripJson = "{" + myMatch.Groups[1].Value + "}";

                        // parse the JSON string and retrieve the QSL information
                        // build the string to show in the panel
                        JToken myToken = JToken.Parse(myStripJson);
                        QSLInfoJSON = QSLInfoJSON + (string)myToken.SelectToken("info") + "\n";
                    }
                }
                // we have not found an entry in qslinfo.de for this call
                else
                    QSLInfoJSON = "-";

            }
            else
            {
                MessageBox.Show("Response Code: " + response.StatusCode + " - " + response.ReasonPhrase, "QSL Manager Search");
            }

            return QSLInfoJSON;
        }

        // retrieve specific data from the QRZ data record
        private string QRZField(DataRow row, string f)
        {
            if (row.Table.Columns.Contains(f))
                return row[f].ToString();
            else
                return "";
        }



        public void callQRZ(string url)
        {
            Stream qrzstrm;
            try
            {
                QRZData.Clear();
                qrzstrm = wc.OpenRead(url);
                QRZData.ReadXml(qrzstrm, XmlReadMode.InferSchema);
                qrzstrm.Close();

                if (!QRZData.Tables.Contains("QRZDatabase"))
                {
                    MessageBox.Show("Error: failed to receive QRZDatabase object", "XML Server Error");
                    return;
                }

                DataRow dr = QRZData.Tables["QRZDatabase"].Rows[0];
                DataTable sess = QRZData.Tables["Session"];
                DataRow sr = sess.Rows[0];
                QRZxmlError = QRZField(sr, "Error");
                QRZxmlSession = QRZField(sr, "Key");
                QRZxmlMessage = QRZField(sr, "Message");

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "XML Error");

            }
            QRZconnected = (QRZxmlSession.Length > 0) ? true : false;
        }


        // log into QRZ.com to get an XML session id
        public void QRZlogin()
        {
            string url = QRZdburl + "?username=" + settings.QRZusername + ";password=" + settings.QRZpassword;
            callQRZ(url);
        }

        // use the XML session id to retrieve callsign information
        private void QRZgetCallsign(string cs)
        {
            if ((cs.Length < 3) || (!QRZconnected))
                return;

            string url = QRZdburl + "?s=" + QRZxmlSession + ";callsign=" + cs;
            callQRZ(url);
        }

        // update the QRZPanel with the infoirmation retrieved or an error message
        private void UpdateQRZPanel()
        {
            // clear the panel
            ClearAllLabels();

            // check if the data table contains valid callsign information
            // if no valid call info found, display a short message
            if (!QRZData.Tables.Contains("Callsign") || !string.IsNullOrEmpty(QRZxmlError))
            {
                QRZfullname.Text = QRZxmlError;
                return;
            }

            DataTable callTable = QRZData.Tables["Callsign"];
            DataRow dr = callTable.Rows[0];

            QRZfullname.Text = QRZField(dr, "fname") + " " + QRZField(dr, "name");
            QRZaddr1.Text = QRZField(dr, "addr1");
            QRZaddr2.Text = QRZField(dr, "addr2") + " " + QRZField(dr, "state") + " " + QRZField(dr, "zip");
            QRZcountry.Text = QRZField(dr, "country");

            QRZCQZ.Text = "CQZ: " + QRZField(dr, "cqzone");
            QRZITUZ.Text = "ITU: " + QRZField(dr, "ituzone");
            QRZIOTA.Text = "IOTA: " + QRZField(dr, "iota");

            if (QRZField(dr, "lotw") == "1")
                QRZLOTW.Text = "LoTW: Y";
            else
                QRZLOTW.Text = "LoTW: N";

            if (QRZField(dr, "eqsl") == "1")
                QRZEQSL.Text = "eQSL: Y";
            else
                QRZEQSL.Text = "eQSL: N";

            if (QRZField(dr, "mqsl") == "1")
                QRZMail.Text = "Mail: Y";
            else
                QRZMail.Text = "Mail: N";

            if (QRZField(dr, "qslmgr") == "")
                QRZQSLMgr.Text = "QRZ QSL Info: -";
            else
                QRZQSLMgr.Text = "QRZ QSL Info: " + QRZField(dr, "qslmgr");

            QSLInfoMgr.Text = "QSL Mgr (qslinfo.de): " + QSLGetMgr(QRZsearchcall.Text);

            if (QRZField(dr, "image") != "")
            {
                string myQSLImage = QRZField(dr, "image");

                QRZImage.Load(myQSLImage);

            }
 
        }


        /// <summary>
        /// Clear all the labels on the QRZ display panel.
        /// </summary>
        private void ClearAllLabels()
        {
            QRZfullname.Text = "";
            QRZaddr1.Text = "";
            QRZaddr2.Text = "";
            QRZcountry.Text = "";

            QRZCQZ.Text = "CQZ: ";
            QRZITUZ.Text = "ITU: ";
            QRZIOTA.Text = "IOTA: ";

            QRZLOTW.Text = "LoTW: ";
            QRZEQSL.Text = "eQSL: ";
            QRZMail.Text = "Mail: ";

            QRZQSLMgr.Text = "QRZ QSL Info: ";
            QSLInfoMgr.Text = "QSL Mgr (qslinfo.de):";

            QRZImage.Image = null;

        }

        private void MyQRZPanel_Load(object sender, EventArgs e)
        {
            settings = (Settings)Plugin.Settings;

            QRZsearchcall.Focus();

            if (!string.IsNullOrEmpty(settings.QRZusername) && !string.IsNullOrEmpty(settings.QRZpassword))
            {
                QRZlogin();
            }
            else
            {
                MessageBox.Show("Add QRZ.com credentials in Plugin Settings", "Missing QRZ.com credentials");
                return;
            }

        }


    }
}
