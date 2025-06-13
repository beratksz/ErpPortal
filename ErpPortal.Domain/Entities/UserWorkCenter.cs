namespace ErpPortal.Domain.Entities
{
    public class UserWorkCenter
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int WorkCenterId { get; set; }
        public WorkCenter WorkCenter { get; set; }
    }
} 