using Moserware.Skills;

namespace HLTVRatingTest
{
	class PlayerData
	{
		public Rating Rating { get; set; }

		public int Kills { get; set; }

		public int Deaths { get; set; }

		public PlayerData()
		{
			Rating = MatrixEvaluator.GameInfo.DefaultRating;
			Kills = 0;
			Deaths = 0;
		}
	}
}
