using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiBeamPlusPlus.Exceptions {
    class IllegalIndexException : Exception {

        public IllegalIndexException()
            : base() { }

        public IllegalIndexException(String message)
            : base(message) { }
    }
}
