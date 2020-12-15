using System.Collections.Generic;

namespace ApexVpkInventory
{
    public class VpkInventory
    {
        public GameInfo GameInfo { get; private set; } = new GameInfo();

        public List<VpkFile> Packages { get; private set; } = new List<VpkFile>();
    }
}
