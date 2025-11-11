using ParkSquare.Discogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicQuiz
{
    public class DiscogsService 
    {
        public DiscogsClient Client { get; }

        public DiscogsService()
        {
            Client = new DiscogsClient(
               new HttpClient(),
               new ApiQueryBuilder(new HardCodedClientConfig())
            );
        }

        // Get the earliease release date for a song and artist
        public static int? GetEarliestReleaseYear(DiscogsClient client, string artist, string track)
        {
            var searchResult = client.SearchAllAsync(new SearchCriteria
            {
                Query = $"{artist} {track}",
                Type = "release"
            }).Result;

            if (searchResult.Results == null || searchResult.Results.Count == 0)
                return null;

            List<int> years = new List<int>();
            foreach (var result in searchResult.Results)
            {
                if (!string.IsNullOrEmpty(result.Year))
                {
                    if (int.TryParse(result.Year, out int year))
                        years.Add(year);
                }
            }

            if (years.Count == 0)
                return null;

            return years.Min();
        }


    }
}
