using System;
using System.Collections.Generic;
using System.Threading;

namespace Backend
{

	class DataRequestDesc
	{
		public DataRequestDesc() { }

		public DataRequestDesc(string lookupName, string url, string fallbackFilename, bool shouldCache)
		{
			Set(lookupName, url, fallbackFilename, shouldCache);
		}

		void Set(string lookupName, string url, string fallbackFilename, bool shouldCache)
		{
			LookupName = lookupName;
			URL = url;
			FallbackLocalFilename = fallbackFilename;
			ShouldCache = shouldCache;

			Success = false;
			Result = string.Empty;
		}

		// Input
		public string LookupName { get; set; }
		public string URL { get; set; }
		public string FallbackLocalFilename { get; set; }
		public bool ShouldCache { get; set; }

		// Output
		public bool Success { get; set; }
		public string Result { get; set; }
	};

	abstract class DataFetcher
	{
		class DataRequest
		{
			public Thread Thread { get; set; }
			public DataRequestDesc Desc { get; set; }
		};

		private List<DataRequestDesc> mRequests;
		private List<DataRequest> mActiveRequests;
		private bool mFetchCalled;

		abstract protected bool FetchURL(string URL, out string result);
		abstract protected bool ReadLocalFile(string filename, out string result);
		abstract protected bool WriteLocalFile(string filename, string text);

		public string GetResultText(string tag)
		{
			if (!IsReady())
				return string.Empty;

			foreach (DataRequestDesc desc in mRequests)
			{
				if (desc.LookupName == tag)
				{
					return desc.Result;
				}
			}
			return string.Empty;
		}

		public DataFetcher()
		{
			mActiveRequests = new List<DataRequest>();
			mFetchCalled = false;
		}

		public void Shutdown()
		{
			List<DataRequest> requests;
			lock (mActiveRequests)
			{
				requests = new List<DataRequest>(mActiveRequests);
			}
			foreach (DataRequest r in requests)
			{
				r.Thread.Join();
			}
		}

		public void SetRequests(List<DataRequestDesc> requests)
		{
			if (mRequests == null)
				mRequests = new List<DataRequestDesc>(requests);
		}

		public bool IsReady()
		{
			return mActiveRequests.Count == 0;
		}

		public bool FetchSuccessful()
		{
			if (!IsReady())
				return false;

			foreach( DataRequestDesc d in mRequests)
			{
				if (!d.Success)
					return false;
			}
			return true;
		}

		public void FetchAll(bool async)
		{
			if (mFetchCalled)
				return;

			foreach (DataRequestDesc desc in mRequests)
			{
				DataRequest r = new DataRequest();
				r.Desc = desc;
				mActiveRequests.Add(r);
				if (async)
				{
					r.Thread = new Thread(() => ProcessRequest(r));
					r.Thread.Start();
				}
				else
				{
					ProcessRequest(r);
				}
			}
		}

		public void FetchAllAsync()
		{
			if (mFetchCalled)
				return;
		}

		private void ProcessRequest(DataRequest request)
		{
			DataRequestDesc d = request.Desc;
			string outText;
			if (FetchURL(d.URL, out outText))
			{
				d.Result = outText;
				d.Success = true;
				if (d.ShouldCache)
				{
					WriteLocalFile(d.FallbackLocalFilename, outText);
				}
			}
			else if (ReadLocalFile(d.FallbackLocalFilename, out outText))
			{
				d.Result = outText;
				d.Success = true;
			}
			else
			{
				d.Success = false;
				d.Result = string.Empty;
				CBackendUtil.DebugPrint(string.Format("Failed to load file {0}", request.Desc.LookupName));
	        }

			lock (mActiveRequests)
			{
				mActiveRequests.Remove(request);
			}
		}

	};
}