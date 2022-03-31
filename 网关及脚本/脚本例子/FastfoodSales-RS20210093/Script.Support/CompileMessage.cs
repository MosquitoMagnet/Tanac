using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Support
{
   public class CompileMessage : MarshalByRefObject
    {
		private bool _isWarning;

		private string _text;

		private int _line;

		private int _column;

		public bool IsWarning => _isWarning;

		public bool IsError => !_isWarning;

		public string Text => _text;

		public int Line => _line;

		public int Column => _column;

		public CompileMessage(string text, int line, int column, bool warning)
		{
			_text = text;
			_line = line;
			_column = column;
			_isWarning = warning;
		}

	}
}
