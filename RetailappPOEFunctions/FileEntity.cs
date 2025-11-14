using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailappPOEFunctions;

namespace RetailappPOEFunctions

{
    public class FileEntity
    {
        public string? Name { get; set; }
        public long Size { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public string? DisplaySize { get; set; }
    }
}