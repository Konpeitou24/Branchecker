using Branchecker.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Branchecker.Models {
    public class GeminiRequest {
        public Content[] contents { get; set; } = Array.Empty<Content>();
    }

}
