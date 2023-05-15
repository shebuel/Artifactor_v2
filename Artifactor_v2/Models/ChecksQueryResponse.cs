using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artifactor_v2.Models;

class ChecksQueryResponse
{
    public IList<ObservableCheck>? Checks
    {
    get; set; }

    public ChecksQueryResponse(IList<ObservableCheck> observableChecks)
    {
        Checks = observableChecks;
    }
}
