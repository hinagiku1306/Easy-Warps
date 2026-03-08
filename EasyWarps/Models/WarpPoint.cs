namespace EasyWarps.Models
{
    public class WarpPoint
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string LocationName { get; set; } = "";
        public int TileX { get; set; }
        public int TileY { get; set; }
        public bool IsFavorite { get; set; }
        public uint CreatedDay { get; set; }
        public long CreatedTick { get; set; }
        public uint LastUsedDay { get; set; }
        public long LastUsedTick { get; set; }
    }
}
