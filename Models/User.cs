using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Models
{
    public class User : IdentityUser
    {
        public User()
        {
        }
        public int _Index { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime Registred { get; set; }
        public string Document { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public bool? Gender { get; set; }
        //-----------------------------------------------
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        //-----------------------------------------------
        public virtual ICollection<AddressBook> AddressBook { get; set; }
        public virtual AddressBook SelectedAddress { get; set; }
        //-----------------------------------------------
        public virtual ICollection<Message> UserSend { get; set; }
        public virtual ICollection<Message> AdminSend { get; set; }
        public virtual ICollection<SharedMessageUser> AdminSharedMessage { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
