/*
Configuration
Copyright (C)2007 Adrian O' Neill

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net;

using Aonaware.SyslogShared;

namespace Aonaware.SyslogWeb
{
    public partial class Configuration : System.Web.UI.Page
    {
        private SharedData ObtainSharedData()
        {
            // Read configuration values

            int sPort = Properties.Settings.Default.ServerPort;
            if (sPort <= 0)
                throw new Exception("Invalid server port specified in configuration file");

            string serverName = Properties.Settings.Default.ServerName;
            if ((serverName == null) || (serverName.Length == 0))
                throw new Exception("Invalid server name specified in configuration file");

            string url = String.Format("http://{0}:{1}/SyslogSharedData", serverName, sPort);
            return (SharedData)Activator.GetObject(typeof(SharedData), url);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Enter means go
            ClientScript.RegisterHiddenField("__EVENTTARGET", btnSubmit.ClientID);

            if (!IsPostBack)
            {
                // Validation rules
                rngPort.MinimumValue = IPEndPoint.MinPort.ToString();
                rngPort.MaximumValue = IPEndPoint.MaxPort.ToString();
                rngCleanup.MinimumValue = SyslogConfiguration.MinRetentionPeriod.ToString();
                rngCleanup.MaximumValue = SyslogConfiguration.MaxRetentionPeriod.ToString();

                try
                {
                    SharedData sd = ObtainSharedData();

                    txtPort.Text = sd.Port.ToString();
                    txtCleanup.Text = sd.RetentionPeriod.ToString();

                    lblResults.Text = string.Empty;
                    pnlMain.Visible = true;
                }
                catch (Exception ex)
                {
                    lblResults.Text = String.Format("Could not load configuration - check that the server process"
                        + " is running and correctly configured<br>{0}", ex.Message);
                    pnlMain.Visible = false;
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!IsValid)
                return;

            try
            {
                SharedData sd = ObtainSharedData();
                sd.Port = Convert.ToInt32(txtPort.Text);
                sd.RetentionPeriod = Convert.ToInt32(txtCleanup.Text);
                sd.StoreConfiguration();

                lblResults.Text = "Configuration Saved Successfully";
            }
            catch (Exception ex)
            {
                lblResults.Text = String.Format("Could not save configuration - check that the server process"
                    + " is running and correctly configured<br>{0}", ex.Message);
            }
        }
    }
}
