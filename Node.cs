using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kademlia
{
	public class Node
	{
		protected int _id;
		protected bool _touched;
		protected int _uploads;
		protected int _downloads;
		protected bool _alive;
		protected List<string> _log;

		public Node(int id)
		{
			_uploads = 0;
			_downloads = 0;
			_id = id;
			_touched = false;
			_alive = true;
			_log = new List<string>();
		}

		public int GetId()
		{
			return _id;
		}

		public virtual bool BroadCast(string msg)
		{
			return false;
		}

		public void PrintLog()
		{
			Console.WriteLine("Log for Node " + _id);
			Console.WriteLine("Log length : " + _log.Count);
			int count = 0;
			foreach (string str in _log)
			{
				Console.WriteLine(count + ". " + str);
				count++;
			}
		}

		public void PrintStats()
		{
			Console.WriteLine("Statistics for Node " + _id);
			Console.WriteLine("U : " + _uploads + " D : " + _downloads);
		}

		public int GetUploads()
		{
			return _uploads;
		}

		public int GetDownloads()
		{
			return _downloads;
		}

		public bool Touched()
		{
			return _touched;
		}
		public bool Respond()
		{
			return _alive;
		}

		public void Halt()
		{
			_alive = false;
		}

		public virtual void Close()
		{
		}
	}
}
