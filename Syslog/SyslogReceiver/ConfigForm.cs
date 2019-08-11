/*
Configuration Form
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
using Aonaware.Utility.Networking;

namespace Aonaware.SyslogReceiver
{
	/// <summary>
	/// Configuration form
	/// </summary>
	public class ConfigForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.RadioButton radListenAll;
		private System.Windows.Forms.RadioButton radListenSpecific;
		private System.Windows.Forms.ComboBox comInterfaces;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.ErrorProvider errorProvider;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ConfigForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			txtPort.Text = Port.ToString();

			// Populate list of IP addresses
			IPAddress[] localAddresses = LocalIPAddresses.AllAddresses();
			if (localAddresses != null)
			{
				foreach (IPAddress ip in localAddresses) 
				{
					comInterfaces.Items.Add(ip);
					if (ip == SpecificIP)
						comInterfaces.SelectedItem = ip;
				}

				if (comInterfaces.SelectedItem == null && (comInterfaces.Items.Count > 0))
					comInterfaces.SelectedItem = comInterfaces.Items[0];
			}
			
			radListenAll.Checked = (SpecificIP == null);
			radListenSpecific.Checked = !radListenAll.Checked;

			UpdateGUI();
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

		private void UpdateGUI()
		{
			comInterfaces.Enabled = (radListenSpecific.Checked);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.radListenAll = new System.Windows.Forms.RadioButton();
			this.radListenSpecific = new System.Windows.Forms.RadioButton();
			this.comInterfaces = new System.Windows.Forms.ComboBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.errorProvider = new System.Windows.Forms.ErrorProvider();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.comInterfaces);
			this.groupBox1.Controls.Add(this.radListenSpecific);
			this.groupBox1.Controls.Add(this.radListenAll);
			this.groupBox1.Controls.Add(this.txtPort);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(256, 136);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Server Settings";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 24);
			this.label1.TabIndex = 0;
			this.label1.Text = "Port:";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(80, 24);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(64, 20);
			this.txtPort.TabIndex = 1;
			this.txtPort.Text = "";
			// 
			// radListenAll
			// 
			this.radListenAll.Location = new System.Drawing.Point(16, 56);
			this.radListenAll.Name = "radListenAll";
			this.radListenAll.Size = new System.Drawing.Size(184, 16);
			this.radListenAll.TabIndex = 2;
			this.radListenAll.Text = "Listen on All Local Addresses";
			this.radListenAll.CheckedChanged += new System.EventHandler(this.radListenAll_CheckedChanged);
			// 
			// radListenSpecific
			// 
			this.radListenSpecific.Location = new System.Drawing.Point(16, 80);
			this.radListenSpecific.Name = "radListenSpecific";
			this.radListenSpecific.Size = new System.Drawing.Size(192, 16);
			this.radListenSpecific.TabIndex = 3;
			this.radListenSpecific.Text = "Listen on Specific Addresss:";
			this.radListenSpecific.CheckedChanged += new System.EventHandler(this.radListenSpecific_CheckedChanged);
			// 
			// comInterfaces
			// 
			this.comInterfaces.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comInterfaces.Location = new System.Drawing.Point(32, 104);
			this.comInterfaces.Name = "comInterfaces";
			this.comInterfaces.Size = new System.Drawing.Size(200, 21);
			this.comInterfaces.TabIndex = 4;
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(120, 152);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(64, 24);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(200, 152);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(64, 24);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			// 
			// errorProvider
			// 
			this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider.ContainerControl = this;
			// 
			// ConfigForm
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(272, 183);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConfigForm";
			this.Text = "Configuration";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Displayed port
		/// </summary>
		public int Port 
		{
			get
			{
				return _port;
			}
			set
			{
				_port = value;
			}
		}

		/// <summary>
		/// Specific IP address to listen to
		/// </summary>
		public IPAddress SpecificIP 
		{
			get
			{
				return _specificIP;
			}
			set
			{
				_specificIP = value;
			}
		}

		private int _port = SyslogServer.DefaultPort;
		private IPAddress _specificIP = null;

		private void radListenAll_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateGUI();	
		}

		private void radListenSpecific_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateGUI();
		}

		private bool ValidInput()
		{
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

			if (radListenSpecific.Checked)
			{
				if (comInterfaces.SelectedItem == null)
				{
					errorProvider.SetError(comInterfaces, "Select a valid interface");
					return false;
				}
			}
			errorProvider.SetError(comInterfaces, string.Empty);

			return true;
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			if (ValidInput()) 
			{
				_port = Convert.ToInt32(txtPort.Text);
				if (radListenAll.Checked)
				{
					_specificIP = null;
				} 
				else
				{
					_specificIP = (IPAddress) comInterfaces.SelectedItem;
				}
				
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
