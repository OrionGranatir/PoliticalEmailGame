using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	class CEmailTemplate
	{
		public EmailCategory Category { get; set; }
		public string From { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }

		public CEmailTemplate()
		{

		}
	}
}
