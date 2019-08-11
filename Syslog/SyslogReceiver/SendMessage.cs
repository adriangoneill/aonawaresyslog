/*
Send Message Form
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;

using Aonaware.Syslog;
using Aonaware.Utility.WinForms;

namespace Aonaware.SyslogReceiver
{
	/// <summary>
	/// Dialog allowing you to send message
	/// </summary>
	public class SendMessage : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtServer;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtMessage;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox comFacility;
		private System.Windows.Forms.ComboBox comSeverity;
		private System.Windows.Forms.ErrorProvider errorProvider;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SendMessage()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Populate drop down lists
			foreach (SyslogMessage.FacilityCode fc in
				Enum.GetValues(typeof(SyslogMessage.FacilityCode)))
			{
				comFacility.Items.Add(fc);
			}
			comFacility.SelectedItem = SyslogMessage.DefaultFacility;

			foreach (SyslogMessage.SeverityCode sc in
				Enum.GetValues(typeof(SyslogMessage.SeverityCode)))
			{
				comSeverity.Items.Add(sc);
			}
			comSeverity.SelectedItem = SyslogMessage.DefaultSeverity;

			txtPort.Text = SyslogUdpClient.DefaultPort.ToString();
			txtServer.Text = "localhost";
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtServer = new System.Windows.Forms.TextBox();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtMessage = new System.Windows.Forms.TextBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.comSeverity = new System.Windows.Forms.ComboBox();
			this.comFacility = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.errorProvider = new System.Windows.Forms.ErrorProvider();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Server:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Port:";
			// 
			// txtServer
			// 
			this.txtServer.Location = new System.Drawing.Point(88, 24);
			this.txtServer.Name = "txtServer";
			this.txtServer.Size = new System.Drawing.Size(168, 20);
			this.txtServer.TabIndex = 1;
			this.txtServer.Text = "";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(88, 56);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(64, 20);
			this.txtPort.TabIndex = 2;
			this.txtPort.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(120, 16);
			this.label3.TabIndex = 4;
			this.label3.Text = "Message:";
			// 
			// txtMessage
			// 
			this.txtMessage.Location = new System.Drawing.Point(32, 224);
			this.txtMessage.Name = "txtMessage";
			this.txtMessage.Size = new System.Drawing.Size(232, 20);
			this.txtMessage.TabIndex = 3;
			this.txtMessage.Text = "";
			// 
			// btnSend
			// 
			this.btnSend.Location = new System.Drawing.Point(136, 272);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(64, 24);
			this.btnSend.TabIndex = 4;
			this.btnSend.Text = "Send";
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(216, 272);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(64, 24);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "Close";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.txtPort);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.txtServer);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(272, 88);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Server Details";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.comSeverity);
			this.groupBox2.Controls.Add(this.comFacility);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Location = new System.Drawing.Point(8, 112);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(272, 152);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Message Details";
			// 
			// comSeverity
			// 
			this.comSeverity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comSeverity.Location = new System.Drawing.Point(88, 56);
			this.comSeverity.Name = "comSeverity";
			this.comSeverity.Size = new System.Drawing.Size(168, 21);
			this.comSeverity.TabIndex = 3;
			// 
			// comFacility
			// 
			this.comFacility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comFacility.Location = new System.Drawing.Point(88, 24);
			this.comFacility.Name = "comFacility";
			this.comFacility.Size = new System.Drawing.Size(168, 21);
			this.comFacility.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 56);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(48, 16);
			this.label5.TabIndex = 2;
			this.label5.Text = "Severity:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(48, 16);
			this.label4.TabIndex = 0;
			this.label4.Text = "Facility:";
			// 
			// errorProvider
			// 
			this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider.ContainerControl = this;
			// 
			// SendMessage
			// 
			this.AcceptButton = this.btnSend;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(290, 303);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.txtMessage);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.groupBox2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "SendMessage";
			this.Text = "Send Syslog Message";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private bool ValidateRequiredField(TextBox control)
		{
			if (control.Text.Length < 1)
			{
				errorProvider.SetError(control, "Please complete this field.");
				return false;
			}
   
			errorProvider.SetError(control, string.Empty);
			return true;
		}

		private bool ValidInput()
		{
			bool validInput = ValidateRequiredField(txtServer) &&
				ValidateRequiredField(txtPort) &&
				ValidateRequiredField(txtMessage);
			
			if (!validInput)
				return false;

			try
			{
				int port = Convert.ToInt32(txtPort.Text);
				if (port <=0 || port > 65535)
					throw new Exception("Invalid Port");
			}
			catch (Exception)
			{
				errorProvider.SetError(txtPort, "Invalid Port Number");
				return false;
			}
			errorProvider.SetError(txtPort, string.Empty);

			return true;
		}

		private void btnSend_Click(object sender, System.EventArgs e)
		{
			if (ValidInput()) 
			{
				// Wait cursor
				using (new WaitCursor())

				try
				{
					// Lookup server IP Address
					IPHostEntry IPList = Dns.GetHostEntry(txtServer.Text);
					if ((IPList == null) || (IPList.AddressList.Length == 0))
						throw new Exception("Unable to resolve address for host " + 
							txtServer.Text);

					IPAddress destAddress = IPList.AddressList[0];

					// Get our host name
					string hostName = Dns.GetHostName();

					// Create message
					SyslogMessage msg = new SyslogMessage(hostName,
						txtMessage.Text,
						(SyslogMessage.FacilityCode) comFacility.SelectedItem,
						(SyslogMessage.SeverityCode) comSeverity.SelectedItem,
						DateTime.Now);

					// Send message
					using (SyslogClient client = new SyslogUdpClient())
					{
                        client.Connect(destAddress, Convert.ToInt32(txtPort.Text));
						client.Send(msg);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(this, "Unable to send message:\n" + ex.Message);
				}
			}
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
