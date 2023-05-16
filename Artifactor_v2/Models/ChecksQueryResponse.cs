using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artifactor_v2.Models;

internal record ChecksQueryResponse(IList<ObservableCheck> Checks
);

