using System;
using System.Collections.Generic;

namespace Backend
{
	public enum EmailCategory
	{
		Unknown,
		Bill,
		PrivateServer,
		PublicServer,
		CampaignManager,
		Trash
	};

	class CEnumUtility
	{
		private static bool isInited = false;
		private static void InitOnce()
		{
			if( isInited == false )
			{
				isInited = true;

				mCategoryLookup = new Dictionary<string, EmailCategory>();
				string[] names = Enum.GetNames(typeof(EmailCategory));
                for (int i = 0; i < names.Length; i++)
				{
					mCategoryLookup.Add(names[i].ToLower(), (EmailCategory)i);
				}

				mCategoryLookup.Add( string.Empty, EmailCategory.Unknown );
			}
		}

		private static Dictionary<string, EmailCategory> mCategoryLookup;
		public static bool EmailCategoryFromString(string text, out EmailCategory outCategory)
		{
			InitOnce();
			return mCategoryLookup.TryGetValue(text.ToLower(), out outCategory);
		}
	}
}