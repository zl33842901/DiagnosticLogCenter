using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.UserInterface.Models
{
    public class CallBackEntity
    {
        public CallBackEntity(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                this.Success = true;
                this.Message = string.Empty;
            }
            else
            {
                this.Success = false;
                this.Message = msg;
            }
        }
        public bool Success { get; set; }
        public string Message { get; set; }
        public static implicit operator CallBackEntity(string s)
        {
            return new CallBackEntity(s);
        }
    }
}
