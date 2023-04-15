namespace PasswordGenerator.Interfaces;

public interface ICharacterSet
{
    char[] Set { get; set; }
    string Characters { get; set; }
    int Min { get; set; }
}