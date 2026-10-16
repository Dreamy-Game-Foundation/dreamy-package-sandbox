using System;
using System.Collections.Generic;
using Dreamy.Datasave;

namespace Dreamy.Template.Demo
{
    // Read-only v2 migration DTO. New resource mutations go through the shared Economy owner.
    [Serializable]
    public sealed class FoundationProgressionInventorySave : SaveData
    {
        public long Count;
        public Dictionary<string, string> Receipts = new();
    }
}
