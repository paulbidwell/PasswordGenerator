using PasswordGenerator.Core.Interfaces.Sets;

namespace PasswordGenerator.Sets
{
    /// <summary>
    /// Represents a set of characters with usage constraints for password generation.
    /// </summary>
    public class CharacterSet : ICharacterSet
    {
        /// <summary>
        /// Gets or sets the array of characters in this set.
        /// </summary>
        public required char[] Set { get; set; }

        /// <summary>
        /// Gets or sets the original character string used to create this set.
        /// </summary>
        public required string Characters { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of characters from this set that must appear in generated passwords.
        /// </summary>
        public int Min { get; set; }
    }
}