/*
Main Form
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
using System.Data;
using System.Net;

using Aonaware.Syslog;
using Aonaware.Utility.WinForms;

namespace Aonaware.SyslogReceiver
{
	/// <summary>
	/// Main test form
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			
			InitializeComponent();

			UpdateGUI();
		}

		private void OnSyslogMessage(object sender, SyslogEventArgs e)
		{
			Console.WriteLine(e.Message.ToString());
			ShowSyslogMessage(e.SourceAddress, e.Message);
		}

		// Delegate to notify UI thread of worker thread progress
		private delegate void ShowSyslogMessageDelegate(IPAddress sourceAddress, SyslogMessage msg);

		private void ShowSyslogMessage(IPAddress sourceAddress, SyslogMessage msg)
		{
			if (lstMessages.InvokeRequired)
			{
				ShowSyslogMessageDelegate del = new ShowSyslogMessageDelegate(ShowSyslogMessage);
				this.Invoke(del, new object[] { sourceAddress, msg });
			}
			else
			{
				ListViewItem li = new ListViewItem(
					new string[] { 
									 msg.MessageTime.ToString(),
									 sourceAddress.ToString(),
									 msg.LocalTime.ToString(),
									 msg.Facility.ToString(),
									 msg.Severity.ToString(),
									 msg.Message});

				lstMessages.Items.Add(li);
				lstMessages.EnsureVisible(lstMessages.Items.Count - 1);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}

				if (_server != null)
				{
					_server.Close();
					_server = null;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.lstMessages = new System.Windows.Forms.ListView();
			this.colRecd = new System.Windows.Forms.ColumnHeader();
			this.colIP = new System.Windows.Forms.ColumnHeader();
			this.colLocalTime = new System.Windows.Forms.ColumnHeader();
			this.colFacility = new System.Windows.Forms.ColumnHeader();
			this.colSeverity = new System.Windows.Forms.ColumnHeader();
			this.colMessage = new System.Windows.Forms.ColumnHeader();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuFile = new System.Windows.Forms.MenuItem();
			this.menuFileConnect = new System.Windows.Forms.MenuItem();
			this.menuFileSend = new System.Windows.Forms.MenuItem();
			this.menuFileConfig = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuFileExit = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpAbout = new System.Windows.Forms.MenuItem();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.statusBarMainPanel = new System.Windows.Forms.StatusBarPanel();
			((System.ComponentModel.ISupportInitialize)(this.statusBarMainPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// lstMessages
			// 
			this.lstMessages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.colRecd,
																						  this.colIP,
																						  this.colLocalTime,
																						  this.colFacility,
																						  this.colSeverity,
																						  this.colMessage});
			this.lstMessages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstMessages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstMessages.Location = new System.Drawing.Point(0, 0);
			this.lstMessages.MultiSelect = false;
			this.lstMessages.Name = "lstMessages";
			this.lstMessages.Size = new System.Drawing.Size(672, 269);
			this.lstMessages.TabIndex = 3;
			this.lstMessages.View = System.Windows.Forms.View.Details;
			// 
			// colRecd
			// 
			this.colRecd.Text = "Received";
			this.colRecd.Width = 70;
			// 
			// colIP
			// 
			this.colIP.Text = "IP Address";
			this.colIP.Width = 70;
			// 
			// colLocalTime
			// 
			this.colLocalTime.Text = "Local Time";
			this.colLocalTime.Width = 70;
			// 
			// colFacility
			// 
			this.colFacility.Text = "Facility";
			this.colFacility.Width = 50;
			// 
			// colSeverity
			// 
			this.colSeverity.Text = "Severity";
			this.colSeverity.Width = 50;
			// 
			// colMessage
			// 
			this.colMessage.Text = "Message";
			this.colMessage.Width = 340;
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFile,
																					 this.menuHelp});
			// 
			// menuFile
			// 
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFileConnect,
																					 this.menuFileSend,
																					 this.menuFileConfig,
																					 this.menuItem2,
																					 this.menuFileExit});
			this.menuFile.Text = "File";
			// 
			// menuFileConnect
			// 
			this.menuFileConnect.Index = 0;
			this.menuFileConnect.Text = "Collect Events";
			this.menuFileConnect.Click += new System.EventHandler(this.menuFileConnect_Click);
			// 
			// menuFileSend
			// 
			this.menuFileSend.Index = 1;
			this.menuFileSend.Text = "Send Message...";
			this.menuFileSend.Click += new System.EventHandler(this.menuFileSend_Click);
			// 
			// menuFileConfig
			// 
			this.menuFileConfig.Index = 2;
			this.menuFileConfig.Text = "Configuration...";
			this.menuFileConfig.Click += new System.EventHandler(this.menuFileConfig_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 3;
			this.menuItem2.Text = "-";
			// 
			// menuFileExit
			// 
			this.menuFileExit.Index = 4;
			this.menuFileExit.Text = "Exit";
			this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 1;
			this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuHelpAbout});
			this.menuHelp.Text = "Help";
			// 
			// menuHelpAbout
			// 
			this.menuHelpAbout.Index = 0;
			this.menuHelpAbout.Text = "About...";
			this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 269);
			this.statusBar.Name = "statusBar";
			this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						 this.statusBarMainPanel});
			this.statusBar.Size = new System.Drawing.Size(672, 16);
			this.statusBar.TabIndex = 4;
			// 
			// statusBarMainPanel
			// 
			this.statusBarMainPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.statusBarMainPanel.Width = 10;
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(672, 285);
			this.Controls.Add(this.lstMessages);
			this.Controls.Add(this.statusBar);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Menu = this.mainMenu;
			this.Name = "MainForm";
			this.Text = "Syslog Receiver";
			((System.ComponentModel.ISupportInitialize)(this.statusBarMainPanel)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		private void Connect()
		{
			if (_server != null)
				return;

			try
			{
				IPAddress ip = _specificIP;
				if (ip == null) 
				{
					ip = SyslogServer.DefaultAddress;
				}
				_server = new SyslogServer(ip, _port);
				_server.SyslogMessageReceived += new
					SyslogServer.SyslogMessageDelegate(OnSyslogMessage);
				_server.Connect();
			}
			catch (Exception)
			{
				if (_server != null)
				{
					_server.Close();
					_server = null;
				}
				throw;
			}
		}

		private void Disconnect()
		{
			if (_server == null)
				return;

			try
			{
				_server.Close();
				_server = null;
			}
			catch (Exception)
			{
				_server = null;
				throw;
			}
		}

		private void menuFileConnect_Click(object sender, System.EventArgs e)
		{
			// Wait cursor
			using (new WaitCursor())

			try
			{
				if (_server == null)
				{
					Connect();
				}
				else
				{
					Disconnect();
				}
			} 
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error Connecting");
			}
			finally
			{
				UpdateGUI();
			}
		}

		private void menuFileSend_Click(object sender, System.EventArgs e)
		{
			SendMessage sendMessageDialog = new SendMessage();
			sendMessageDialog.Show();
		}

		private void menuFileConfig_Click(object sender, System.EventArgs e)
		{
			if (_configForm == null)
				_configForm = new ConfigForm();

			_configForm.Port = _port;
			_configForm.SpecificIP = _specificIP;

			if (_configForm.ShowDialog(this) == DialogResult.OK)
			{
				_port = _configForm.Port;
				_specificIP = _configForm.SpecificIP;

				// Reconnect if necessary
				if (_server != null)
				{
					// Wait cursor
					using (new WaitCursor())

					try
					{
						Disconnect();
						Connect();
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, ex.Message, "Error Reconnecting");
					}
					finally
					{
						UpdateGUI();
					}
				}
			}
		}

		private void menuFileExit_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void menuHelpAbout_Click(object sender, System.EventArgs e)
		{
			if (_aboutForm == null)
				_aboutForm = new AboutForm();

			_aboutForm.ShowDialog(this);
		}

		private void UpdateGUI()
		{
			menuFileConnect.Checked = (_server != null);

			if (_server == null)
			{
				statusBar.Text = "Choose File->Collect Events to begin";
			}
			else 
			{
				statusBar.Text = String.Format("Listening for syslog messages on port {0}",
					_port);
			}
		}

		private System.Windows.Forms.ListView lstMessages;
		private System.Windows.Forms.ColumnHeader colIP;
		private System.Windows.Forms.ColumnHeader colRecd;
		private System.Windows.Forms.ColumnHeader colFacility;
		private System.Windows.Forms.ColumnHeader colSeverity;
		private System.Windows.Forms.ColumnHeader colMessage;
		private System.Windows.Forms.ColumnHeader colLocalTime;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem menuFileExit;
		private System.Windows.Forms.MenuItem menuHelpAbout;
		private System.Windows.Forms.MenuItem menuFileSend;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuFileConnect;
		private System.Windows.Forms.MenuItem menuFileConfig;
		private System.Windows.Forms.MenuItem menuFile;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.StatusBarPanel statusBarMainPanel;

		private SyslogServer _server = null;
		private int _port = SyslogServer.DefaultPort;
		private IPAddress _specificIP = null;

		private AboutForm _aboutForm = null;
		private ConfigForm _configForm = null;
	}
}
