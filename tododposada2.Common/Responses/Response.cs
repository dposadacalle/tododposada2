﻿using System;
using System.Collections.Generic;
using System.Text;

namespace tododposada2.Common.Responses
{
    public class Response
    {
        public bool IsSucess { get; set;  }
        public string Messages { get; set; }  
        public object Result { get; set; }

    }
}