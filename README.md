# NPWS-PasswordChecker
This tool uses the SDK/API of [Netwrix Password Secure](https://www.netwrix.com/enterprise_password_management_software.html) to connect to their service. With this, all passwords accessible by the logged in user are loaded. After they are loaded, the secrets are checked for security, taking the most common risks into account.

> [!NOTE]  
> You can use the project only if your Password Secure license contains the SDK/API module.

## Contribution
To contribute to the development of this project, take a look at the [Contribution guidelines for this project](CONTRIBUTING.md).

## Security checks
### Quality
Currently, the quality calculation is done with the algorithm provided by Netwrix. But I implemented different boundaries for the ratings (Weak, Good, Strong), ignoring the settings that can be set in the solution.

### Duplicates
The program checks if the same secret is used multiple times. If so, the integrity of the password is compromised. So it is suggested to change all of them to unique values.

### Have I Been Pwned
The NPWS-PasswordChecker uses the [Have I Been Pwned API](https://haveibeenpwned.com/) to check if each secret is existing in any data breaches. If a match was found, the software raises an alarm and it's highly recommended to change the password.

#### How is the check against their API working?
The most important point to mention is the following: Your password **NEVER** leaves your computer! For the check, the value of the password field is hashed client-side with SHA1. From this hash, the first 5 characters are transferred to Have I Been Pwned. As a result, we recive all known hashes starting with the passed chars and additionally the number of data breaches. The result is analyzed client-side then.


## Other functions
### Ignore fields
In case you have stored information that should not be checked, you can simply configure the field to be ignored. When using the software for the first time, it is pre-configured to ignore PIN fields, as usually they have a bad quality. The detection of fields to ignore is done by the name of the field. You can add them by using [Regular Expression](https://en.wikipedia.org/wiki/Regular_expression)s. The default value for ignoring PINs is the following:
```
^(.*)PIN(.*)$
```

### PDF export
The result can be saved as PDF report. It contains all relevant security information detected above.
> [!IMPORTANT]  
> No clear-text value of any password is exported into the PDF file.

## License
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

The content of this project is licensed under the MIT license. I am not allowed and not able to license the binaries of Syncfusion as well as the Netwrix Password Secure SDK/API as part of this project!
