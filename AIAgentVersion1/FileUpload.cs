using System.ComponentModel.DataAnnotations;

namespace AIAgentVersion1
{
    public class FileUpload
    {
        [Required]
        public string? FilePath { get; set; }
    }
}
