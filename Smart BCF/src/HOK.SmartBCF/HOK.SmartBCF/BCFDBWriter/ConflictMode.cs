using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.BCFDBWriter
{
    public enum ConflictMode
    {
        ROLLBACK, ABORT, FAIL, IGNORE, REPLACE
    }
}
