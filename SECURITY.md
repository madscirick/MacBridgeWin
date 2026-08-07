# Security policy

## Supported versions

Security fixes are applied to the latest code on the default branch.

## Reporting a vulnerability

Please do not publish suspected vulnerabilities in a public issue.

Use **Security → Advisories → Report a vulnerability** in the GitHub repository. This creates a private discussion with the maintainers. If private vulnerability reporting is not available, use the private contact method listed on the maintainer's GitHub profile.

Include the affected version, Windows version, reproduction steps, expected impact, and whether the affected application was elevated. Do not include secrets, private configuration files, or unreviewed logs. You should receive an acknowledgement within seven days. No particular resolution timeline is guaranteed for this volunteer-maintained project, but confirmed issues will be prioritized by severity.

Because MacBridgeWin uses global input hooks, reports involving unexpected input capture, privilege boundaries, or synthetic input should include Windows version and whether the affected application is elevated.

## Release integrity

Official release archives are attached to GitHub Releases and include a SHA-256 checksum file. Release executables are not currently code-signed; verify the repository and checksum before running them. Never disable Windows security software to install MacBridgeWin.
