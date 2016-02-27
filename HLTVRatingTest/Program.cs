using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLTVRatingTest
{
	class Program
	{
		static void Main(string[] arguments)
		{
			if (arguments.Length != 3)
				return;
			string seedPath = arguments[0];
			string outputDirectory = arguments[1];
			string ratingOutputPath = arguments[2];
			var downloader = new KillMatrixDownloader(outputDirectory);
			// downloader.Run(seedPath);
			var evaluator = new MatrixEvaluator();
			evaluator.Run(outputDirectory);
			evaluator.Write(ratingOutputPath);
		}
	}
}
