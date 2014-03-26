using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRHost
{
	class Program
	{
		static void Main(string[] args)
		{
			// requires netsh http add urlacl url=http://*:8081/ user=[machine_name]\[user_name]
			string url = "http://*:8081";
			using (WebApp.Start(url))
			{
				Console.WriteLine("Server running on {0}", url);
				Console.ReadLine();
			}
		}
	}
}
