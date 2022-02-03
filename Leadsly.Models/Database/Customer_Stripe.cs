using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Database
{
    /// <summary>
    /// Represents look up table between application user id and stripe customer id
    /// </summary>
    public class Customer_Stripe
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string Customer { get; set; }
    }
}
