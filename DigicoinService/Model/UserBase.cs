
namespace DigicoinService.Model
{
    public abstract class UserBase
    {
        protected UserBase(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; private set; }
    }
}
