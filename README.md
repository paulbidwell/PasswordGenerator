# Password Generator

This is a password generator I have created for my own use, but also decided to share.\
The aim is that the application will generate a strong and secure password.\
\
This application may have issues\\bugs and will always be in development. Use at your own risk.

## Usage

The application is a .net 7 console application. All settings are configurable in appsettings.json, as below.

```json
{
  "PasswordGenerator": {
    "AllowSequences": false,
    "AllowUpperLowerSequences": false,
    "CharactersSets": [
      {
        "Characters": "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
        "Min": 5
      },
      {
        "Characters": "abcdefghijklmnopqrstuvwxyz",
        "Min": 5
      },
      {
        "Characters": "0123456789",
        "Min": 2
      },
      {
        "Characters": "!£$%^&*()-_=+[]{}@#~",
        "Min": 2
      }
    ],
    "Length": 20,
    "MaxRepetition": 1,
    "OutputPath": "d:\\passwords.txt",
    "OutputToConsole": true,
    "OutputToFile": true,
    "PasswordsToGenerate": 1000000
  }
}
```

### Configurable Options
**AllowSequences - Boolean**\
Enables or disables sequences appearing in a generated password, for example "aa", "33"

**AllowUpperLowerSequences - Boolean**\
If this is enabled, it will allow sequences of any case, only when 'AllowSequences' is enabled, for example "Aa" "zZ"

**CharactersSets - Array (String, Int)**\
This is were you can define the character sets to be used in generating a password. There must be at least 1 set.\
Each set will have a string of characters and the minimum amount of times a character from that set should appear.\
If the sum of the minimum amounts is more than the set length of the generated password, an exception will occur.

**Length - Int**\
Determines the length of the generated password that is output.

**MaxRepetition - Int**\
This determines the amount of times any character can be repeated in the generated password.\
Set to -1 to allow for unlimited repetition.

**OutputPath - String**\
The path and filename all generated passwords will be dumped too if 'OutputToFile' is enabled.

**OutputToConsole - Boolean**\
If enabled, the generated password(s) will display on the console as they are generated.

**OutputToFile - Boolean**\
If enabled, the generated password(s) will be output to a file, which can be determined using the 'OutputPath' option.

**PasswordsToGenerate - Int**\
The amount of passwords that will be generated.