using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	public class CEmail
	{
		public string From { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public CharacterType Category { get; set; }
	}
}
