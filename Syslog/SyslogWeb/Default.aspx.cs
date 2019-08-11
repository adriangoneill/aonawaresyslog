/*
Main
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
using System.Data.OleDb;

namespace Aonaware.SyslogWeb
{
    public partial class Default : System.Web.UI.Page
    {
        ICollection CreateDataSource()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Received Time", typeof(DateTime));
            dt.Columns.Add("Address", typeof(string));
            dt.Columns.Add("Local Time", typeof(DateTime));
            dt.Columns.Add("Facility", typeof(string));
            dt.Columns.Add("Severity", typeof(string));
            dt.Columns.Add("Message", typeof(string));

            using (OleDbConnection conn = GetConnection())
            {
                conn.Open();

                OleDbCommand cmdPage = new OleDbCommand("SyslogPagingResults", conn);
                cmdPage.CommandType = CommandType.StoredProcedure;

                OleDbParameter pageIndex = new OleDbParameter("pageIndex",
                    OleDbType.Integer, 0);
                pageIndex.Direction = ParameterDirection.Input;
                pageIndex.Value = dgMain.CurrentPageIndex + 1;
                cmdPage.Parameters.Add(pageIndex);

                OleDbParameter pageCount = new OleDbParameter("pageCount",
                    OleDbType.Integer, 0);
                pageCount.Direction = ParameterDirection.Input;
                pageCount.Value = dgMain.PageSize;
                cmdPage.Parameters.Add(pageCount);

                if (dgMain.CurrentPageIndex > 0)
                {
                    object li = Session[_currentRows];
                    if (li != null)
                    {
                        OleDbParameter lastIndex = new OleDbParameter("lastIndex",
                            OleDbType.BigInt, 0);
                        lastIndex.Direction = ParameterDirection.Input;
                        lastIndex.Value = (long)li;
                        cmdPage.Parameters.Add(lastIndex);
                    }
                }

                DataRow newRow;
                OleDbDataReader dr = cmdPage.ExecuteReader();
                while (dr.Read())
                {
                    newRow = dt.NewRow();

                    newRow[0] = dr["receivedTime"];
                    newRow[1] = dr["address"];
                    newRow[2] = dr["localTime"];
                    newRow[3] = dr["facilityCode"];
                    newRow[4] = dr["severityCode"];
                    newRow[5] = dr["message"];

                    dt.Rows.Add(newRow);
                }
            }

            DataView dv = new DataView(dt);
            return dv;
        }


        private OleDbConnection GetConnection()
        {
            // Database connection
            string connString = Properties.Settings.Default.DbConnection;
            if (connString == null || connString.Length == 0)
                throw new Exception("No database connection string specified");

            return new OleDbConnection(connString);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Don't allow this page to be cached
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now);

            if (!IsPostBack)
            {
                // Get how many lines to display per page

                int lines = Properties.Settings.Default.DisplayLines;
                if (lines > 0)
                    dgMain.PageSize = lines;

                using (OleDbConnection conn = GetConnection())
                {
                    conn.Open();

                    // Work out how many rows there are
                    OleDbCommand cmd = new OleDbCommand("SELECT count(id) as ct, MAX(id) as mx from syslog",
                        conn);
                    OleDbDataReader rd = cmd.ExecuteReader();
                    if (!rd.Read())
                        throw new Exception("Unable to read from database!");

                    int count = rd.GetInt32(0);
                    long max = count > 0 ? rd.GetInt64(1) : 0;
                    rd.Close();

                    dgMain.VirtualItemCount = count;
                    Session[_currentRows] = max;
                    conn.Close();
                }

                dgMain.DataSource = CreateDataSource();
                dgMain.DataBind();
            }
        }

        private const string _currentRows = "currentRows";

        protected void dgMain_PageIndexChanged(object source, DataGridPageChangedEventArgs e)
        {
            dgMain.CurrentPageIndex = e.NewPageIndex;

            dgMain.DataSource = CreateDataSource();
            dgMain.DataBind();
        }
    }
}
