using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAMLib.Models
{
  public class WMIClass
  {
    //public enum ClassStatus {Unknown, OK, Empty, Error}

    public string Name { get; set; }
    public string Catagory { get; set; }
    public string Status { get; set; }
  }
}
