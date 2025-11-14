using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicQuiz
{
    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Egy előadónak több száma lehet
        public List<Song> Songs { get; set; } = new();
    }
}
