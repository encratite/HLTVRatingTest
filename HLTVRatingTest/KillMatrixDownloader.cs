using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace HLTVRatingTest
{
	class KillMatrixDownloader
	{
		private string _OutputDirectory;

		public KillMatrixDownloader(string outputDirectory)
		{
			_OutputDirectory = outputDirectory;
		}

		public void Run(string seedPath)
		{
			string content = File.ReadAllText(seedPath);
			var document = new HtmlDocument();
			document.LoadHtml(content);
			var nodes = document.DocumentNode.SelectNodes("//a[contains(@href, '/match/')]");
			var links = nodes.Reverse().ToList();
			foreach (var node in links)
			{
				string link = string.Format("http://www.hltv.org{0}", node.Attributes["href"].Value);
				var pattern = new Regex(@"\d+");
				var match = pattern.Match(link);
				int id = int.Parse(match.Value);
				DownloadKillMatrix(id);
			}
		}

		private string Download(string uri)
		{
			var client = new WebClient();
			string output = client.DownloadString(new Uri(uri));
			return output;
		}

		private void DownloadKillMatrix(int id)
		{
			string uri = string.Format("http://www.hltv.org/?pageid=113&matchid={0}&killMatrix=1&clean=1", id);
			string content = Download(uri);
			string filename = string.Format("{0}.html", id);
			string path = Path.Combine(_OutputDirectory, filename);
			if (File.Exists(path))
			{
				Console.WriteLine("Skipping {0}", path);
				return;
			}
			if (!content.Contains("killed"))
			{
				Console.WriteLine("Failed to download {0}", path);
				return;
			}
			File.WriteAllText(path, content);
			Console.WriteLine("Wrote {0}", path);
		}
	}
}
