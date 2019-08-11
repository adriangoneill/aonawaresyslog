/*
About Form
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
using System.Reflection;

namespace Aonaware.SyslogReceiver
{
	/// <summary>
	/// Standard about box
	/// </summary>
	public class AboutForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox pctIcon;
		private System.Windows.Forms.LinkLabel linkHome;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Label lblVersion;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AboutForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Set link URL == Text
			linkHome.LinkArea = new LinkArea(0, linkHome.Text.Length);
			linkHome.Links[0].LinkData = linkHome.Text;

			// Version
			Version ver = Assembly.GetCallingAssembly().GetName().Version;
			lblVersion.Text = "Syslog Receiver version " + ver.ToString();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.lblVersion = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pctIcon = new System.Windows.Forms.PictureBox();
            this.linkHome = new System.Windows.Forms.LinkLabel();
            this.btnOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pctIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lblVersion
            // 
            this.lblVersion.Location = new System.Drawing.Point(64, 16);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(232, 16);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "Syslog Receiver";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(64, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(168, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Copyright ©2008 Adrian O\' Neill";
            // 
            // pctIcon
            // 
            this.pctIcon.Image = ((System.Drawing.Image)(resources.GetObject("pctIcon.Image")));
            this.pctIcon.Location = new System.Drawing.Point(16, 16);
            this.pctIcon.Name = "pctIcon";
            this.pctIcon.Size = new System.Drawing.Size(32, 40);
            this.pctIcon.TabIndex = 2;
            this.pctIcon.TabStop = false;
            // 
            // linkHome
            // 
            this.linkHome.Location = new System.Drawing.Point(80, 64);
            this.linkHome.Name = "linkHome";
            this.linkHome.Size = new System.Drawing.Size(144, 16);
            this.linkHome.TabIndex = 3;
            this.linkHome.TabStop = true;
            this.linkHome.Text = "http://www.aonaware.com/";
            this.linkHome.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHome_LinkClicked);
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOk.Location = new System.Drawing.Point(224, 88);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(72, 24);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "OK";
            // 
            // AboutForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btnOk;
            this.ClientSize = new System.Drawing.Size(306, 119);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.linkHome);
            this.Controls.Add(this.pctIcon);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.Text = "About Syslog Receiver";
            ((System.ComponentModel.ISupportInitialize)(this.pctIcon)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		private void linkHome_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			linkHome.Links[linkHome.Links.IndexOf(e.Link)].Visited = true;

			string target = e.Link.LinkData as string;

			// If the value looks like a URL, navigate to it.
			if (null != target && target.StartsWith("http://"))
			{
				System.Diagnostics.Process.Start(target);
			}
		}
	}
}
