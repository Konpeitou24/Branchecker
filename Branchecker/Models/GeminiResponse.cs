using Branchecker.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Branchecker.Models {
    public class GeminiResponse {
        public Candidate[] candidates { get; set; } = Array.Empty<Candidate>();
    }

}
