
using System;

namespace DigicoinService.Model
{
    public abstract class UserBase
    {
        protected UserBase(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("userId");
            }

            UserId = userId;
        }

        public string UserId { get; private set; }
    }
}
