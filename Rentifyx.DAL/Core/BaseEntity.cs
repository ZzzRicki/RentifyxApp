using System.ComponentModel.DataAnnotations;

namespace Rentifyx.DAL.Core
{
    public abstract class BaseEntity : AuditEntity
    {
        [Key]
        public int Id { get; set; }
    }
}

