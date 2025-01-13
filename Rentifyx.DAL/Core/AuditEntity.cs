using System.ComponentModel.DataAnnotations;

namespace Rentifyx.DAL.Core
{
    public abstract class AuditEntity
    {
        public string? CreationUser { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string? DeleteUser { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool Deleted { get; set; }
        public AuditEntity()
        {
            CreationDate = DateTime.Now;
            Deleted = false;
        }
    }
}
