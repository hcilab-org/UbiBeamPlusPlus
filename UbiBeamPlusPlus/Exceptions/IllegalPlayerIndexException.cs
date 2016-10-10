using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiBeamPlusPlus.Exceptions {
    class IllegalPlayerIndexException : IllegalIndexException {

        public IllegalPlayerIndexException()
            : base() { }

        public IllegalPlayerIndexException(String message)
            : base(message) { }
    }
}
