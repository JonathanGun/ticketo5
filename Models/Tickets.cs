using System;
using System.ComponentModel.DataAnnotations;
using Ticketo5.Data;

namespace Ticketo5.Models
{
    public enum Status
    {
        Open, InProgress, Closed
    }

    public enum Category
    {
        Emergency, Urgent, Scheduled, NonUrgent
    }

    public class Ticket
    {
        public int ID { get; set; }
        
        [Required]
        [StringLength(60, MinimumLength = 4)]
        public string name { get; set; }
        
        public string description { get; set; }
        
        public string assignedBy { get; set; }
        
        [Required]
        public string ownedBy { get; set; }
        
        [Required]
        public Category category { get; set; }
        
        [Required]
        public Status status { get; set; }
        
        [Display]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:ddd, dd MMMM yyyy HH:mm 'GMT'}", ApplyFormatInEditMode = true)]
        public DateTime createdOn { get; set; }
    }
}
