using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelResponse
{
    public class FeedbackResponse
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string? AccountName { get; set; }
        public long ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
