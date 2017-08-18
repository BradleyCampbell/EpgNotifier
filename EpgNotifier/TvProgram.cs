using System.Diagnostics;

namespace EpgNotifier
{
    [DebuggerDisplay("{Title} - {Season}x{Episode}")]
    public class TvProgram
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }

        public override string ToString()
        {
            return $"{Title} - {Season}x{Episode}";
        }
    }
}
