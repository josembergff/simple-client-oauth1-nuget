
namespace simple_client_oauth1.Models
{
    public class QueryParameter
    {
        protected string name = null;
        protected string value = null;

        public QueryParameter(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name
        {
            get { return name; }
        }

        public string Value
        {
            get { return value; }
        }
    }
}
