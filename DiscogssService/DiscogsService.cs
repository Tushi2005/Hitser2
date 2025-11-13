using ParkSquare.Discogs;
using ParkSquare.Discogs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MusicQuiz
{
    public class DiscogsService 
    {
        public DiscogsClient _client { get; }

        public DiscogsService()
        {
            _client = new DiscogsClient(
               new HttpClient(),
               new ApiQueryBuilder(new HardCodedClientConfig())
            );
        }

        // Get the earliease release date for a song and artist
        public async Task<int?> GetEarliestReleaseYear(string artist, string track)
        {
            var searchResult = await _client.SearchAsync(new SearchCriteria
            {
                Artist = artist,
                Track = track,
                Type = "release",
            });

            // Ha nincs találat, próbáljuk Query-vel
            if (searchResult == null || searchResult.Results.Count == 0)
            {
                searchResult = await _client.SearchAsync(new SearchCriteria
                {
                    Query = $"{artist} {track}",
                    Type = "release"
                });
            }

            if (searchResult.Results.Count == 0)
            {
                Console.WriteLine("Not found");
                return 0;
            }

            // Debug: írjuk ki az összes találatot
            //Console.WriteLine("Search results:");
            //foreach (var r in searchResult.Results)
            //{
            //    var release = await _client.GetReleaseAsync((int)r.ReleaseId);
            //    Console.WriteLine($"Year: {release.Year} - Artist: {release.ArtistsSort} - Track Name: {release.Title}");
            //}

            // párhuzamos lekérések
            var releaseTasks = searchResult.Results
                .Where(r => r.ReleaseId != null)
                .Select(async r =>
                {
                    try
                    {
                        var release = await _client.GetReleaseAsync((int)r.ReleaseId);
                        return release;
                    }
                    catch
                    {
                        return null;
                    }
                });

            var releases = await Task.WhenAll(releaseTasks);

            // évek kigyűjtése
            var years = releases
                .Where(r => r != null && r.Year > 1850)
                .Select(r => r.Year)
                .ToList();

            return years.Count > 0 ? years.Min() : 0;
        }

    }
}
