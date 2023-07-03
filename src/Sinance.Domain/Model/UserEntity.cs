namespace Sinance.Domain.Model
{
    public class UserEntity : Entity
    {
        public SinanceUser User { get; set; }

        public int UserId { get; set; }
    }
}