using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend
{
	class CEmailGenerator
	{
		private CEmailGenerator()
		{

		}

		public static void Init()
		{
			mInstance = new CEmailGenerator();
		}

		#region Singleton
		private static CEmailGenerator mInstance = null;
		public static CEmailGenerator Instance
		{
			get
			{
				if (mInstance == null)
					Init();
				return mInstance;
			}
		}
		#endregion

		private T RandomEnum<T>()
		{
			Array values = Enum.GetValues(typeof(T));
			Random random = new Random();
			T obj = (T)values.GetValue(random.Next(values.Length));
			return obj;
		}

		public CEmail GenerateEmail()
		{
			CEmail email = new CEmail();
			email.To = "To Text";
			email.From = "From Text";
			email.Subject = "Subject Text";
			email.Category = RandomEnum<EmailCategory>();
			email.Body = string.Format("Send This To {0}", email.Category.ToString());
			return email;
		}
	}
}
