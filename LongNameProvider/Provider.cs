using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace LongNameProvider
{
    [CmdletProvider("LongNameProvider", ProviderCapabilities.Include | ProviderCapabilities.ShouldProcess)]
    public class LongNameProvider : NavigationCmdletProvider
    {
        protected override bool IsValidPath(string path)
        {
            throw new NotImplementedException();
        }

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            base.CopyItem(path, copyPath, recurse);
        }

        protected override object CopyItemDynamicParameters(string path, string destination, bool recurse)
        {
            return base.CopyItemDynamicParameters(path, destination, recurse);
        }
    }
}
