using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	class CEmailTemplateGroup
	{
		public string Name { get; set; }
		public List<CEmailTemplate> Emails = new List<CEmailTemplate>();
	}

	class CEmailTemplate
	{
		public CharacterType Category { get; set; }
		public string From { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string EmailGroupText { get; set; }
		public int Weight { get; set; }
	}
}
