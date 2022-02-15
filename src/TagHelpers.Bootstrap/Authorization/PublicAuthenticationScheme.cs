namespace Microsoft.AspNetCore.Authentication
{
    public class PublicAuthenticationScheme
    {
        public string Name { get; }

        public PublicAuthenticationScheme(string name)
        {
            Name = name;
        }
    }
}
