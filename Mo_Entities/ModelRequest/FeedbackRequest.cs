using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelRequest
{
    public class FeedbackRequest
    {
        public long AccountId { get; set; }
        public long ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
