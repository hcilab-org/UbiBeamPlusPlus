using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	public class Div
	{
		private Style myStyle;

		public Div()
		{
			myStyle = new Style();
		}

		public bool hasChildNodes()
		{
			return false;
		}

		public void appendChild(object child)
		{

		}

		public void removeChild(object child)
		{
		
		}

		public object lastChild { get { return null; } }

		public Style style { get { return myStyle; } }
	}
}
