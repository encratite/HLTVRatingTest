using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Moserware.Skills;

namespace HLTVRatingTest
{
	class MatrixEvaluator
	{
		private const double Mu = 25.0;
		// private const double Mu = 1000.0;
        private const double Sigma = Mu / 3.0;
		private const double Beta = Sigma / 2.0;
		// private const double Beta = 3.0 * Sigma / 2.0;
		private const double Tau = Sigma / 100.0;
		// private const double Tau = 3.0 * Sigma / 100.0;
        // private const double DrawProbability = 0.10;
		private const double DrawProbability = 0.0;

		public static GameInfo GameInfo = new GameInfo(Mu, Sigma, Beta, Tau, DrawProbability);

		private Dictionary<string, PlayerData> _PlayerData = new Dictionary<string, PlayerData>();

		public void Run(string matrixDirectory)
		{
			var directory = new DirectoryInfo(matrixDirectory);
			var files = directory.GetFiles().OrderBy(file => file.CreationTime);
			foreach (var file in files)
			{
				EvaluateMatrix(file.FullName);
			}
		}

		public void Write(string path)
		{
			using (var writer = new StreamWriter(path))
			{
				var pairs = _PlayerData.AsEnumerable().OrderByDescending(pair => pair.Value.Rating.ConservativeRating);
				foreach (var pair in pairs)
				{
					string name = pair.Key;
					var playerData = pair.Value;
					var rating = playerData.Rating;
					writer.WriteLine("{0}: {1:F2} TrueSkill (mu {2:F2}, sigma {3:F2}), {4} kills - {5} deaths", name, rating.ConservativeRating, rating.Mean, rating.StandardDeviation, playerData.Kills, playerData.Deaths);
				}
			}
		}

		private void EvaluateMatrix(string path)
		{
			string content = File.ReadAllText(path);
			var pattern = new Regex("([^\"]+) killed ([^\"]+) (\\d+) times, [^\"]+ killed [^\"]+ (\\d+) times");
			var matches = pattern.Matches(content);
			if (matches.Count == 0)
			{
				Console.WriteLine("Failed to process {0}", path);
				return;
			}
			foreach (Match match in matches)
			{
				var groups = match.Groups;
				string name1 = groups[1].Value;
				string name2 = groups[2].Value;
				int kills1 = int.Parse(groups[3].Value);
				int kills2 = int.Parse(groups[4].Value);
				var player1 = new Player(name1);
				var player2 = new Player(name2);
				while (kills1 + kills2 > 0)
				{
					var playerData1 = GetPlayerData(name1);
					var playerData2 = GetPlayerData(name2);
					var team1 = new Team(player1, playerData1.Rating);
					var team2 = new Team(player2, playerData2.Rating);
					var teams = Teams.Concat(team1, team2);
					kills1 = AdjustRatings(kills1, true, GameInfo, teams, name1, name2);
					kills2 = AdjustRatings(kills2, false, GameInfo, teams, name1, name2);
				}
			}
			Console.WriteLine("Processed {0}", path);
		}

		private int AdjustRatings(int kills, bool firstTeamWon, GameInfo gameInfo, IEnumerable<IDictionary<Player, Rating>> teams, string name1, string name2)
		{
			if (kills > 0)
			{
				const int team1 = 1;
				const int team2 = 2;
				int winner = firstTeamWon ? team1 : team2;
				int loser = firstTeamWon ? team2 : team1;
				string killerName = firstTeamWon ? name1 : name2;
				string victimName = firstTeamWon ? name2 : name1;
				var killer = GetPlayerData(killerName);
				var victim = GetPlayerData(victimName);
				killer.Kills++;
				victim.Deaths++;
				var newRatings = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, winner, loser);
				foreach (var pair in newRatings)
				{
					string name = (string)pair.Key.Id;
					var playerData = _PlayerData[name];
					playerData.Rating = pair.Value;
				}
				kills--;
			}
			return kills;
		}

		private PlayerData GetPlayerData(string name)
		{
			PlayerData playerData;
			if (!_PlayerData.TryGetValue(name, out playerData))
			{
				playerData = new PlayerData();
				_PlayerData[name] = playerData;
			}
			return playerData;
		}
	}
}
