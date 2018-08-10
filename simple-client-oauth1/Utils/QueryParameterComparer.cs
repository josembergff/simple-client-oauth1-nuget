using simple_client_oauth1.Models;
using System.Collections.Generic;

namespace simple_client_oauth1.Utils
{
    public class QueryParameterComparer : IComparer<QueryParameter>
    {
        public int Compare(QueryParameter x, QueryParameter y)
        {
            if (x.Name == y.Name)
            {
                return string.Compare(x.Value, y.Value);
            }
            else
            {
                return string.Compare(x.Name, y.Name);
            }
        }
    }
}
