using ParkSquare.Discogs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicQuiz
{
    internal class Song
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }

        // Kapcsolat az előadóhoz
        public int ArtistId { get; set; }
        public Artist Artist { get; set; } = null!;
    }
}
