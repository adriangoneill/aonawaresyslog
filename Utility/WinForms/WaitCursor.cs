/*
Wait cursor
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
using System.Windows.Forms;

namespace Aonaware.Utility.WinForms
{
	/// <summary>
	/// MFC style CWaitCursor class.
	/// Displays wait cursor on screen - use within using block
	/// </summary>
	/// 
	public class WaitCursor : WaitCursorBase
	{
		public WaitCursor() : base (Cursors.WaitCursor)
		{
		}
	}

	public abstract class WaitCursorBase : IDisposable
	{
		private Cursor _saved = null;

		protected WaitCursorBase (Cursor newCursor)
		{
			_saved = Cursor.Current;
			Cursor.Current = newCursor;
		}

		public void Dispose()
		{
			Cursor.Current = _saved;
		}
	}
}
