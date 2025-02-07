namespace DotNet9Services.Service.Extensions
{
    /// <summary>
    /// Extension class for <see cref="string"/>
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Takes a string parameter and try to make it an URI.
        /// </summary>
        /// <param name="value">The stinrg that should be a URI</param>
        /// <returns>
        /// An <see cref="Uri"/>
        /// </returns>
        /// <exception cref="ArgumentNullException" >
        /// If the string is null or empty
        /// </exception>
        /// <exception cref="UriFormatException">
        /// If the Uri is in worng format and can't convert to URI.
        /// </exception>
        public static Uri ToUri(this string value)
        {
            try
            {
                Uri uri = new(value);
                return uri;
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("The URI can't be NULL or empty.");
                throw;
            }
            catch (UriFormatException)
            {
                Console.WriteLine($"The URI {value} is in wrong format.");
                throw;
            }
        }
    }
}
