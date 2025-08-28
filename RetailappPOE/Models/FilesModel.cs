namespace RetailappPOE.Models
{
    public class FilesModel
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTimeOffset? LastModified { get; set; }

        public string DisplaySize
        {
            get
            {
                if (Size >= 1024 * 1024)
                    return $"{Size / (1024 * 1024)} MB";
                if (Size >= 1024)
                    return $"{Size / 1024} KB";
                return $"{Size} B";
            }
        }
    }
}
