using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneEnhancementLabeling.Models
{
    public  class ImageDetail
    {
        public string Path { get; set; }
        public string Extension { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
    }
}
