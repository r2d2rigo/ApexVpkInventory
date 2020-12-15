using System.Collections.Generic;

namespace ApexVpkInventory
{
    public class VpkFile
    {
        public string Filename { get; set; } = string.Empty;

        public List<VpkEntryInfo> Entries { get; private set; } = new List<VpkEntryInfo>();
    }
}
