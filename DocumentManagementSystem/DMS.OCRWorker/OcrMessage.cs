using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.OCRWorker
{
    public class OcrMessage
    {
        public int DocumentId { get; set; }
        public string ObjectName { get; set; }  // MinIO Object Name
    }
}
