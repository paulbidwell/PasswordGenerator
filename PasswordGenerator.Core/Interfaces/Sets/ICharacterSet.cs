namespace PasswordGenerator.Core.Interfaces.Sets;

public interface ICharacterSet
{
    char[] Set { get; set; }
    string Characters { get; set; }
    int Min { get; set; }
}