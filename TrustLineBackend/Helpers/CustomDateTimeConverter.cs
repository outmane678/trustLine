using Newtonsoft.Json.Converters;

namespace AnonymousComplaintsAPI.Helpers
{
    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public CustomDateTimeConverter()
        {
            DateTimeFormat = "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz '(GMT)'";
        }

        // Add a public parameterless constructor
        public CustomDateTimeConverter(string format)
        {
            DateTimeFormat = format;
        }

    }
}
